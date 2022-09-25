using System.IO.Ports;
using PIC24FlashProgrammer.Properties;

namespace PIC24FlashProgrammer
{
    public partial class MainForm : Form
    {
        private readonly UsbDeviceMonitor deviceMonitor = new();
        private readonly Dictionary<string, DeviceInfo> devices = new();
        private FlashProgrammer? flashProgrammer;
        private bool autoSelecting = false;
        private readonly bool initializing = true;

        private readonly Dictionary<int, string> deviceMap = new()
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

        public MainForm()
        {
            InitializeComponent();
            PopulatePortsCombo();
            PopulateBaudRateCombo();
            var baudRate = Configuration.BaudRate;
            if (this.comboBaudRate.Items.Contains(baudRate))
            {
                this.comboBaudRate.SelectedItem = baudRate;
            }
            EnableControls();
            UpdateDeviceInfo(null);
            UpdateVersion(null);

            this.deviceMonitor.ConnectionChanged += OnConnectionChanged;
            this.deviceMonitor.Start();
            this.initializing = false;
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
                        this.flashProgrammer?.Close();
                        this.flashProgrammer = new FlashProgrammer()
                        {
                            ProgrammingExecutiveFile = "RIPE_01b_000033.hex",
                            DeviceTypeMap = deviceMap
                        };
                        this.flashProgrammer.UpdateMessage += FlashProgrammer_UpdateMessage;
                        this.flashProgrammer.ModeChanged += FlashProgrammer_ModeChanged;
                        this.flashProgrammer.DebugChanged += FlashProgrammer_DebugChanged;

                        this.flashProgrammer.Open(port, baudRate);
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
                this.flashProgrammer?.Close();
                this.buttonConnect.Text = Resources.OpenSerial;
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
                BeginInvoke(() => EnableControls());
                return;
            }

            var selectedPort = this.comboSerialPort.SelectedItem as string;
            var portAvailable = SerialPort.GetPortNames().Contains(selectedPort);
            this.buttonConnect.Enabled = portAvailable;
            this.groupInfo.Enabled = portAvailable;
            var mode = this.flashProgrammer?.SerialProgrammingMode ?? ProgrammingMode.None;
            var execLoaded = this.flashProgrammer?.IsExecLoaded?? false;
            var portOpen = this.flashProgrammer?.IsOpen ?? false;
            this.buttonEnterEICSP.Enabled = portOpen && mode == ProgrammingMode.None;
            this.buttonEnterICSP.Enabled = portOpen && mode == ProgrammingMode.None;
            this.buttonExitICSP.Enabled = portOpen;
            this.buttonLoadExec.Enabled = portOpen && mode == ProgrammingMode.ICSP;
            this.buttonCheckExec.Enabled = portOpen && mode == ProgrammingMode.ICSP;
            this.buttonDebugMode.Enabled = portOpen;

            this.buttonDebugMode.Text = this.flashProgrammer?.DebugMode?? false ? Resources.DebugDisable : Resources.DebugEnable;
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
                BeginInvoke(new Action<string>(SetStatus), status);
                return;
            }
            this.textBoxLog.AppendText(status + Environment.NewLine);
        }

        private void UpdateVersion(string? version)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<string>(UpdateVersion), version);
                return;
            }
            this.labelExecVersion.Text = version == null? String.Empty : $"Executive version {version}";
        }

        private void UpdateDeviceInfo(DeviceInfo? info)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<DeviceInfo>(UpdateDeviceInfo), info);
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
            this.labelDeviceInfo1.Visible = true;
            this.labelDeviceInfo2.Visible = true;
            this.labelDevinfoNone.Visible = false;
        }

        private void ButtonPExecVersion_Click(object sender, EventArgs e)
        {
            this.flashProgrammer?.RequestExecVersion();
            EnableControls();
        }

        private void ButtonLoadPExec_Click(object sender, EventArgs e)
        {
            EnableControls();

        }

        private void ButtonExitICSP_Click(object sender, EventArgs e)
        {
            this.flashProgrammer?.RequestExitICSP();
            EnableControls();
        }

        private void ButtonEnterICSP_Click(object sender, EventArgs e)
        {
            this.flashProgrammer?.RequestEnterICSP();
            EnableControls();
        }

        private void ButtonEnterEICSP_Click(object sender, EventArgs e)
        {
            this.flashProgrammer?.RequestEnterEICSP();
            EnableControls();
        }

        private void buttonCheckExec_Click(object sender, EventArgs e)
        {
            this.flashProgrammer?.RequestApplicationID();
            EnableControls();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.flashProgrammer?.Close();
        }

        private void buttonDebugMode_Click(object sender, EventArgs e)
        {
            var enable = this.buttonDebugMode.Text == Resources.DebugEnable;
            this.flashProgrammer?.EnableDebug(enable);
            this.buttonDebugMode.Enabled = false;
        }
    }
}