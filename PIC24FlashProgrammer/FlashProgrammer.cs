using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using HexParser;

namespace PIC24FlashProgrammer
{
    internal enum MessageType
    {
        DeviceUpdate,
        VersionUpdate,
        StatusMessage
    }

    internal enum ProgrammingMode
    {
        None,
        ICSP,
        EICSP
    }

    internal class MessageEventArgs : EventArgs
    {
        public MessageType Type { get; }
        public string? Message { get; }
        public DeviceInfo? Info { get; }

        public MessageEventArgs(MessageType type, string? message, DeviceInfo? info = null)
        {
            Type = type;
            Message = message;
            Info = info;
        }
    }

    internal class ModeChangedEventArgs : EventArgs
    {
        public ProgrammingMode Mode { get; }

        public ModeChangedEventArgs(ProgrammingMode mode)
        {
            Mode = mode;
        }
    }

    internal class DebugChangedEventArgs : EventArgs
    {
        public bool Enabled { get; }

        public DebugChangedEventArgs(bool enabled)
        {
            Enabled = enabled;
        }
    }

    internal class FlashProgrammer
    {
        public event EventHandler<MessageEventArgs>? UpdateMessage;
        public event EventHandler<ModeChangedEventArgs>? ModeChanged;
        public event EventHandler<DebugChangedEventArgs>? DebugChanged;
        public event EventHandler<EventArgs>? Closed;

        private const int LoadedExecAppID = 0xCB;
        private readonly AutoResetEvent serialEvent = new(false);
        private readonly StringBuilder serialBuffer = new();
        private string serialData = string.Empty;
        private string injectedCommand = string.Empty;
        private volatile bool closing = false;
        private SerialPort? serialPort;
        private ProgrammingMode programmingMode;
        private bool debugMode;

        public ProgrammingMode SerialProgrammingMode
        {
            get => this.programmingMode;
            private set
            {
                this.programmingMode = value;
                ModeChanged?.Invoke(this, new ModeChangedEventArgs(value));
            }
        }

        public bool DebugMode
        {
            get => this.debugMode;
            private set
            {
                this.debugMode = value;
                DebugChanged?.Invoke(this, new DebugChangedEventArgs(value));
            }
        }

        public string? ApplicationFile { get; set; }
        public string? ProgrammingExecutiveFile { get; set; }
        public Dictionary<int, string>? DeviceTypeMap { get; set; }
        public bool IsOpen => this.serialPort?.IsOpen ?? false;
        public bool IsExecLoaded { get; set; }

        public void Open(string port, int baudRate)
        {
            Close();
            this.serialPort = new SerialPort(port, baudRate);
            this.serialPort.DataReceived += SerialPort_DataReceived;
            this.serialPort.ErrorReceived += SerialPort_ErrorReceived;
            this.serialPort.Open();
            InjectSerialCommand(SerialCommand.ExitICSP);
            Task.Run(() => SerialMonitor(this.serialPort));
        }

        public void Close()
        {
            this.closing = true;
            _ = this.serialEvent.Set();
            if (this.serialPort?.IsOpen ?? false)
            {
                this.serialPort.Close();
            }
        }

        private void NotifyStatus(string message)
        {
            UpdateMessage?.Invoke(this, new MessageEventArgs(MessageType.StatusMessage, message));
        }

        private void NotifyDeviceUpdate(DeviceInfo info)
        {
            UpdateMessage?.Invoke(this, new MessageEventArgs(MessageType.DeviceUpdate, null, info));
        }

        private void NotifyVersionUpdate(string version)
        {
            UpdateMessage?.Invoke(this, new MessageEventArgs(MessageType.VersionUpdate, version));
        }

        private void SerialMonitor(SerialPort serialPort)
        {
            var data = 0.0;
            var deviceInfo = new DeviceInfo();
            var hexFile = new IntelHex();
            this.closing = false;
            while (!this.closing)
            {
                _ = this.serialEvent.WaitOne();
                if (this.closing)
                {
                    return;
                }
                if (this.serialData.StartsWith(SerialResponse.DataOutput))
                {
                    var value = this.serialData[SerialResponse.DataOutput.Length..];
                    Debug.WriteLine(value);
                    var ok = value.StartsWith("0x")
                        ? double.TryParse(value, System.Globalization.NumberStyles.HexNumber, null, out data)
                        : double.TryParse(value, out data);
                    if (!ok)
                    {
                        NotifyStatus($"Device sent invalid data: {this.serialData}");
                    }
                }
                else if (this.serialData.StartsWith(SerialResponse.TextOutput))
                {
                    _ = this.serialData[SerialResponse.TextOutput.Length..];
                }
                else
                {
                    string? fileName;
                    switch (this.serialData)
                    {
                        case SerialResponse.CommandInjected:
                            serialPort.WriteLine(this.injectedCommand);
                            break;
                        case "START":
                            serialPort.WriteLine(SerialCommand.Start);
                            break;
                        case SerialResponse.Reset:
                            NotifyStatus("Arduino was reset");
                            SerialProgrammingMode = ProgrammingMode.None;
                            break;
                        case SerialResponse.EnteredICSP:
                            NotifyStatus("Entered ICSP mode");
                            SerialProgrammingMode = ProgrammingMode.ICSP;
                            serialPort.WriteLine(SerialCommand.RequestDeviceID);
                            break;
                        case SerialResponse.EnteredEICSP:
                            NotifyStatus("Entered EICSP mode");
                            SerialProgrammingMode = ProgrammingMode.EICSP;
                            serialPort.WriteLine(SerialCommand.RequestExecutiveVersion);
                            break;
                        case SerialResponse.DebugModeOn:
                            NotifyStatus("Debug mode enabled");
                            DebugMode = true;
                            break;
                        case SerialResponse.DebugModeOff:
                            NotifyStatus("Debug mode disabled");
                            DebugMode = false;
                            break;
                        case SerialResponse.SendApplication:
                            NotifyStatus($"Loading application...");
                            if (!File.Exists(ApplicationFile))
                            {
                                NotifyStatus($"Application file not found. {ApplicationFile}");
                                break;
                            }
                            hexFile.Open(ApplicationFile);
                            serialPort.WriteLine(SerialCommand.LoadApplication);
                            break;
                        case SerialResponse.DeviceID:
                            deviceInfo.ID = (int)data;
                            if (DeviceTypeMap != null && DeviceTypeMap.ContainsKey(deviceInfo.ID))
                            {
                                deviceInfo.Model = DeviceTypeMap[deviceInfo.ID];
                            }
                            NotifyDeviceUpdate(deviceInfo);
                            break;
                        case SerialResponse.AppID:
                            IsExecLoaded = (int)data == LoadedExecAppID;
                            NotifyStatus($"Application ID is 0x{(int)data:X2}, programming executive {(IsExecLoaded ? "is" : "needs to be")} loaded");
                            break;
                        case SerialResponse.ExitedICSP:
                            NotifyStatus("Exited ICSP mode");
                            SerialProgrammingMode = ProgrammingMode.None;
                            break;
                        case SerialResponse.SendExecutive:
                            NotifyStatus("Loading programming executive");
                            fileName = ProgrammingExecutiveFile;
                            if (!string.IsNullOrEmpty(fileName))
                            {
                                hexFile.Open(fileName);
                                serialPort.WriteLine(SerialCommand.LoadProgrammingExecutive);
                            }
                            break;
                        case SerialResponse.ExecutiveLoaded:
                            NotifyStatus("Programming executive loaded, verifying...");
                            fileName = ProgrammingExecutiveFile;
                            if (!string.IsNullOrEmpty(fileName))
                            {
                                hexFile.Open(fileName);
                                serialPort.WriteLine(SerialCommand.VerifyProgrammingExecutive);
                            }
                            break;
                        case SerialResponse.ExecutiveVerified:
                            NotifyStatus("Programming executive verified");
                            break;
                        case SerialResponse.ExecutiveVersion:
                            NotifyVersionUpdate(data.ToString("0.0"));
                            break;
                        case SerialResponse.ApplicationLoaded:
                            NotifyStatus("Application loaded, verifying...");
                            hexFile.Open(ApplicationFile);
                            serialPort.WriteLine(SerialCommand.VerifyApplication);
                            break;
                        case SerialResponse.ApplicationVerified:
                            NotifyStatus("Application verified");
                            break;
                        case SerialResponse.SendNextPage:
                            NotifyStatus("Sending page...");
                            var page = hexFile.ReadPageHex();
                            if (string.IsNullOrEmpty(page))
                            {
                                NotifyStatus("All pages sent");
                                serialPort.WriteLine(SerialCommand.NoMorePages);
                                break;
                            }
                            serialPort.WriteLine(page);
                            break;
                        case SerialResponse.Error:
                            NotifyStatus("Error");
                            break;

                        default:
                            NotifyStatus($"{this.serialData}");
                            break;
                    }
                }
            }
            Closed?.Invoke(this, EventArgs.Empty);
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            lock (this.serialEvent)
            {
                if (sender is SerialPort serialPort)
                {
                    _ = this.serialBuffer.Append(serialPort.ReadExisting());
                    if (this.serialBuffer[^1] == '\n')
                    {
                        this.serialData = this.serialBuffer.ToString().TrimEnd();
                        _ = this.serialBuffer.Clear();
                        _ = this.serialEvent.Set();
                    }
                }
            }
        }

        private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {

        }

        private void InjectSerialCommand(string command)
        {
            lock (this.serialEvent)
            {
                this.injectedCommand = command;
                this.serialData = SerialResponse.CommandInjected;
                _ = this.serialEvent.Set();
            }
        }

        public void RequestExecVersion()
        {
            InjectSerialCommand(SerialCommand.RequestExecutiveVersion);
        }

        public void RequestEnterICSP()
        {
            InjectSerialCommand(SerialCommand.EnterICSP);
        }
        public void RequestEnterEICSP()
        {
            if(!IsExecLoaded)
            {
                NotifyStatus($"Enhanced ICSP will fail if the programming executive is not loaded. Check the application ID on fail.");
            }
            InjectSerialCommand(SerialCommand.EnterEICSP);
        }

        public void RequestExitICSP()
        {
            InjectSerialCommand(SerialCommand.ExitICSP);
        }

        public void RequestApplicationID()
        {
            InjectSerialCommand(SerialCommand.RequestAppID);
        }

        public void EnableDebug(bool enable)
        {
            InjectSerialCommand(enable ? SerialCommand.DebugOn : SerialCommand.DebugOff);
        }
    }

    internal class SerialCommand
    {
        public const string Start = "Start";
        public const string RequestExecutiveVersion = "ExecVersion";
        public const string RequestDeviceID = "DeviceID";
        public const string RequestAppID = "AppID";
        public const string EnterICSP = "EnterICSP";
        public const string EnterEICSP = "EnterEICSP";
        public const string ExitICSP = "ExitICSP";
        public const string LoadProgrammingExecutive = "LoadExec";
        public const string LoadApplication = "LoadApp";
        public const string VerifyProgrammingExecutive = "VerifyExec";
        public const string VerifyApplication = "VerifyApp";
        public const string NoMorePages = "Done";
        public const string DebugOn = "DebugOn";
        public const string DebugOff = "DebugOff";
    }

    internal class SerialResponse
    {
        public const string Error = "ERROR";
        public const string Reset = "RESET";
        public const string DataOutput = "DATA_OUT:";
        public const string TextOutput = "TEXT_OUT:";
        public const string EnteredICSP = "ENTERED_ICSP";
        public const string EnteredEICSP = "ENTERED_EICSP";
        public const string ExitedICSP = "EXITED_ICSP";
        public const string SendNextPage = "SEND_PAGE";
        public const string SendExecutive = "SEND_EXEC";
        public const string SendApplication = "SEND_APP";
        public const string ExecutiveLoaded = "EXEC_LOADED";
        public const string ExecutiveVerified = "EXEC_VERIFIED";
        public const string ExecutiveVersion = "EXEC_VERSION";
        public const string ApplicationLoaded = "APP_LOADED";
        public const string ApplicationVerified = "APP_VERIFIED";
        public const string DeviceID = "DEVICE_ID";
        public const string AppID = "APP_ID";
        public const string DebugModeOn = "DEBUG_ON";
        public const string DebugModeOff = "DEBUG_OFF";
        public const string CommandInjected = "COMMAND_INJECTED";
    }
}
