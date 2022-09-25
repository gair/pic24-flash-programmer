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
