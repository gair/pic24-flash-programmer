using System.Globalization;

namespace PIC24FlashProgrammer
{
    internal class Configuration
    {
        private const string RegKeyWindowLeft = "WinLeft";
        private const string RegKeyWindowTop = "WinTop";
        private const string RegKeyWindowWidth = "WinWidth";
        private const string RegKeyWindowHeight = "WinHeight";
        private const string RegKeyWindowMaxed = "WinMaxed";
        private const string RegKeyBaudRate = "BaudRate";
        private const string RegKeySerialPort = "SerialPort";
        private const string RegKeyProgExec = "ProgExec";
        private const string RegKeyFlashFile = "FlashFile";
        private const string RegKeyBlankSize = "BlankSize";
        private const string RegKeyWordAddress = "WordAddr";
        private const string RegKeyPageAddress = "PageAddr";
        private const string RegKeyBlockAddress = "BlockAddr";

        public static Size WindowSize
        {
            get => GetWindowSize();
            set => SetWindowSize(value);
        }

        public static Point WindowLocation
        {
            get => GetWindowLocation();
            set => SetWindowLocation(value);
        }

        private static Point GetWindowLocation()
        {
            var x = GetRegistryItem(RegKeyWindowLeft, -1);
            var y = GetRegistryItem(RegKeyWindowTop, -1);
            return new Point(x, y);
        }

        private static void SetWindowLocation(Point point)
        {
            SetRegistryItem(RegKeyWindowLeft, point.X < 0 ? 0 : point.X);
            SetRegistryItem(RegKeyWindowTop, point.Y < 0 ? 0 : point.Y);
        }

        private static Size GetWindowSize()
        {
            var w = GetRegistryItem(RegKeyWindowWidth, 0);
            var h = GetRegistryItem(RegKeyWindowHeight, 0);
            return new Size(w, h);
        }

        private static void SetWindowSize(Size size)
        {
            SetRegistryItem(RegKeyWindowWidth, size.Width);
            SetRegistryItem(RegKeyWindowHeight, size.Height);
        }

        public static bool WindowMaxed
        {
            get => GetRegistryItem(RegKeyWindowMaxed, false);
            set => SetRegistryItem(RegKeyWindowMaxed, value);
        }

        public static int BaudRate
        {
            get => GetRegistryItem(RegKeyBaudRate, 9600);
            set => SetRegistryItem(RegKeyBaudRate, value);
        }

        public static string? SerialPort
        {
            get => GetRegistryItem<string>(RegKeySerialPort, null);
            set => SetRegistryItem(RegKeySerialPort, value);
        }

        public static string? ProgrammingExecutive
        {
            get => GetRegistryItem<string>(RegKeyProgExec, null);
            set => SetRegistryItem(RegKeyProgExec, value);
        }

        public static string? ApplicationFile
        {
            get => GetRegistryItem<string>(RegKeyFlashFile, null);
            set => SetRegistryItem(RegKeyFlashFile, value);
        }

        public static uint WordAddress
        {
            get => GetRegistryItem(RegKeyWordAddress, 0xFF0000U);
            set => SetRegistryItem(RegKeyWordAddress, value);
        }

        public static uint BlankSize
        {
            get => GetRegistryItem(RegKeyBlankSize, 0x5600U);
            set => SetRegistryItem(RegKeyBlankSize, value);
        }

        public static uint PageAddress
        {
            get => GetRegistryItem(RegKeyPageAddress, 0U);
            set => SetRegistryItem(RegKeyPageAddress, value);
        }

        public static uint BlockAddress
        {
            get => GetRegistryItem(RegKeyBlockAddress, 0U);
            set => SetRegistryItem(RegKeyBlockAddress, value);
        }

        private static T? GetRegistryItem<T>(string keyName, T? defaultValue)
        {
            try
            {
                return Application.UserAppDataRegistry.GetValue(keyName) is string value ? (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture) : defaultValue;
            }
            catch (FormatException)
            {
                SetRegistryItem(keyName, defaultValue);
                return defaultValue;
            }
        }

        private static void SetRegistryItem<T>(string keyName, T value)
        {
            if (value == null)
            {
                Application.UserAppDataRegistry.DeleteValue(keyName);
            }
            Application.UserAppDataRegistry.SetValue(keyName, $"{value}");
        }

    }
}
