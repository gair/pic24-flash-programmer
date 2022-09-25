using System.Text;

namespace HexParser
{
    public class IntelHex
    {
        private readonly List<Segment> segments = new();
        private uint linearAddress, segmentAddress, filledAddress;
        private int segmentIndex, segmentPos;
        private bool initialized;

        public void Open(string fileName, int pageSize = 256)
        {
            this.linearAddress = 0;
            this.segmentAddress = 0;
            this.filledAddress = 0;
            this.initialized = false;
            PageSize = pageSize;
            this.segments.Clear();
            this.segments.Add(new Segment());
            TotalPages = 0;
            Rewind();

            foreach (var line in File.ReadLines(fileName))
            {
                if (line.StartsWith(":"))
                {
                    ProcessLine(line);
                }
            }

            foreach (var segment in this.segments)
            {
                segment.TotalPages = segment.Data.Count / pageSize;
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
                    // pad the last page to be on a page boundary
                    while (this.segments.Last().Data.Count % PageSize != 0)
                    {
                        this.segments.Last().Data.Add(0xff);
                    }
                    break;
                case RecordType.Data:
                    length = GetRecordLength(line);
                    address = this.linearAddress + this.segmentAddress + GetLoadOffset(line);
                    if (!this.initialized)
                    {
                        this.initialized = true;
                        this.filledAddress = address;
                        this.segments.Last().StartAddress = address;
                    }
                    FillAddressGaps(address);
                    this.filledAddress += length;
                    var i = (int)RecordIndex.Data;
                    while (length > 0)
                    {
                        var data = Convert.ToByte(line.Substring(i, 2), 16);
                        this.segments.Last().Data.Add(data);
                        i += 2;
                        length--;
                        address++;
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
                throw new InvalidDataException("Checksum fail");
            }
        }

        private void FillAddressGaps(uint address)
        {
            if (address != this.filledAddress)
            {
                var pageBoundary = (uint)(PageSize - 1);
                var nextPageAddress = this.filledAddress + pageBoundary;
                nextPageAddress &= ~pageBoundary;

                // Fill up to address or nextPageAddress, which ever comes first.
                for (; (this.filledAddress < nextPageAddress) && (this.filledAddress < address); this.filledAddress++)
                {
                    this.segments.Last().Data.Add(0xff);
                }

                // if address is not page aligned, set nextPageAddress to the preceeding page boundary of address.
                nextPageAddress = address & ~pageBoundary;

                // check for a jump in pages
                if ((address - this.filledAddress) >= PageSize)
                {
                    this.segments.Add(new Segment());
                    this.segments.Last().StartAddress = nextPageAddress;
                    this.filledAddress = nextPageAddress;
                }

                // check if address is page aligned and fill if not
                for (; this.filledAddress < address; this.filledAddress++)
                {
                    this.segments.Last().Data.Add(0xff);
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

        public (uint address, byte[] data) ReadPage(bool readAllSegments = true)
        {
            var bytesAvailable = this.segments[this.segmentIndex].Data.Count - this.segmentPos;

            if (bytesAvailable == 0 && readAllSegments)
            {
                if (this.segmentIndex < this.segments.Count - 1)
                {
                    this.segmentIndex++;
                    this.segmentPos = 0;
                    bytesAvailable = this.segments[this.segmentIndex].Data.Count - this.segmentPos;
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

        public string ReadPageHex(bool readAllSegments = true)
        {
            var sb = new StringBuilder();
            var n = 0;
            var (address, data) = ReadPage(readAllSegments);
            var len = data.Length;
            if (len == 0)
            {
                return string.Empty;
            }

            for (var i = 0; i < PageSize; i++)
            {
                if (n++ % 4 == 0)
                {
                    sb.Append(' ');
                }
                byte b = 0xff;
                if (i < len)
                {
                    b = data[i];
                }

                sb.Append($"{b:X2}");
            }
            return $"{address:X8}{sb}";
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

        public int SelectNextSegment()
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
            public uint StartAddress { get; set; }

            public int TotalPages { get; set; }

            public List<byte> Data { get; } = new List<byte>();
        }
    }

    public class ParseException : Exception
    {
        public ParseException(string? message) : base(message)
        {
        }
    }
}
