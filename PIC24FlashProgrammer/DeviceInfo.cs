using System.Text;

namespace PIC24FlashProgrammer
{
    internal class DeviceInfo
    {
        public int ID { get; set; }

        public uint AddressLimit { get; set; }
        public uint CW1 => AddressLimit;
        public uint CW2 => AddressLimit - 2;
        public uint CW3 => AddressLimit - 4;
        public uint CW4 => AddressLimit - 6;

        public string? Model { get; set; }

        public string ShowCW1(int data)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Register CW1:");
            sb.AppendLine($"JTAG port: {((data & 1 << 14) != 0 ? "Enabled" : "Disabled")}");
            sb.AppendLine($"General segment code-protect: {((data & 1 << 13) == 0 ? "Enabled" : "Disabled")}");
            sb.AppendLine($"General segment write-protect:  {((data & 1 << 12) == 0 ? "Enabled" : "Disabled")}");
            sb.AppendLine($"Background debugger: {((data & 1 << 11) == 0 ? "Reset into debug mode" : "Reset in operational mode")}");
            var pin = (data & 0x0300) >> 8;
            pin = pin == 3 ? 1 : pin == 1 ? 3 : pin;
            sb.AppendLine($"ICD emulator pin placement: PGEC{pin}/PGED{pin}");
            sb.AppendLine($"Watchdog timer: {((data & 1 << 7) != 0 ? "Enabled" : "Disabled")}");
            sb.AppendLine($"Windowed watchdog timer: {((data & 1 << 6) != 0 ? "Standard" : "Windowed")} watchdog enabled");
            sb.AppendLine($"Watchdog timer prescaler ratio: {((data & 1 << 4) == 0 ? "1:32" : "1:128")}");
            sb.AppendLine($"Watchdog timer postscaler: 1:{1 << (data & 0xf)}");
            return sb.ToString();
        }

        public string ShowCW2(int data)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Register CW2:");
            sb.AppendLine($"Internal external switchover two speed startup: {((data & 1 << 15) != 0 ? "Enabled" : "Disabled")}");
            var mode = (data & 0x7000) >> 12;
            var values = new int[] { 1, 2, 3, 4, 5, 6, 8, 12 };
            sb.AppendLine($"USB 96 MHz prescaler: Input divided by {values[mode]} ({values[mode] * 4} MHz input)");
            sb.AppendLine($"USB 96 MHz startup: Enabled {((data & 1 << 11) == 0 ? "in software (with PLLEN bit CLKDIV<5>)" : "automatically on start-up")}");
            mode = (data & 0x0700) >> 8;
            var options = new string[] {
                "FRC", "FRC with PLL", "Primary (XT, HS, EC)",
                "Primary (XTPLL, HSPLL, ECPLL) with PLL", "Secondary (SOSC)",
                "Low power RC (LPRC)", "Reserved", "Internal fast RC (FRCDIV) with postscaler" };
            sb.AppendLine($"Initial oscillator select: {options[mode]}");
            mode = (data & 0xC0) >> 6;
            sb.AppendLine($"Clock switching: {(mode == 0 ? "Enabled, monitor enabled" : mode == 1 ? "Enabled, monitor disabled" : "Disabled, monitor disabled")}");
            sb.AppendLine($"OSCO pin function: {((data & 1 << 5) == 0 ? "Digital port I/O (RC15)" : "Clock output (Fosc/2)")}");
            sb.AppendLine($"IOLOCK one-way set: {((data & 1 << 4) == 0 ? "Can be set and cleared as needed" : "Can only be set once")}");
            sb.AppendLine($"I2C1 pin select: {((data & 1 << 2) == 0 ? "Alternate" : "Default")} SCL1/SDA1 pins");
            mode = data & 3;
            sb.AppendLine($"Primary oscillator: {(mode == 3 ? "Disabled" : mode == 2 ? "HS crystal" : mode == 1 ? "XT crystal" : "EC (external clock)")}");
            return sb.ToString();
        }

        public string ShowCW3(int data)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Register CW3:");
            sb.AppendLine($"Segment write protect end page: {((data & 1 << 15) == 0 ? "Lower" : "Upper")} boundary 0x{(data & 0x1f):X3}");
            sb.AppendLine($"Configuration word code protect: Last page and configuration words are {((data & 1 << 14) == 0 ? "protected" : "not protected")}");
            sb.AppendLine($"Segment write protection disable: Write protection {((data & 1 << 13) == 0 ? "enabled" : "disabled")}");
            var mode = (data & 0xC00) >> 10;
            sb.AppendLine($"Voltage regulator standby wake-up time: {(mode == 3 ? "Default start-up" : mode == 2 ? "Fast start-up" : "Reserved; do not use")}");
            mode = (data & 0x300) >> 8;
            sb.AppendLine($"Secondary oscillator power: {(mode == 3 ? "Default (high drive strength) SOSC" : mode == 1 ? "Low power (low drive strength) SOSC" : mode == 0 ? "External clock input (SCLKI); digital I/O (RA4, RB4)" : "Reserved")}");
            return sb.ToString();
        }

        public string ShowCW4(int data)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Register CW4:");
            sb.AppendLine($"Deep sleep watchdog timer: {((data & 1 << 7) != 0 ? "Enabled" : "Disabled")}");
            sb.AppendLine($"Deep sleep BOR enable: {((data & 1 << 6) != 0 ? "Enabled" : "Disabled")}");
            sb.AppendLine($"RTCC reference clock: {((data & 1 << 5) != 0 ? "SOSC" : "LPRC")}");
            sb.AppendLine($"Deep sleep watchdog timer clock: DSWDT uses {((data & 1 << 4) != 0 ? "LPRC" : "SOSC")}");
            var options = new string[] {
                "1:2 (2.1 ms)", "1:8 (8.3 ms)", "1:32 (33 ms)","1:128 (132 ms)","1:512 (528 ms)", "1:512 (528 ms)","1:8,192 (8.5 seconds)",
                "1:32,768 (34 seconds)","1:32,768 (34 seconds)","1:524,288 (9 minutes)", "1:2,097,152 (36 minutes)","1:8,388,608 (2.4 hours)",
                "1:8,388,608 (2.4 hours)","1:8,388,608 (2.4 hours)","1:536,870,912 (6.4 days)","1:2,147,483,648 (25.7 days)"
            };
            var mode = data & 0xf;
            sb.AppendLine($"DSWDT postscaler select: {options[mode]}");
            return sb.ToString();
        }
    }
}
