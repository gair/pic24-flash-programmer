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
        private int addressLimit = 0;

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
            this.textBoxExecFile.Text = Configuration.ProgrammingExecutive;
            this.textBoxAppFile.Text = Configuration.ApplicationFile;
            this.textBoxBlankSize.Text = Configuration.BlankSize;
            this.textBoxWordAddress.Text = Configuration.WordAddress;
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
                PopulatePortsCombo();
                EnableControls();
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
            this.buttonBlankCheck.Enabled = !string.IsNullOrEmpty(this.textBoxBlankSize.Text);
            this.panelBlankCheck.Enabled = portOpen && mode == ProgrammingMode.EICSP;
            this.buttonReadWord.Enabled = !string.IsNullOrEmpty(this.textBoxWordAddress.Text);
            this.panelReadWord.Enabled = portOpen && mode == ProgrammingMode.ICSP;
            this.panelReadPage.Enabled = portOpen && (mode != ProgrammingMode.None);
            this.checkBoxErase.Enabled = this.buttonLoadApp.Enabled;
            this.panelErase.Enabled = portOpen && mode == ProgrammingMode.ICSP;
            this.panelBottom.Enabled = portOpen;
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
            this.labelExecVersion.Text = version == null ? string.Empty : $"Executive version {version}";
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
            this.labelDeviceInfo1.Visible = true;
            this.labelDeviceInfo2.Visible = true;
            this.labelDevinfoNone.Visible = false;
        }

        private void ButtonExecVersion_Click(object sender, EventArgs e)
        {
            this.flashProgrammer?.ExecVersion();
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
            this.openFileDialog.Title = "Select program executive file";
            this.openFileDialog.Filter = "RIPE files (RIPE*.hex)|RIPE*.hex|All Files (*.*)|*.*";
            this.openFileDialog.InitialDirectory = Path.GetDirectoryName(this.flashProgrammer?.ProgrammingExecutiveFile ?? string.Empty);
            if (this.openFileDialog.ShowDialog() == DialogResult.OK)
            {
                this.textBoxExecFile.Text = this.openFileDialog.FileName;
            }
        }

        private void buttonBrowseFlash_Click(object sender, EventArgs e)
        {
            this.openFileDialog.FileName = string.Empty;
            this.openFileDialog.Title = "Select code flash file";
            this.openFileDialog.Filter = "Flash files (*.hex)|*.hex|All Files (*.*)|*.*";
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

        private static bool GetValue(string text, out int value)
        {
            return text.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
            ? int.TryParse(text[2..], System.Globalization.NumberStyles.HexNumber, null, out value)
            : int.TryParse(text, out value);
        }

        private void buttonBlankCheck_Click(object sender, EventArgs e)
        {
            var ok = GetValue(this.textBoxBlankSize.Text, out var value);
            if (ok)
            {
                this.flashProgrammer?.BlankCheck(value);
            }
            else
            {
                SetStatus("Invalid size");
            }
        }

        private void textBoxBlankSize_TextChanged(object sender, EventArgs e)
        {
            if (GetValue(this.textBoxBlankSize.Text, out var value))
            {
                if (this.addressLimit > 0 && value > this.addressLimit)
                {
                    value = this.addressLimit;
                    if (this.textBoxBlankSize.Text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                    {
                        this.textBoxBlankSize.Text = $"0x{value:X4}";
                    }
                    else
                    {
                        this.textBoxBlankSize.Text = $"{value}";
                    }
                    this.textBoxBlankSize.SelectionStart = this.textBoxBlankSize.TextLength;
                }

                Configuration.BlankSize = this.textBoxBlankSize.Text;
                EnableControls();
            }
        }

        private void buttonEraseExec_Click(object sender, EventArgs e)
        {
            this.flashProgrammer?.EraseExecutive();
        }

        private void buttonEraseBlock_Click(object sender, EventArgs e)
        {
            this.flashProgrammer?.EraseBlock((int)this.numericBlockAddress.Value);
            Configuration.BlockAddress = (int)this.numericBlockAddress.Value;
        }

        private void buttonEraseChip_Click(object sender, EventArgs e)
        {
            this.flashProgrammer?.EraseChip();
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

        private void textBoxWordAddress_TextChanged(object sender, EventArgs e)
        {
            Configuration.WordAddress = this.textBoxWordAddress.Text;
            EnableControls();
        }

        private void buttonReadWord_Click(object sender, EventArgs e)
        {
            var ok = GetValue(this.textBoxWordAddress.Text, out var value);
            if (ok)
            {
                this.flashProgrammer?.ReadWord(value);
            }
            else
            {
                SetStatus("Invalid address");
            }
        }

        private void buttonReadPage_Click(object sender, EventArgs e)
        {
            this.flashProgrammer?.ReadPage((int)this.numericPageAddress.Value);
            Configuration.PageAddress = (int)this.numericPageAddress.Value;
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
            await Task.Run(() =>
            {
                var hexFile = new IntelHex();
                hexFile.Open(this.textBoxAppFile.Text, 64);
                while (true)
                {
                    var page = hexFile.ReadPage24Hex();
                    if (string.IsNullOrEmpty(page))
                    {
                        break;
                    }
                    SetStatus(page);
                }
            });
        }

        private async void buttonViewExec_Click(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                var hexFile = new IntelHex();
                hexFile.Open(this.textBoxExecFile.Text, 64);
                while (true)
                {
                    var page = hexFile.ReadPage24Hex();
                    if (string.IsNullOrEmpty(page))
                    {
                        break;
                    }
                    SetStatus(page);
                }
            });
        }
    }
}