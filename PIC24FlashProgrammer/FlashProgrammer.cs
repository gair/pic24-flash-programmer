using System;
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
        public event EventHandler<ProgressEventArgs>? Progress;

        private const int DefaultPageSize = 256;
        private const int ExecAppID = 0xCB;
        private readonly AutoResetEvent serialEvent = new(false);
        private readonly StringBuilder serialBuffer = new();
        private string serialData = string.Empty;
        private volatile bool closing = false;
        private readonly SerialPort serialPort;
        private ProgrammingMode programmingMode;
        private uint blankCheckSize;
        private uint memoryAddress;
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

        public int PageSize { get; set; } = DefaultPageSize;

        public string? ApplicationFile { get; set; }

        public string? ProgrammingExecutiveFile { get; set; }

        public Dictionary<int, (string model, uint addrLimit)>? DeviceTypeMap { get; set; }

        public List<(int start, int end, bool avaiable)> MemoryMap { get; set; } = new();

        public bool IsOpen => this.serialPort?.IsOpen ?? false;

        public bool IsExecLoaded { get; set; }

        public bool ApplicationExists => ApplicationFile != null && File.Exists(ApplicationFile);

        public bool ProgrammingExecutiveExists => ProgrammingExecutiveFile != null && File.Exists(ProgrammingExecutiveFile);

        public FlashProgrammer(string port, int baudRate)
        {
            DeviceTypeMap = new Dictionary<int, (string name, uint limit)>()
            {
                { 0x4202, ("PIC24FJ32GA102", 0x57fe) },
                { 0x4206, ("PIC24FJ64GA102", 0xabfe) },
                { 0x420A, ("PIC24FJ32GA104", 0x57fe) },
                { 0x420E, ("PIC24FJ64GA104", 0xabfe) },
                { 0x4203, ("PIC24FJ32GB002", 0x57fe) },
                { 0x4207, ("PIC24FJ64GB002", 0xabfe) },
                { 0x420B, ("PIC24FJ32GB004", 0x57fe) },
                { 0x420F, ("PIC24FJ64GB004", 0xabfe) },
            };
            this.serialPort = new SerialPort(port, baudRate);
            this.serialPort.DataReceived += SerialPort_DataReceived;
            this.serialPort.ErrorReceived += SerialPort_ErrorReceived;
            this.serialPort.Open();
            SendSerial(CR.Initialize);
            _ = Task.Run(() => SerialMonitor());
        }

        private static bool GetValue(string value, out int data)
        {
            if (value.StartsWith("0x"))
            {
                return int.TryParse(value[2..], System.Globalization.NumberStyles.HexNumber, null, out data);
            }
            else
            {
                return int.TryParse(value, out data);
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

        private void SerialMonitor()
        {
            try
            {
                var data = 0;
                var pageNumber = 0;
                var deviceInfo = new DeviceInfo();
                var hexFile = new IntelHex();
                var textOutput = string.Empty;
                var progressAction = string.Empty;
                this.closing = false;
                while (!this.closing)
                {
                    _ = this.serialEvent.WaitOne();
                    if (this.closing)
                    {
                        return;
                    }
                    if (this.serialData.StartsWith(DeviceResponse.DataOutput))
                    {
                        var value = this.serialData[DeviceResponse.DataOutput.Length..];
                        if (!GetValue(value, out data))
                        {
                            NotifyStatus($"Device sent invalid data: {this.serialData}");
                        }
                    }
                    else if (this.serialData.StartsWith(DeviceResponse.TextOutput))
                    {
                        textOutput = this.serialData[DeviceResponse.TextOutput.Length..];
                    }
                    else
                    {
                        var sd = this.serialData;
                        if (DeviceResponse.Reset.Equals(sd))
                        {
                            NotifyStatus("Arduino was reset");
                            SerialProgrammingMode = ProgrammingMode.None;
                        }
                        else if (CR.Initialize.Response(sd))
                        {
                            SerialProgrammingMode = ProgrammingMode.None;
                            DebugMode = false;
                        }
                        else if (CR.EnterICSP.Response(sd))
                        {
                            NotifyStatus("Entered ICSP mode");
                            SerialProgrammingMode = ProgrammingMode.ICSP;
                            SendSerial(CR.DeviceID);
                        }
                        else if (CR.EnterEICSP.Response(sd))
                        {
                            NotifyStatus("Entered EICSP mode");
                            SerialProgrammingMode = ProgrammingMode.EICSP;
                            IsExecLoaded = true;
                            SendSerial(CR.DeviceID);
                            //SendSerial(CR.ExecutiveVersion);
                        }
                        else if (CR.DebugOn.Response(sd))
                        {
                            NotifyStatus("Debug mode enabled");
                            DebugMode = true;
                        }
                        else if (CR.DebugOff.Response(sd))
                        {
                            NotifyStatus("Debug mode disabled");
                            DebugMode = false;
                        }
                        else if (CR.BlankSize.Response(sd))
                        {
                            SendSerial($"{this.blankCheckSize:X4}");
                        }
                        else if (CR.MemoryAddress.Response(sd))
                        {
                            SendSerial($"{this.memoryAddress:X4}");
                        }
                        else if (CR.ReadWord.Response(sd))
                        {
                            NotifyStatus($"0x{this.memoryAddress:X6} -> 0x{data:X4}");
                        }
                        else if (CR.ReadPage.Response(sd) || CR.ReadPageEx.Response(sd))
                        {
                            DumpPage(textOutput);
                        }
                        else if (CR.EraseBlock.Response(sd))
                        {
                            NotifyStatus("Block erased");
                        }
                        else if (CR.EraseChip.Response(sd))
                        {
                            NotifyStatus("Chip erased");
                            if (this.flashAfterErase)
                            {
                                this.flashAfterErase = false;
                                SendSerial(CR.RequstApplication);
                            }
                        }
                        else if (CR.BlankCheck.Response(sd))
                        {
                            NotifyStatus($"Selected memory region is {(data == 1 ? string.Empty : "NOT ")}blank");
                        }
                        else if (CR.DeviceID.Response(sd))
                        {
                            deviceInfo.ID = data;
                            if (DeviceTypeMap != null && DeviceTypeMap.ContainsKey(deviceInfo.ID))
                            {
                                deviceInfo.Model = DeviceTypeMap[deviceInfo.ID].model;
                                deviceInfo.AddressLimit = DeviceTypeMap[deviceInfo.ID].addrLimit;
                            }
                            NotifyDeviceUpdate(deviceInfo);
                        }
                        else if (CR.AppID.Response(sd))
                        {
                            IsExecLoaded = data == ExecAppID;
                            NotifyStatus($"Application ID is 0x{data:X2}, program executive {(IsExecLoaded ? "is" : "needs to be")} loaded");
                        }
                        else if (CR.ExitICSP.Response(sd))
                        {
                            NotifyStatus("Exited ICSP mode");
                            SerialProgrammingMode = ProgrammingMode.None;
                        }
                        else if (CR.SendExecutive.Response(sd))
                        {
                            NotifyStatus("Loading program executive");
                            if (ProgrammingExecutiveExists)
                            {
                                hexFile.Open(ProgrammingExecutiveFile!, PageSize);
                                pageNumber = 0;
                                progressAction = "Loading program executive";
                                SendSerial(CR.LoadExecutive);
                            }
                        }
                        else if (CR.LoadExecutive.Response(sd))
                        {
                            NotifyStatus("Program executive loaded, verifying...");
                            if (ProgrammingExecutiveExists)
                            {
                                hexFile.Open(ProgrammingExecutiveFile!, PageSize);
                                pageNumber = 0;
                                progressAction = "Verifying program executive";
                                SendSerial(CR.VerifyExecutive);
                            }
                        }
                        else if (CR.VerifyExecutive.Response(sd))
                        {
                            NotifyStatus("Program executive verified");
                        }
                        else if (CR.ExecutiveVersion.Response(sd))
                        {
                            var ver = $"{data:X2}";
                            NotifyVersionUpdate(ver.Insert(1, "."));
                        }
                        else if (CR.SendApplication.Response(sd))
                        {
                            NotifyStatus("Loading application...");
                            if (ApplicationExists)
                            {
                                hexFile.Open(ApplicationFile!, PageSize);
                                pageNumber = 0;
                                progressAction = "Loading application";
                                SendSerial(SerialProgrammingMode == ProgrammingMode.EICSP
                                    ? CR.LoadApplicationEx : CR.LoadApplication);
                            }
                            else
                            {
                                NotifyStatus($"Application file not found. {ApplicationFile}");
                            }
                        }
                        else if (CR.LoadApplication.Response(sd))
                        {
                            NotifyStatus("Application loaded, verifying...");
                            hexFile.Open(ApplicationFile!, PageSize);
                            pageNumber = 0;
                            progressAction = "Verifying application";
                            SendSerial(SerialProgrammingMode == ProgrammingMode.EICSP
                                ? CR.VerifyApplicationEx : CR.VerifyApplication);
                        }
                        else if (CR.VerifyApplication.Response(sd))
                        {
                            NotifyStatus("Application verified");
                        }
                        else if (CR.ResetMCU.Response(sd))
                        {
                            NotifyStatus("MCU reset, application running");
                            SerialProgrammingMode = ProgrammingMode.None;
                        }
                        else if (CR.SendPage.Response(sd))
                        {
                            var page = hexFile.ReadPage24Hex();
                            if (string.IsNullOrEmpty(page))
                            {
                                SendSerial(CR.NoPages);
                                NotifyStatus("No more pages");
                                Progress?.Invoke(this, new ProgressEventArgs(progressAction, 100));
                            }
                            else
                            {
                                var progress = ++pageNumber * 100 / hexFile.TotalPages;
                                Progress?.Invoke(this, new ProgressEventArgs(progressAction, progress));
                                SendSerial(page);
                            }
                        }
                        else if (DeviceResponse.VerifyFail.Equals(sd))
                        {
                            NotifyStatus("Flash programming verification failed.");
                        }
                        else if (DeviceResponse.Error.Equals(sd))
                        {
                            this.flashAfterErase = false;
                            NotifyStatus("Error");
                        }
                        else
                        {
                            NotifyStatus(sd);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                NotifyStatus($"Serial port must be closed. {e.Message}");
            }
        }

        private void DumpPage(string page)
        {
            const int WordWidth = 16;
            var sa = page.Trim().Split(' ');
            var sb = new StringBuilder();
            var i = 0;
            var addr = this.memoryAddress;
            var newLine = true;

            foreach (var s in sa)
            {
                if (newLine)
                {
                    _ = sb.Append($"{addr:X6}: ");
                    newLine = false;
                }
                var n = int.Parse(s, System.Globalization.NumberStyles.HexNumber);
                _ = sb.Append($"{n:X6} ");
                if (++i % WordWidth == 0)
                {
                    addr += WordWidth * 2;
                    _ = sb.AppendLine();
                    newLine = true;
                }
            }
            NotifyStatus(sb.ToString());
        }

        private void SendSerial(CommandResponse cr)
        {
            SendSerial(cr.Command);
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
                _ = this.serialBuffer.Append(this.serialPort.ReadExisting());
                var data = this.serialBuffer.ToString();
                var pos = data.IndexOf('\n');
                if (pos != -1)
                {
                    this.serialData = data[..pos++].Trim();
                    _ = this.serialBuffer.Clear();
                    if (pos < data.Length)
                    {
                        _ = this.serialBuffer.Append(data[pos..]);
                    }
                    _ = this.serialEvent.Set();
                }
            }
        }

        private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            NotifyStatus(e.EventType.ToString());
        }

        public void ExecVersion()
        {
            SendSerial(CR.ExecutiveVersion);
        }

        public void LoadExecutive()
        {
            SendSerial(CR.RequestExecutive);
        }

        public void LoadApplication(bool eraseFirst)
        {
            if (eraseFirst)
            {
                this.flashAfterErase = true;
                SendSerial(CR.EraseChip);
            }
            else
            {
                SendSerial(CR.RequstApplication);
            }
        }

        public void EnterICSP()
        {
            SendSerial(CR.EnterICSP);
        }
        public void EnterEICSP()
        {
            SendSerial(CR.EnterEICSP);
        }

        public void ExitICSP()
        {
            SendSerial(CR.ExitICSP);
        }

        public void ApplicationID()
        {
            SendSerial(CR.AppID);
        }

        public void BlankCheck(uint size)
        {
            this.blankCheckSize = size;
            SendSerial(CR.BlankCheck);
        }

        public void ReadWord(uint address)
        {
            this.memoryAddress = address;
            SendSerial(CR.ReadWord);
        }

        public void ReadPage(uint address)
        {
            this.memoryAddress = address;
            SendSerial(SerialProgrammingMode == ProgrammingMode.EICSP ? CR.ReadPageEx : CR.ReadPage);
        }

        public void EraseBlock(uint address)
        {
            this.memoryAddress = address;
            SendSerial(CR.EraseBlock);
        }

        public void EraseChip()
        {
            SendSerial(CR.EraseChip);
        }

        public void ResetMCU()
        {
            SendSerial(CR.ResetMCU);
        }

        public void EraseExecutive()
        {
            SendSerial(CR.EraseExecutive);
        }

        public void EnableDebug(bool enable)
        {
            SendSerial(enable ? CR.DebugOn : CR.DebugOff);
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

    internal class ProgressEventArgs : EventArgs
    {
        public string Action { get; }
        public int ProgressPercent { get; }

        public ProgressEventArgs(string action, int progress)
        {
            Action = action;
            ProgressPercent = progress;
        }
    }

    internal class DeviceResponse
    {
        public const string Error = "ERROR";
        public const string Reset = "RESET";
        public const string DataOutput = "DATA_OUT:";
        public const string TextOutput = "TEXT_OUT:";
        public const string VerifyFail = "VERIFY_FAIL";
    }

    internal class CR
    {
        public static CommandResponse Initialize { get; } = new("Initialize");
        public static CommandResponse ExecutiveVersion { get; } = new("ExecVersion");
        public static CommandResponse BlankCheck { get; } = new("BlankCheck");
        public static CommandResponse BlankSize { get; } = new("BlankSize");
        public static CommandResponse DeviceID { get; } = new("DeviceID");
        public static CommandResponse AppID { get; } = new("AppID");
        public static CommandResponse EnterICSP { get; } = new("EnterICSP");
        public static CommandResponse EnterEICSP { get; } = new("EnterEICSP");
        public static CommandResponse ExitICSP { get; } = new("ExitICSP");
        public static CommandResponse SendExecutive { get; } = new("SendExec");
        public static CommandResponse RequestExecutive { get; } = new("RequestExec");
        public static CommandResponse RequstApplication { get; } = new("RequestApp");
        public static CommandResponse LoadExecutive { get; } = new("LoadExec");
        public static CommandResponse SendApplication { get; } = new("SendApp");
        public static CommandResponse LoadApplication { get; } = new("LoadApp");
        public static CommandResponse LoadApplicationEx { get; } = new("LoadAppEx");
        public static CommandResponse VerifyExecutive { get; } = new("VerifyExec");
        public static CommandResponse VerifyApplication { get; } = new("VerifyApp");
        public static CommandResponse VerifyApplicationEx { get; } = new("VerifyAppEx");
        public static CommandResponse NoPages { get; } = new("NoPages");
        public static CommandResponse DebugOn { get; } = new("DebugOn");
        public static CommandResponse DebugOff { get; } = new("DebugOff");
        public static CommandResponse EraseBlock { get; } = new("EraseBlock");
        public static CommandResponse EraseChip { get; } = new("EraseChip");
        public static CommandResponse EraseExecutive { get; } = new("EraseExec");
        public static CommandResponse ReadWord { get; } = new("ReadWord");
        public static CommandResponse ReadPage { get; } = new("ReadPage");
        public static CommandResponse ReadPageEx { get; } = new("ReadPageEx");
        public static CommandResponse MemoryAddress { get; } = new("MemoryAddr");
        public static CommandResponse SendPage { get; } = new("SendPage");
        public static CommandResponse ResetMCU { get; } = new("ResetMCU");
    }

    internal class CommandResponse
    {
        private const int LowerCaseMask = 0x20;
        private readonly string response;
        public string Command { get; }

        public CommandResponse(string command)
        {
            Command = command;
            var sb = new StringBuilder();
            var lower = false;
            for (var i = 0; i < command.Length; i++)
            {
                var upper = (command[i] & LowerCaseMask) == 0;
                if (upper && lower)
                {
                    _ = sb.Append('_');
                }
                lower = !upper;
                _ = sb.Append((char)(command[i] & ~LowerCaseMask));
            }
            this.response = sb.ToString();
        }

        public bool Response(string resp)
        {
            return resp.Equals(this.response);
        }
    }
}

