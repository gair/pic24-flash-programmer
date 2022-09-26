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

        public static string? BlankSize
        {
            get => GetRegistryItem(RegKeyBlankSize, "0x5600");
            set => SetRegistryItem(RegKeyBlankSize, value);
        }

        public static string? WordAddress
        {
            get => GetRegistryItem(RegKeyWordAddress, "0xFF0000");
            set => SetRegistryItem(RegKeyWordAddress, value);
        }

        private static T? GetRegistryItem<T>(string keyName, T? defaultValue)
        {
            return Application.UserAppDataRegistry.GetValue(keyName) is string value ? (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture) : defaultValue;
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
