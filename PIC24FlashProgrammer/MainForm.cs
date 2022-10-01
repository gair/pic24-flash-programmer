using System.IO.Ports;
using HexParser;
using PIC24FlashProgrammer.Properties;

namespace PIC24FlashProgrammer
{
    public partial class MainForm : Form
    {
        private readonly UsbDeviceMonitor deviceMonitor = new();
        private FlashProgrammer? flashProgrammer;
        private bool autoSelecting = false;
        private readonly bool initializing = true;
        private uint addressLimit = 0;
        private int prevProgress = -1;

        public MainForm()
        {
            InitializeComponent();
            var loc = Configuration.WindowLocation;
            var size = Configuration.WindowSize;
            if (loc.X >= 0 && loc.Y >= 0 && size.Width > 0 && size.Height > 0)
            {
                StartPosition = FormStartPosition.Manual;
                Location = loc;
                Size = size;
                if (Configuration.WindowMaxed)
                {
                    WindowState = FormWindowState.Maximized;
                }
            }

            PopulatePortsCombo();
            PopulateBaudRateCombo();
            var baudRate = Configuration.BaudRate;
            if (this.comboBaudRate.Items.Contains(baudRate))
            {
                this.comboBaudRate.SelectedItem = baudRate;
            }
            UpdateDeviceInfo(null);
            UpdateVersion(null);
            ShowProgressBar(false);
            this.progressBar.Top = this.panelFiles.Top - this.progressBar.Height - 8;
            this.labelProgress.Top = this.progressBar.Top - this.labelProgress.Height - 4;
            this.textBoxExecFile.Text = Configuration.ProgrammingExecutive;
            this.textBoxAppFile.Text = Configuration.ApplicationFile;
            this.numericWordAddress.Value = Configuration.WordAddress;
            this.numericBlankSize.Value = Configuration.BlankSize;
            this.numericBlockAddress.Value = Configuration.BlockAddress;
            this.numericPageAddress.Value = Configuration.PageAddress;

            this.deviceMonitor.ConnectionChanged += OnConnectionChanged;
            this.deviceMonitor.Start();
            this.initializing = false;
            EnableControls();
        }

        private void OnConnectionChanged(object? sender, UsbDeviceMonitorArgs e)
        {
            _ = BeginInvoke(new Action(() =>
            {
                if (!e.Device.IsConnected && this.buttonConnect.Text == Resources.CloseSerial)
                {
                    this.buttonConnect.PerformClick();
                }
                else
                {
                    PopulatePortsCombo();
                    EnableControls();
                }
            }));
        }

        private void PopulatePortsCombo()
        {
            foreach (var port in SerialPort.GetPortNames())
            {
                if (!this.comboSerialPort.Items.Contains(port))
                {
                    _ = this.comboSerialPort.Items.Add(port);
                }
            }
            var savedPort = Configuration.SerialPort;
            if (savedPort != null && this.comboSerialPort.Items.Contains(savedPort))
            {
                this.autoSelecting = true;
                this.comboSerialPort.SelectedItem = savedPort;
            }
            else if (this.comboSerialPort.Items.Count > 0)
            {
                this.autoSelecting = true;
                this.comboSerialPort.SelectedIndex = 0;
            }
        }

        private void PopulateBaudRateCombo()
        {
            this.comboBaudRate.Items.Clear();
            this.comboBaudRate.Items.AddRange(new object[] { 110, 150, 300, 1200, 2400, 4800, 9600, 19200, 38400, 57600, 74880, 115200, 230400, 460800, 921600 });
            this.comboBaudRate.SelectedIndex = 6;
        }

        private void comboSerialPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.comboSerialPort.SelectedIndex != -1 && !this.autoSelecting)
            {
                if (!this.initializing && this.comboSerialPort.SelectedItem is string port)
                {
                    Configuration.SerialPort = port;
                }
            }
            this.autoSelecting = false;
            EnableControls();
        }

        private void buttonOpen_Click(object sender, EventArgs e)
        {
            if (this.buttonConnect.Text == Resources.OpenSerial)
            {

                if (this.comboSerialPort.SelectedItem is string port)
                {
                    var baudRate = (int)this.comboBaudRate.SelectedItem;
                    try
                    {
                        this.flashProgrammer?.Dispose();
                        this.flashProgrammer = new FlashProgrammer(port, baudRate)
                        {
                            ProgrammingExecutiveFile = this.textBoxExecFile.Text,
                            ApplicationFile = this.textBoxAppFile.Text,
                        };
                        this.flashProgrammer.UpdateMessage += FlashProgrammer_UpdateMessage;
                        this.flashProgrammer.ModeChanged += FlashProgrammer_ModeChanged;
                        this.flashProgrammer.DebugChanged += FlashProgrammer_DebugChanged;
                        this.flashProgrammer.Progress += FlashProgrammer_Progress;

                        Configuration.SerialPort = port;
                        Configuration.BaudRate = baudRate;
                        this.buttonConnect.Text = Resources.CloseSerial;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("{0}", ex.Message);
                        SetStatus(ex.Message);
                    }
                }
            }
            else
            {
                this.flashProgrammer?.Dispose();
                this.flashProgrammer = null;
                this.buttonConnect.Text = Resources.OpenSerial;
                UpdateDeviceInfo(null);
                UpdateVersion(null);
            }
            EnableControls();
        }

        private void FlashProgrammer_Progress(object? sender, ProgressEventArgs e)
        {
            if (this.prevProgress != e.ProgressPercent)
            {
                Invoke(new Action(() =>
                {
                    this.timerProgress.Stop();
                    if (e.ProgressPercent == -1)
                    {
                        ShowProgressBar(false, animate: true);
                        return;
                    }
                    this.prevProgress = e.ProgressPercent;
                    this.progressBar.Value = e.ProgressPercent;
                    if (e.ProgressPercent == 0)
                    {
                        ShowProgressBar(true, e.Action, true);
                    }
                    else if (e.ProgressPercent == 100)
                    {
                        this.timerProgress.Interval = 2000;
                        this.timerProgress.Start();
                    }
                }));
            }
        }

        private void timerProgress_Tick(object sender, EventArgs e)
        {
            FlashProgrammer_Progress(this, new ProgressEventArgs(string.Empty, -1));
        }

        private void FlashProgrammer_DebugChanged(object? sender, DebugChangedEventArgs e)
        {
            EnableControls();
        }

        private void FlashProgrammer_ModeChanged(object? sender, ModeChangedEventArgs e)
        {
            EnableControls();
        }

        private void FlashProgrammer_UpdateMessage(object? sender, MessageEventArgs e)
        {
            switch (e.Type)
            {
                case MessageType.StatusMessage:
                    SetStatus(e.Message);
                    break;
                case MessageType.DeviceUpdate:
                    UpdateDeviceInfo(e.Info);
                    break;
                case MessageType.VersionUpdate:
                    UpdateVersion(e.Message);
                    break;
            }
        }

        private void EnableControls()
        {
            if (InvokeRequired)
            {
                _ = BeginInvoke(() => EnableControls());
                return;
            }

            var selectedPort = this.comboSerialPort.SelectedItem as string;
            var portAvailable = SerialPort.GetPortNames().Contains(selectedPort);
            this.buttonConnect.Enabled = portAvailable;
            this.groupInfo.Enabled = portAvailable;
            var mode = this.flashProgrammer?.SerialProgrammingMode ?? ProgrammingMode.None;
            var portOpen = this.flashProgrammer?.IsOpen ?? false;
            this.buttonEnterEICSP.Enabled = portOpen && mode == ProgrammingMode.None;
            this.buttonEnterICSP.Enabled = portOpen && mode == ProgrammingMode.None;
            this.buttonExitICSP.Enabled = portOpen;
            this.buttonLoadExec.Enabled = portOpen && mode == ProgrammingMode.ICSP;
            this.buttonCheckExec.Enabled = portOpen && mode == ProgrammingMode.ICSP;
            this.buttonDebugMode.Enabled = portOpen;
            this.buttonLoadExec.Enabled = portOpen && mode == ProgrammingMode.ICSP && (this.flashProgrammer?.ProgrammingExecutiveExists ?? false);
            this.buttonLoadApp.Enabled = portOpen && mode != ProgrammingMode.None && (this.flashProgrammer?.ApplicationExists ?? false);
            this.buttonDebugMode.Text = this.flashProgrammer?.DebugMode ?? false ? Resources.DebugDisable : Resources.DebugEnable;
            this.panelBlankCheck.Enabled = portOpen && mode == ProgrammingMode.EICSP;
            this.panelReadWord.Enabled = portOpen && mode != ProgrammingMode.None;
            this.panelReadPage.Enabled = portOpen && mode != ProgrammingMode.None;
            this.checkBoxErase.Enabled = portOpen && mode == ProgrammingMode.ICSP;
            this.buttonEraseChip.Enabled = portOpen && mode == ProgrammingMode.ICSP;
            this.panelErase.Enabled = portOpen && mode != ProgrammingMode.None;
            this.panelBottom.Enabled = portOpen;
            this.buttonRunApp.Enabled = portOpen;
            this.buttonViewApp.Enabled = this.flashProgrammer?.ApplicationExists ?? false;
            this.buttonViewExec.Enabled = this.flashProgrammer?.ProgrammingExecutiveExists ?? false;
        }

        private void comboBaudRate_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!this.initializing && this.comboBaudRate.SelectedIndex != -1)
            {
                Configuration.BaudRate = (int)this.comboBaudRate.SelectedItem;
            }
        }

        private void SetStatus(string? status)
        {
            if (InvokeRequired)
            {
                _ = BeginInvoke(new Action<string>(SetStatus), status);
                return;
            }
            this.textBoxLog.AppendText(status + Environment.NewLine);
            EnableControls();
        }

        private void UpdateVersion(string? version)
        {
            if (InvokeRequired)
            {
                _ = BeginInvoke(new Action<string>(UpdateVersion), version);
                return;
            }
            this.labelExecVersion.Text = version == null ? string.Empty : string.Format(Resources.ExecutiveVersion, version);
        }

        private void UpdateDeviceInfo(DeviceInfo? info)
        {
            if (InvokeRequired)
            {
                _ = BeginInvoke(new Action<DeviceInfo>(UpdateDeviceInfo), info);
                return;
            }
            if (info == null)
            {
                this.labelDeviceInfo1.Visible = false;
                this.labelDeviceInfo2.Visible = false;
                this.labelDevinfoNone.Visible = true;
                this.labelDevinfoNone.Text = Resources.DevinfoNone;
                this.labelDevinfoNone.Dock = DockStyle.Fill;
                return;
            }

            this.labelDeviceInfo1.Text = string.Format(Resources.DeviceID, info.ID);
            this.labelDeviceInfo2.Text = info.Model == null ? string.Empty : string.Format(Resources.DeviceModel, info.Model);
            this.addressLimit = info.AddressLimit;
            this.numericBlankSize.Maximum = (info.AddressLimit / 2) + 1;
            this.labelDeviceInfo1.Visible = true;
            this.labelDeviceInfo2.Visible = true;
            this.labelDevinfoNone.Visible = false;
        }

        private void ButtonLoadExec_Click(object sender, EventArgs e)
        {
            this.flashProgrammer?.LoadExecutive();
        }

        private void buttonLoadApp_Click(object sender, EventArgs e)
        {
            this.flashProgrammer?.LoadApplication(this.checkBoxErase.Checked);
        }

        private void ButtonExitICSP_Click(object sender, EventArgs e)
        {
            this.flashProgrammer?.ExitICSP();
        }

        private void ButtonEnterICSP_Click(object sender, EventArgs e)
        {
            this.flashProgrammer?.EnterICSP();
        }

        private void ButtonEnterEICSP_Click(object sender, EventArgs e)
        {
            this.flashProgrammer?.EnterEICSP();
            this.checkBoxErase.Checked = false;
        }

        private void buttonCheckExec_Click(object sender, EventArgs e)
        {
            this.flashProgrammer?.ApplicationID();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.flashProgrammer?.Dispose();
            Configuration.WindowMaxed = WindowState == FormWindowState.Maximized;
            if (!Configuration.WindowMaxed)
            {
                Configuration.WindowLocation = Location;
                Configuration.WindowSize = Size;
            }
        }

        private void buttonDebugMode_Click(object sender, EventArgs e)
        {
            var enable = this.buttonDebugMode.Text == Resources.DebugEnable;
            this.flashProgrammer?.EnableDebug(enable);
            this.buttonDebugMode.Enabled = false;
        }

        private void buttonBrowseExec_Click(object sender, EventArgs e)
        {
            this.openFileDialog.FileName = string.Empty;
            this.openFileDialog.Title = Resources.SelectExecutiveFile;
            this.openFileDialog.Filter = Resources.ExecutiveFileFilter;
            this.openFileDialog.InitialDirectory = Path.GetDirectoryName(this.flashProgrammer?.ProgrammingExecutiveFile ?? string.Empty);
            if (this.openFileDialog.ShowDialog() == DialogResult.OK)
            {
                this.textBoxExecFile.Text = this.openFileDialog.FileName;
            }
        }

        private void buttonBrowseApp_Click(object sender, EventArgs e)
        {
            this.openFileDialog.FileName = string.Empty;
            this.openFileDialog.Title = Resources.SelectApplicationFile;
            this.openFileDialog.Filter = Resources.ApplicationFileFilter;
            this.openFileDialog.InitialDirectory = Path.GetDirectoryName(this.flashProgrammer?.ApplicationFile ?? string.Empty);
            if (this.openFileDialog.ShowDialog() == DialogResult.OK)
            {
                this.textBoxAppFile.Text = this.openFileDialog.FileName;
            }
        }

        private void textBoxExecFile_TextChanged(object sender, EventArgs e)
        {
            if (this.flashProgrammer != null)
            {
                this.flashProgrammer.ProgrammingExecutiveFile = this.textBoxExecFile.Text;
                if (this.flashProgrammer.ProgrammingExecutiveExists)
                {
                    Configuration.ProgrammingExecutive = this.flashProgrammer.ProgrammingExecutiveFile;
                }
                EnableControls();
            }
        }

        private void textBoxAppFile_TextChanged(object sender, EventArgs e)
        {
            if (this.flashProgrammer != null)
            {
                this.flashProgrammer.ApplicationFile = this.textBoxAppFile.Text;
                if (this.flashProgrammer.ApplicationExists)
                {
                    Configuration.ApplicationFile = this.flashProgrammer.ApplicationFile;
                }
                EnableControls();
            }
        }

        private static bool GetValue(string text, out uint value)
        {
            return text.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
            ? uint.TryParse(text[2..], System.Globalization.NumberStyles.HexNumber, null, out value)
            : uint.TryParse(text, out value);
        }

        private void buttonBlankCheck_Click(object sender, EventArgs e)
        {
            this.flashProgrammer?.BlankCheck((uint)this.numericBlankSize.Value);
            Configuration.BlankSize = (uint)this.numericBlankSize.Value;
        }

        private string? LimitRange(string valueString, uint limit = 0)
        {
            // try not to access reserved memory
            if (GetValue(valueString, out var value))
            {
                var newValue = value & ~1;
                if (limit > 0)
                {
                    newValue = Math.Min(limit, value);
                }
                else if (value > 0xff0002)
                {
                    newValue = 0xff0002;
                }
                else if (value is > 0x80088E and < 0xff0000)
                {
                    newValue = 0xff0000;
                }
                else if (value is > 0x800800 and < 0x800880)
                {
                    newValue = 0x800880;
                }
                else if (this.addressLimit > 0 && value > this.addressLimit && value < 0x800000)
                {
                    newValue = this.addressLimit;
                }

                valueString = valueString.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? $"0x{newValue:X4}" : $"{newValue}";

                if (newValue != value)
                {
                    return valueString;
                }
            }

            return null;
        }

        private void buttonEraseExec_Click(object sender, EventArgs e)
        {
            this.flashProgrammer?.EraseExecutive();
        }

        private void buttonEraseBlock_Click(object sender, EventArgs e)
        {
            this.flashProgrammer?.EraseBlocks((uint)this.numericBlockAddress.Value, (int)this.numericBlockCount.Value);
            Configuration.BlockAddress = (uint)this.numericBlockAddress.Value;
        }

        private void buttonEraseChip_Click(object sender, EventArgs e)
        {
            this.flashProgrammer?.EraseChip();
        }

        private void buttonRunApp_Click(object sender, EventArgs e)
        {
            this.flashProgrammer?.ResetMCU();
        }

        private void textBoxBlockAddress_TextChanged(object sender, EventArgs e)
        {
            EnableControls();
        }

        private void numericBlockAddress_ValueChanged(object sender, EventArgs e)
        {
            if (this.numericBlockAddress.Value % 0x400 != 0)
            {
                this.numericBlockAddress.Value -= this.numericBlockAddress.Value % 0x400;
            }
        }

        private void buttonReadWord_Click(object sender, EventArgs e)
        {
            this.flashProgrammer?.ReadWord((uint)this.numericWordAddress.Value);
            Configuration.WordAddress = (uint)this.numericWordAddress.Value;
        }

        private void buttonReadPage_Click(object sender, EventArgs e)
        {
            this.flashProgrammer?.ReadPage((uint)this.numericPageAddress.Value);
            Configuration.PageAddress = (uint)this.numericPageAddress.Value;
        }

        private void numericPageAddress_ValueChanged(object sender, EventArgs e)
        {
            if (this.numericPageAddress.Value % 0x80 != 0)
            {
                this.numericPageAddress.Value -= this.numericPageAddress.Value % 0x80;
            }
        }

        private async void buttonViewApp_Click(object sender, EventArgs e)
        {
            await ViewHexFile(this.textBoxAppFile.Text);
        }

        private async void buttonViewExec_Click(object sender, EventArgs e)
        {
            await ViewHexFile(this.textBoxExecFile.Text);
        }

        private async void textBoxLog_DragDrop(object sender, DragEventArgs e)
        {
            var obj = e.Data?.GetData(DataFormats.FileDrop);
            if (obj is string[] files && files.Length == 1)
            {
                await ViewHexFile(files[0]);
            }
        }

        private void textBoxLog_DragOver(object sender, DragEventArgs e)
        {
            var obj = e.Data?.GetData(DataFormats.FileDrop);
            if (obj is string[] files && files.Length == 1 && files[0].EndsWith(".hex", StringComparison.OrdinalIgnoreCase))
            {
                e.Effect = DragDropEffects.Copy;
                return;
            }
            e.Effect = DragDropEffects.None;
        }

        private async void ShowProgressBar(bool show, string? message = null, bool animate = false)
        {
            const int ControlSpacing = 8;
            const int AnimationDelay = 20;
            const string AnimatedProperty = "Height";
            var heightShown = this.panelFiles.Height + (ControlSpacing * 7);
            var heightHidden = this.panelFiles.Height;
            var startHeight = heightHidden;
            var endHeight = heightShown;
            if (!show)
            {
                startHeight = heightShown;
                endHeight = heightHidden;
            }
            else if (message != null)
            {
                this.labelProgress.Text = message;
            }
            if (this.panelBottom.Height == endHeight)
            {
                return;
            }

            if (animate)
            {
                await Animation.AnimateControl(this.panelBottom, startHeight, endHeight, 6, AnimationDelay, AnimatedProperty);
            }
            else
            {
                this.panelBottom.Height = endHeight;
            }
        }

        private Task ViewHexFile(string fileName)
        {
            return Task.Run(() =>
            {
                if (File.Exists(fileName))
                {
                    try
                    {
                        var hexFile = new IntelHex();
                        hexFile.Open(fileName, 64);
                        while (true)
                        {
                            var page = hexFile.ReadPage24Hex();
                            if (string.IsNullOrEmpty(page))
                            {
                                break;
                            }
                            SetStatus(page);
                        }
                    }
                    catch (Exception e)
                    {
                        SetStatus(e.Message);
                    }
                }
            });
        }
    }
}