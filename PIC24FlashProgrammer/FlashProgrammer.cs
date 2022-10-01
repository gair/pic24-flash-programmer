using System.IO.Ports;
using System.Text;
using HexParser;
using PIC24FlashProgrammer.Properties;

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
        private volatile bool closing = false;
        private readonly SerialPort serialPort;
        private ProgrammingMode programmingMode;
        private uint memorySize;
        private uint memoryAddress;
        private int blockCount;
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
            this.serialPort.Open();
            SendSerial(CR.Initialize);
            _ = Task.Run(() => SerialMonitor());
        }

        private static bool GetValue(string value, out int data)
        {
            return value.StartsWith("0x")
                ? int.TryParse(value[2..], System.Globalization.NumberStyles.HexNumber, null, out data)
                : int.TryParse(value, out data);
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

                    var sd = this.serialPort.ReadLine().TrimEnd();
                    if (this.closing)
                    {
                        return;
                    }
                    if (CR.DataOutput.AtStartOf(sd))
                    {
                        var value = sd[CR.DataOutput.Length..];
                        if (!GetValue(value, out data))
                        {

                            NotifyStatus(string.Format(Resources.ReceivedInvalid, sd));
                        }
                    }
                    else if (CR.TextOutput.AtStartOf(sd))
                    {
                        textOutput = sd[CR.TextOutput.Length..];
                    }
                    else
                    {
                        if (CR.Reset.Response(sd))
                        {
                            NotifyStatus(Resources.ArduinoReset);
                            SerialProgrammingMode = ProgrammingMode.None;
                        }
                        else if (CR.Initialize.Response(sd))
                        {
                            SerialProgrammingMode = ProgrammingMode.None;
                            DebugMode = false;
                        }
                        else if (CR.EnterICSP.Response(sd))
                        {
                            NotifyStatus(Resources.EnteredICSP);
                            SerialProgrammingMode = ProgrammingMode.ICSP;
                            SendSerial(CR.DeviceID);
                        }
                        else if (CR.EnterEICSP.Response(sd))
                        {
                            NotifyStatus(Resources.EnteredEICSP);
                            SerialProgrammingMode = ProgrammingMode.EICSP;
                            IsExecLoaded = true;
                            SendSerial(CR.DeviceID);
                        }
                        else if (CR.DebugOn.Response(sd))
                        {
                            NotifyStatus(Resources.DebugOn);
                            DebugMode = true;
                        }
                        else if (CR.DebugOff.Response(sd))
                        {
                            NotifyStatus(Resources.DebugOff);
                            DebugMode = false;
                        }
                        else if (CR.MemorySize.Response(sd))
                        {
                            SendSerial($"{this.memorySize:X4}");
                        }
                        else if (CR.MemoryAddress.Response(sd))
                        {
                            SendSerial($"{this.memoryAddress:X4}");
                        }
                        else if (CR.ReadWord.Response(sd))
                        {
                            NotifyStatus($"0x{this.memoryAddress:X6} -> 0x{data:X4}");
                        }
                        else if (CR.ReadPage.Response(sd))
                        {
                            HexDumpPage(textOutput);
                        }
                        else if (CR.EraseBlocks.Response(sd))
                        {
                            NotifyStatus(Resources.BlockErased);
                        }
                        else if (CR.NumPages.Response(sd))
                        {
                            SendSerial($"{this.blockCount}");
                        }
                        else if (CR.EraseChip.Response(sd))
                        {
                            NotifyStatus(Resources.ChipErased);
                            if (this.flashAfterErase)
                            {
                                this.flashAfterErase = false;
                                SendSerial(CR.RequstApplication);
                            }
                        }
                        else if (CR.BlankCheck.Response(sd))
                        {
                            NotifyStatus(data == 1 ? Resources.MemoryBlank : Resources.MemoryNotBlank);
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
                            if (SerialProgrammingMode == ProgrammingMode.EICSP)
                            {
                                SendSerial(CR.ExecutiveVersion);
                            }
                        }
                        else if (CR.AppID.Response(sd))
                        {
                            IsExecLoaded = data == ExecAppID;
                            var status = string.Format(Resources.ApplicationID, data);
                            status += IsExecLoaded ? Resources.ApplicationIDLoaded : Resources.ApplicationIDNotLoaded;
                            NotifyStatus(status);
                        }
                        else if (CR.ExitICSP.Response(sd))
                        {
                            NotifyStatus(Resources.ExitICSP);
                            SerialProgrammingMode = ProgrammingMode.None;
                        }
                        else if (CR.RequestExecutive.Response(sd))
                        {
                            NotifyStatus(Resources.StatusLoadingExec);
                            if (ProgrammingExecutiveExists)
                            {
                                hexFile.Open(ProgrammingExecutiveFile!, PageSize);
                                pageNumber = 0;
                                progressAction = Resources.ProgressLoadingExec;
                                SendSerial(CR.LoadExecutive);
                            }
                        }
                        else if (CR.LoadExecutive.Response(sd))
                        {
                            NotifyStatus(Resources.StatusVerifyingExec);
                            if (ProgrammingExecutiveExists)
                            {
                                hexFile.Open(ProgrammingExecutiveFile!, PageSize);
                                pageNumber = 0;
                                progressAction = Resources.ProgressVerifyingExec;
                                SendSerial(CR.VerifyExecutive);
                            }
                        }
                        else if (CR.VerifyExecutive.Response(sd))
                        {
                            NotifyStatus(Resources.VerifiedExec);
                        }
                        else if (CR.ExecutiveVersion.Response(sd))
                        {
                            var ver = $"{data:X2}";
                            NotifyVersionUpdate(ver.Insert(1, "."));
                        }
                        else if (CR.RequstApplication.Response(sd))
                        {
                            NotifyStatus(Resources.StatusLoadingApp);
                            if (ApplicationExists)
                            {
                                hexFile.Open(ApplicationFile!, PageSize);
                                pageNumber = 0;
                                progressAction = Resources.ProgressLoadingApp;
                                SendSerial(CR.LoadApplication);
                            }
                            else
                            {
                                NotifyStatus(string.Format(Resources.AppFileNotFound, ApplicationFile));
                            }
                        }
                        else if (CR.LoadApplication.Response(sd))
                        {
                            NotifyStatus(Resources.StatusVerifyingApp);
                            hexFile.Open(ApplicationFile!, PageSize);
                            pageNumber = 0;
                            progressAction = Resources.ProgressVerifyingApp;
                            SendSerial(CR.VerifyApplication);
                        }
                        else if (CR.VerifyApplication.Response(sd))
                        {
                            NotifyStatus(Resources.VerifiedApp);
                        }
                        else if (CR.ResetMCU.Response(sd))
                        {
                            NotifyStatus(Resources.MCUReset);
                            SerialProgrammingMode = ProgrammingMode.None;
                        }
                        else if (CR.MemoryCRC.Response(sd))
                        {
                            NotifyStatus($"CRC: 0x{data:X4}");
                        }
                        else if (CR.SendPage.Response(sd))
                        {
                            var page = hexFile.ReadPage24Hex();
                            if (string.IsNullOrEmpty(page))
                            {
                                SendSerial(CR.NoPages);
                                NotifyStatus(Resources.NoPages);
                                Progress?.Invoke(this, new ProgressEventArgs(progressAction, 100));
                            }
                            else
                            {
                                var progress = ++pageNumber * 100 / hexFile.TotalPages;
                                Progress?.Invoke(this, new ProgressEventArgs(progressAction, progress));
                                SendSerial(page);
                            }
                        }
                        else if (CR.VerifyFail.Response(sd))
                        {
                            NotifyStatus(Resources.VerificationFailed);
                        }
                        else if (CR.Error.Response(sd))
                        {
                            this.flashAfterErase = false;
                            NotifyStatus(Resources.Error);
                        }
                        else if (CR.WrongMode.Response(sd))
                        {
                            NotifyStatus(Resources.WrongMode);
                        }
                        else
                        {
                            NotifyStatus(sd);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Ignore, port was probably closed
            }
            catch (Exception e)
            {
                NotifyStatus(string.Format(Resources.SerialPortError, e.Message));
            }
        }

        private void HexDumpPage(string page)
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
            this.memorySize = size;
            SendSerial(CR.BlankCheck);
        }

        public void CalculateCRC(uint address, uint size)
        {
            this.memoryAddress = address;
            this.memorySize = size;
            SendSerial(CR.MemoryCRC);
        }


        public void ReadWord(uint address)
        {
            this.memoryAddress = address;
            SendSerial(CR.ReadWord);
        }

        public void ReadPage(uint address)
        {
            this.memoryAddress = address;
            SendSerial(CR.ReadPage);
        }

        public void EraseBlocks(uint address, int pages)
        {
            this.blockCount = pages;
            this.memoryAddress = address;
            SendSerial(CR.EraseBlocks);
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

    internal class CR
    {
        public static CommandResponse Reset { get; } = new("Reset");
        public static CommandResponse Error { get; } = new("Error");
        public static CommandResponse WrongMode { get; } = new("WrongMode");
        public static CommandResponse VerifyFail { get; } = new("VerifyFail");
        public static CommandResponse TextOutput { get; } = new("TextOut:");
        public static CommandResponse DataOutput { get; } = new("DataOut:");
        public static CommandResponse Initialize { get; } = new("Initialize");
        public static CommandResponse ExecutiveVersion { get; } = new("ExecVersion");
        public static CommandResponse BlankCheck { get; } = new("BlankCheck");
        public static CommandResponse MemoryCRC { get; } = new("MemoryCRC");
        public static CommandResponse MemorySize { get; } = new("MemorySize");
        public static CommandResponse DeviceID { get; } = new("DeviceID");
        public static CommandResponse AppID { get; } = new("AppID");
        public static CommandResponse EnterICSP { get; } = new("EnterICSP");
        public static CommandResponse EnterEICSP { get; } = new("EnterEICSP");
        public static CommandResponse ExitICSP { get; } = new("ExitICSP");
        public static CommandResponse RequestExecutive { get; } = new("RequestExec");
        public static CommandResponse RequstApplication { get; } = new("RequestApp");
        public static CommandResponse LoadExecutive { get; } = new("LoadExec");
        public static CommandResponse LoadApplication { get; } = new("LoadApp");
        public static CommandResponse VerifyExecutive { get; } = new("VerifyExec");
        public static CommandResponse VerifyApplication { get; } = new("VerifyApp");
        public static CommandResponse NoPages { get; } = new("NoPages");
        public static CommandResponse DebugOn { get; } = new("DebugOn");
        public static CommandResponse DebugOff { get; } = new("DebugOff");
        public static CommandResponse EraseBlocks { get; } = new("EraseBlocks");
        public static CommandResponse NumPages { get; } = new("NumPages");
        public static CommandResponse EraseChip { get; } = new("EraseChip");
        public static CommandResponse EraseExecutive { get; } = new("EraseExec");
        public static CommandResponse ReadWord { get; } = new("ReadWord");
        public static CommandResponse ReadPage { get; } = new("ReadPage");
        public static CommandResponse MemoryAddress { get; } = new("MemoryAddr");
        public static CommandResponse SendPage { get; } = new("SendPage");
        public static CommandResponse ResetMCU { get; } = new("ResetMCU");
    }

    internal class CommandResponse
    {
        private readonly string expectedResponse;
        public string Command { get; }

        public CommandResponse(string command)
        {
            Command = command;
            this.expectedResponse = command.ToUpper();
        }

        public bool Response(string resp)
        {
            return resp.Equals(this.expectedResponse);
        }

        public bool AtStartOf(string resp)
        {
            return resp.StartsWith(this.expectedResponse);
        }

        public int Length => this.expectedResponse.Length;
    }
}

