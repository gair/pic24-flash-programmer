using System.Data;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace HexParser
{
    public class IntelHex
    {
        private readonly List<Segment> segments = new();
        private uint linearAddress, segmentAddress, currentAddress;
        private int segmentIndex, segmentPos;
        private bool initialized;

        public void Open(string fileName, int pageSize)
        {
            this.linearAddress = 0;
            this.segmentAddress = 0;
            this.currentAddress = 0;
            this.initialized = false;
            this.segments.Clear();
            this.segments.Add(new Segment(pageSize));
            PageSize = pageSize;
            TotalPages = 0;
            Rewind();

            foreach (var line in File.ReadLines(fileName))
            {
                if (line.StartsWith(":"))
                {
                    ProcessLine(line);
                }
            }

            this.segments.Sort(new Comparison<Segment>((a, b) =>
            {
                return (int)a.StartAddress - (int)b.StartAddress;
            }));

            AlignPages();
            FillGaps();
            MergeSegments();

            foreach (var segment in this.segments)
            {
                segment.Fill();
                TotalPages += segment.TotalPages;
            }
        }

        private void ProcessLine(string line)
        {
            uint address, length;
            Checksum(line);
            switch (GetRecordType(line))
            {
                case RecordType.ExtendedLinearAddress:
                    this.linearAddress = GetLinearAddress(line);
                    break;
                case RecordType.ExtendedSegmentAddress:
                    this.segmentAddress = GetSegmentAddress(line);
                    break;
                case RecordType.StartLinearAddress:
                case RecordType.EndOfFile:
                    break;
                case RecordType.Data:
                    length = GetRecordLength(line);
                    address = this.linearAddress + this.segmentAddress + GetLoadOffset(line);

                    if (!this.initialized)
                    {
                        this.initialized = true;
                        this.currentAddress = address;
                        this.segments.Last().StartAddress = address;
                    }
                    if (address != this.currentAddress)
                    {
                        this.segments.Add(new Segment(PageSize));
                        this.segments.Last().StartAddress = address;
                        this.currentAddress = address;
                    }
                    this.currentAddress += length;
                    var i = (int)RecordIndex.Data;
                    var d = this.segments.Last().Data;
                    while (length-- > 0)
                    {
                        d.Add(Convert.ToByte(line.Substring(i, 2), 16));
                        i += 2;
                    }
                    break;
                case RecordType.StartSegmentAddress:
                    throw new ParseException("Start segment address record is not supported");
            }
        }

        private static void Checksum(string line)
        {
            byte cs = 0;
            for (var i = 1; i < line.Length; i += 2)
            {
                cs += Convert.ToByte(line.Substring(i, 2), 16);
            }
            if (cs != 0)
            {
                throw new ParseException("Checksum fail");
            }
        }

        private void MergeSegments()
        {
            var done = false;
            while (!done)
            {
                done = true;
                for (var i = 0; i < this.segments.Count - 1; i++)
                {
                    if (this.segments[i].StartAddress + this.segments[i].Data.Count == this.segments[i + 1].StartAddress)
                    {
                        this.segments[i].Data.AddRange(this.segments[i + 1].Data);
                        this.segments.RemoveAt(i + 1);
                        done = false;
                        break;
                    } else if (this.segments[i].StartAddress + this.segments[i].Data.Count > this.segments[i + 1].StartAddress)
                    {
                        var overlap = (this.segments[i].StartAddress + this.segments[i].Data.Count) - this.segments[i + 1].StartAddress;
                        var i1 = this.segments[i].Data.Count - (int)overlap;
                        for (var n = 0; n < overlap; n++)
                        {
                            var b = this.segments[i + 1].Data[n];
                            if (b != 0xff)
                            {
                                this.segments[i].Data[i1 + n] = b;
                            }
                        }
                        this.segments[i].Data.AddRange(this.segments[i + 1].Data.Skip((int)overlap));
                        this.segments.RemoveAt(i + 1);
                        done = false;
                        break;
                    }
                }
            }
        }

        private void FillGaps()
        {

            for (var i = 0; i < this.segments.Count - 1; i++)
            {
                var gap = this.segments[i + 1].StartAddress - (this.segments[i].StartAddress + this.segments[i].Data.Count);
                if (gap < PageSize)
                {
                    for (var n = 0; n < gap; n++)
                    {
                        this.segments[i].Data.Add(0xff);
                    }
                }
            }
        }

        private void AlignPages()
        {
            var boundaryMask = ~(uint)(PageSize - 1);
            for (var i = 0; i < this.segments.Count; i++)
            {
                var offset = this.segments[i].StartAddress - (this.segments[i].StartAddress & boundaryMask);
                if (offset > 0)
                {
                    var padding = Enumerable.Repeat((byte)0xff, (int)offset).ToList();
                    this.segments[i].Data.InsertRange(0, padding);
                    this.segments[i].StartAddress -= offset;
                }
            }
        }

        private static RecordType GetRecordType(string line)
        {
            return (RecordType)Convert.ToByte(line.Substring((int)RecordIndex.RecordType, 2), 16);
        }

        private static uint GetRecordLength(string line)
        {
            return Convert.ToByte(line.Substring((int)RecordIndex.RecordLength, 2), 16);
        }

        private static uint GetLoadOffset(string line)
        {
            return Convert.ToUInt16(line.Substring((int)RecordIndex.LoadOffset, 4), 16);
        }

        private static uint GetSegmentAddress(string line)
        {
            return Convert.ToUInt16(line.Substring((int)RecordIndex.Data, 4), 16);
        }

        private static uint GetLinearAddress(string line)
        {
            return GetSegmentAddress(line) << 16;
        }

        public (uint address, byte[] data) ReadPage()
        {
            var bytesAvailable = this.segments[this.segmentIndex].Data.Count - this.segmentPos;

            if (bytesAvailable == 0)
            {
                if (this.segmentIndex < this.segments.Count - 1)
                {
                    this.segmentIndex++;
                    this.segmentPos = 0;
                    bytesAvailable = this.segments[this.segmentIndex].Data.Count;
                }
            }

            var address = this.segments[this.segmentIndex].StartAddress + (uint)this.segmentPos;

            if (bytesAvailable > PageSize)
            {
                bytesAvailable = PageSize;
            }
            var data = this.segments[this.segmentIndex].Data.Skip(this.segmentPos).Take(bytesAvailable).ToArray();

            this.segmentPos += bytesAvailable;
            return (address, data);
        }

        public string ReadPageHex()
        {
            StringBuilder sb = new();
            var n = 0;
            (var address, var data) = ReadPage();
            var len = data.Length;
            if (len == 0)
            {
                return string.Empty;
            }

            for (var i = 0; i < PageSize; i++)
            {
                if (n++ % 4 == 0)
                {
                    _ = sb.Append(' ');
                }
                byte b = 0xff;
                if (i < len)
                {
                    b = data[i];
                }

                _ = sb.Append($"{b:X2}");
            }
            return $"{address:X8}:{sb}";
        }

        public string ReadPage24Hex()
        {
            StringBuilder sb = new();
            StringBuilder sb1 = new();
            (var address, var data) = ReadPage();
            var len = data.Length;
            if (len == 0)
            {
                return string.Empty;
            }

            for (var i = 0; i < PageSize; i++)
            {
                if (sb1.Length == 8)
                {
                    _ = sb.Append(' ');
                    _ = sb.Append(ReverseBytes(sb1.ToString()));
                    _ = sb1.Clear();
                }
                byte b = 0xff;
                if (i < len)
                {
                    b = data[i];
                }

                _ = sb1.Append($"{b:X2}");
            }
            _ = sb.Append(' ');
            _ = sb.Append(ReverseBytes(sb1.ToString()));
            return $"{address >> 1:X6}:{sb}";
        }

        private static string ReverseBytes(string s)
        {
            return $"{s.Substring(4, 2)}{s.Substring(2, 2)}{s[..2]}";
        }

        public int SelectSegment(int segment)
        {
            if (segment < this.segments.Count - 1)
            {
                this.segmentIndex = segment;
                this.segmentPos = 0;
                return this.segmentIndex;
            }
            return -1;
        }

        public void Rewind()
        {
            this.segmentIndex = 0;
            this.segmentPos = 0;
        }

        public void RewindSegment()
        {
            this.segmentPos = 0;
        }

        public int NextSegment()
        {
            return SelectSegment(this.segmentIndex + 1);
        }

        public int TotalPages { get; private set; }

        public int TotalSegments => this.segments.Count;

        public int SegmentSize => this.segments[this.segmentIndex].Data.Count;

        public int SegmentPages => this.segments[this.segmentIndex].TotalPages;

        public int PageSize { get; private set; }

        private enum RecordIndex
        {
            RecordMark = 0,
            RecordLength = 1,
            LoadOffset = 3,
            RecordType = 7,
            Data = 9,
        }

        private enum RecordType
        {
            Data,
            EndOfFile,
            ExtendedSegmentAddress,
            StartSegmentAddress,
            ExtendedLinearAddress,
            StartLinearAddress
        }

        private class Segment
        {
            private readonly int pageSize;

            public Segment(int pageSize)
            {
                this.pageSize = pageSize;
            }

            public uint StartAddress { get; set; }

            public int TotalPages => Data.Count / this.pageSize;

            public List<byte> Data { get; } = new List<byte>();

            public void Fill()
            {
                while (Data.Count % pageSize != 0)
                {
                    Data.Add(0xff);
                }
            }
        }
    }

    public class ParseException : Exception
    {
        public ParseException(string? message) : base(message)
        {
        }
    }
}
