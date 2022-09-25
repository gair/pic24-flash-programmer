using System.Text.RegularExpressions;

namespace PIC24FlashProgrammer
{
    public class UsbDevice
    {
        private const string VidPidRegex = @"^USB\\VID_(?<vid>[0-9A-F]{4})&PID_(?<pid>[0-9A-F]{4})";

        public int PID { get; }
        public int VID { get; }
        public string ID { get; }
        public string Serial { get; }
        public string Caption { get; }
        public string Description { get; }
        public bool IsValid => PID != 0 && VID != 0;
        public bool IsConnected { get; }

        public UsbDevice(string deviceId, string description, string caption, bool connected)
        {
            ID = deviceId;
            Caption = caption;
            Description = description;
            Serial = string.Empty;
            IsConnected = connected;
            var rx = new Regex($@"{VidPidRegex}\\(?<serial>[\w&]+)");
            var m = rx.Match(deviceId);
            if (m.Success)
            {
                if (int.TryParse(m.Groups["vid"].Value, System.Globalization.NumberStyles.HexNumber, null, out var value))
                {
                    VID = value;
                }

                if (int.TryParse(m.Groups["pid"].Value, System.Globalization.NumberStyles.HexNumber, null, out value))
                {
                    PID = value;
                }

                Serial = m.Groups["serial"].Value;
            }
        }
    }
}
