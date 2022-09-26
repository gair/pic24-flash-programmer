using System.IO.Ports;
using System.Text;
using HexParser;

namespace PIC24FlashProgrammer
{
    internal class FlashProgrammer : IDisposable
    {
        public event EventHandler<MessageEventArgs>? UpdateMessage;
        public event EventHandler<ModeChangedEventArgs>? ModeChanged;
        public event EventHandler<DebugChangedEventArgs>? DebugChanged;

        private const int ExecAppID = 0xCB;
        private readonly AutoResetEvent serialEvent = new(false);
        private readonly StringBuilder serialBuffer = new();
        private string serialData = string.Empty;
        private volatile bool closing = false;
        private readonly SerialPort serialPort;
        private ProgrammingMode programmingMode;
        private int blankCheckSize;
        private int memoryAddress;
        private bool debugMode;
        private bool disposedValue;
        private bool flashAfterErase = false;

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

        public bool ApplicationExists => ApplicationFile != null && File.Exists(ApplicationFile);

        public bool ProgrammingExecutiveExists => ProgrammingExecutiveFile != null && File.Exists(ProgrammingExecutiveFile);

        public FlashProgrammer(string port, int baudRate)
        {
            DeviceTypeMap = new()
            {
                { 0x4202, "PIC24FJ32GA102" },
                { 0x4206, "PIC24FJ64GA102" },
                { 0x420A, "PIC24FJ32GA104" },
                { 0x420E, "PIC24FJ64GA104" },
                { 0x4203, "PIC24FJ32GB002" },
                { 0x4207, "PIC24FJ64GB002" },
                { 0x420B, "PIC24FJ32GB004" },
                { 0x420F, "PIC24FJ64GB004" },
            };
            this.serialPort = new SerialPort(port, baudRate);
            this.serialPort.DataReceived += SerialPort_DataReceived;
            this.serialPort.ErrorReceived += SerialPort_ErrorReceived;
            this.serialPort.Open();
            SendSerial(SerialCommand.Initialize);
            _ = Task.Run(() => SerialMonitor());
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

        private void SerialMonitor()
        {
            var data = 0.0;
            var deviceInfo = new DeviceInfo();
            var hexFile = new IntelHex();
            var pageNumber = 0;
            var textOutput = string.Empty;
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
                    var ok = value.StartsWith("0x")
                        ? double.TryParse(value[2..], System.Globalization.NumberStyles.HexNumber, null, out data)
                        : double.TryParse(value, out data);
                    if (!ok)
                    {
                        NotifyStatus($"Device sent invalid data: {this.serialData}");
                    }
                }
                else if (this.serialData.StartsWith(SerialResponse.TextOutput))
                {
                    textOutput = this.serialData[SerialResponse.TextOutput.Length..];
                }
                else
                {
                    switch (this.serialData)
                    {
                        case SerialResponse.Initialized:
                            SerialProgrammingMode = ProgrammingMode.None;
                            DebugMode = false;
                            break;
                        case SerialResponse.Reset:
                            NotifyStatus("Arduino was reset");
                            SerialProgrammingMode = ProgrammingMode.None;
                            break;
                        case SerialResponse.EnteredICSP:
                            NotifyStatus("Entered ICSP mode");
                            SerialProgrammingMode = ProgrammingMode.ICSP;
                            SendSerial(SerialCommand.RequestDeviceID);
                            break;
                        case SerialResponse.EnteredEICSP:
                            NotifyStatus("Entered EICSP mode");
                            SerialProgrammingMode = ProgrammingMode.EICSP;
                            IsExecLoaded = true;
                            SendSerial(SerialCommand.RequestExecutiveVersion);
                            break;
                        case SerialResponse.DebugModeOn:
                            NotifyStatus("Debug mode enabled");
                            DebugMode = true;
                            break;
                        case SerialResponse.DebugModeOff:
                            NotifyStatus("Debug mode disabled");
                            DebugMode = false;
                            break;
                        case SerialResponse.BlankSize:
                            SendSerial($"{this.blankCheckSize:X4}");
                            break;
                        case SerialResponse.PageAddress:
                        case SerialResponse.WordAddress:
                        case SerialResponse.BlockAddress:
                            SendSerial($"{this.memoryAddress:X4}");
                            break;
                        case SerialResponse.BlockErased:
                            NotifyStatus("Block erased");
                            break;
                        case SerialResponse.ReadWord:
                            NotifyStatus($"0x{this.memoryAddress:X6} -> 0x{(int)data:X4}");
                            break;
                        case SerialResponse.ReadPage:
                            DumpPage(textOutput);
                            break;
                        case SerialResponse.ChipErased:
                            NotifyStatus("Chip erased");
                            if (this.flashAfterErase)
                            {
                                this.flashAfterErase = false;
                                SendSerial(SerialCommand.FlashApplication);
                            }
                            break;
                        case SerialResponse.ExecErased:
                            NotifyStatus("Programming executive erased");
                            break;
                        case SerialResponse.BlankCheck:
                            NotifyStatus($"Selected memory region is {(data == 1 ? string.Empty : "NOT ")}blank");
                            break;
                        case SerialResponse.SendApplication:
                            NotifyStatus("Loading application...");
                            if (!ApplicationExists)
                            {
                                NotifyStatus($"Application file not found. {ApplicationFile}");
                                break;
                            }
                            hexFile.Open(ApplicationFile!);
                            pageNumber = 0;
                            SendSerial(SerialCommand.LoadApplication);
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
                            IsExecLoaded = (int)data == ExecAppID;
                            NotifyStatus($"Application ID is 0x{(int)data:X2}, programming executive {(IsExecLoaded ? "is" : "needs to be")} loaded");
                            break;
                        case SerialResponse.ExitedICSP:
                            NotifyStatus("Exited ICSP mode");
                            SerialProgrammingMode = ProgrammingMode.None;
                            break;
                        case SerialResponse.SendExecutive:
                            NotifyStatus("Loading programming executive");
                            if (ProgrammingExecutiveExists)
                            {
                                hexFile.Open(ProgrammingExecutiveFile!);
                                pageNumber = 0;
                                SendSerial(SerialCommand.LoadProgrammingExecutive);
                            }
                            break;
                        case SerialResponse.ExecutiveLoaded:
                            NotifyStatus("Programming executive loaded, verifying...");
                            if (ProgrammingExecutiveExists)
                            {
                                hexFile.Open(ProgrammingExecutiveFile!);
                                pageNumber = 0;
                                SendSerial(SerialCommand.VerifyProgrammingExecutive);
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
                            hexFile.Open(ApplicationFile!);
                            pageNumber = 0;
                            SendSerial(SerialCommand.VerifyApplication);
                            break;
                        case SerialResponse.ApplicationVerified:
                            NotifyStatus("Application verified");
                            break;
                        case SerialResponse.SendNextPage:
                            NotifyStatus($"Sending page {++pageNumber}");
                            var page = hexFile.ReadPageHex();
                            if (string.IsNullOrEmpty(page))
                            {
                                NotifyStatus("All pages sent");
                                SendSerial(SerialCommand.NoMorePages);
                                break;
                            }
                            SendSerial(page);
                            break;
                        case SerialResponse.Error:
                            this.flashAfterErase = false;
                            NotifyStatus("Error");
                            break;

                        default:
                            NotifyStatus($"{this.serialData}");
                            break;
                    }
                }
            }
        }

        private void DumpPage(string page)
        {
            var sa = page.Trim().Split(' ');
            var sb = new StringBuilder();
            var i = 0;
            foreach (var s in sa)
            {
                var n = int.Parse(s, System.Globalization.NumberStyles.HexNumber);
                sb.Append($"{n:X6} ");
                if (++i % 16 == 0)
                {
                    sb.AppendLine();
                }
            }
            NotifyStatus(sb.ToString());
        }

        private void SendSerial(string command)
        {
            lock (this.serialPort)
            {
                this.serialPort.WriteLine(command);
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            lock (this.serialPort)
            {
                this.serialBuffer.Append(this.serialPort.ReadExisting());
                var data = this.serialBuffer.ToString();
                var pos = data.IndexOf('\n');
                if (pos != -1)
                {
                    this.serialData = data.Substring(0, pos++).Trim();
                    this.serialBuffer.Clear();
                    if (pos < data.Length)
                    {
                        this.serialBuffer.Append(data.Substring(pos));
                    }
                    this.serialEvent.Set();
                }
            }
        }

        private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            NotifyStatus(e.EventType.ToString());
        }

        public void RequestExecVersion()
        {
            SendSerial(SerialCommand.RequestExecutiveVersion);
        }

        public void RequestLoadExecutive()
        {
            SendSerial(SerialCommand.FlashProgrammingExecutive);
        }

        public void RequestLoadApplication(bool erase)
        {
            if (erase)
            {
                this.flashAfterErase = true;
                SendSerial(SerialCommand.EraseChip);
            }
            else
            {
                SendSerial(SerialCommand.FlashApplication);
            }
        }

        public void RequestEnterICSP()
        {
            SendSerial(SerialCommand.EnterICSP);
        }
        public void RequestEnterEICSP()
        {
            SendSerial(SerialCommand.EnterEICSP);
        }

        public void RequestExitICSP()
        {
            SendSerial(SerialCommand.ExitICSP);
        }

        public void RequestApplicationID()
        {
            SendSerial(SerialCommand.RequestAppID);
        }

        public void RequestBlankCheck(int size)
        {
            this.blankCheckSize = size;
            SendSerial(SerialCommand.RequestBlankCheck);
        }

        public void ReadWord(int address)
        {
            this.memoryAddress = address;
            SendSerial(SerialCommand.ReadWord);
        }

        public void ReadPage(int address)
        {
            this.memoryAddress = address;
            SendSerial(SerialCommand.ReadPage);
        }

        public void EraseBlock(int address)
        {
            this.memoryAddress = address;
            SendSerial(SerialCommand.EraseBlock);
        }

        public void EraseChip()
        {
            SendSerial(SerialCommand.EraseChip);
        }

        public void EraseExecutive()
        {
            SendSerial(SerialCommand.EraseExecutive);
        }

        public void EnableDebug(bool enable)
        {
            SendSerial(enable ? SerialCommand.DebugOn : SerialCommand.DebugOff);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.closing = true;
                    _ = this.serialEvent.Set();
                    if (this.serialPort.IsOpen)
                    {
                        this.serialPort.Close();
                    }
                }

                this.disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

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


    internal class SerialCommand
    {
        public const string Initialize = "Init";
        public const string RequestExecutiveVersion = "ExecVersion";
        public const string RequestBlankCheck = "BlankCheck";
        public const string RequestDeviceID = "DeviceID";
        public const string RequestAppID = "AppID";
        public const string EnterICSP = "EnterICSP";
        public const string EnterEICSP = "EnterEICSP";
        public const string ExitICSP = "ExitICSP";
        public const string FlashProgrammingExecutive = "FlashExec";
        public const string FlashApplication = "FlashApp";
        public const string LoadProgrammingExecutive = "LoadExec";
        public const string LoadApplication = "LoadApp";
        public const string VerifyProgrammingExecutive = "VerifyExec";
        public const string VerifyApplication = "VerifyApp";
        public const string NoMorePages = "Done";
        public const string DebugOn = "DebugOn";
        public const string DebugOff = "DebugOff";
        public const string EraseBlock = "EraseBlock";
        public const string EraseChip = "EraseChip";
        public const string EraseExecutive = "EraseExec";
        public const string ReadWord = "ReadWord";
        public const string ReadPage = "ReadPage";
    }

    internal class SerialResponse
    {
        public const string Error = "ERROR";
        public const string Reset = "RESET";
        public const string BlankSize = "BLANK_SIZE";
        public const string BlockAddress = "BLOCK_ADDR";
        public const string WordAddress = "WORD_ADDR";
        public const string PageAddress = "PAGE_ADDR";
        public const string ReadWord = "READ_WORD";
        public const string ReadPage = "READ_PAGE";
        public const string BlockErased = "ERASE_BLOCK";
        public const string ChipErased = "ERASE_CHIP";
        public const string ExecErased = "ERASE_EXEC";
        public const string BlankCheck = "BLANK_CHECK";
        public const string Initialized = "INITIALIZED";
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
    }
}
