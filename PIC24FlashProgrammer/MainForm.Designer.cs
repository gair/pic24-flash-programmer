namespace PIC24FlashProgrammer
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.comboSerialPort = new System.Windows.Forms.ComboBox();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.labelDeviceInfo1 = new System.Windows.Forms.Label();
            this.labelExecVersion = new System.Windows.Forms.Label();
            this.groupInfo = new System.Windows.Forms.GroupBox();
            this.labelDevinfoNone = new System.Windows.Forms.Label();
            this.labelDeviceInfo2 = new System.Windows.Forms.Label();
            this.comboBaudRate = new System.Windows.Forms.ComboBox();
            this.textBoxLog = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.buttonCheckExec = new System.Windows.Forms.Button();
            this.buttonEnterEICSP = new System.Windows.Forms.Button();
            this.buttonEnterICSP = new System.Windows.Forms.Button();
            this.buttonExitICSP = new System.Windows.Forms.Button();
            this.buttonLoadExec = new System.Windows.Forms.Button();
            this.buttonDebugMode = new System.Windows.Forms.Button();
            this.groupInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // comboSerialPort
            // 
            this.comboSerialPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboSerialPort.FormattingEnabled = true;
            this.comboSerialPort.Location = new System.Drawing.Point(16, 46);
            this.comboSerialPort.Name = "comboSerialPort";
            this.comboSerialPort.Size = new System.Drawing.Size(120, 33);
            this.comboSerialPort.TabIndex = 0;
            this.comboSerialPort.SelectedIndexChanged += new System.EventHandler(this.comboSerialPort_SelectedIndexChanged);
            // 
            // buttonConnect
            // 
            this.buttonConnect.Location = new System.Drawing.Point(16, 97);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(283, 36);
            this.buttonConnect.TabIndex = 1;
            this.buttonConnect.Text = "Open";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.buttonOpen_Click);
            // 
            // labelDeviceInfo1
            // 
            this.labelDeviceInfo1.AutoSize = true;
            this.labelDeviceInfo1.Location = new System.Drawing.Point(14, 34);
            this.labelDeviceInfo1.Name = "labelDeviceInfo1";
            this.labelDeviceInfo1.Size = new System.Drawing.Size(154, 25);
            this.labelDeviceInfo1.TabIndex = 2;
            this.labelDeviceInfo1.Text = "Device ID: 0x4072";
            // 
            // labelExecVersion
            // 
            this.labelExecVersion.AutoSize = true;
            this.labelExecVersion.Location = new System.Drawing.Point(16, 557);
            this.labelExecVersion.Name = "labelExecVersion";
            this.labelExecVersion.Size = new System.Drawing.Size(108, 25);
            this.labelExecVersion.TabIndex = 3;
            this.labelExecVersion.Text = "Exec version";
            // 
            // groupInfo
            // 
            this.groupInfo.Controls.Add(this.labelDevinfoNone);
            this.groupInfo.Controls.Add(this.labelDeviceInfo2);
            this.groupInfo.Controls.Add(this.labelDeviceInfo1);
            this.groupInfo.Location = new System.Drawing.Point(16, 150);
            this.groupInfo.Name = "groupInfo";
            this.groupInfo.Size = new System.Drawing.Size(283, 107);
            this.groupInfo.TabIndex = 4;
            this.groupInfo.TabStop = false;
            this.groupInfo.Text = "Device information";
            // 
            // labelDevinfoNone
            // 
            this.labelDevinfoNone.Location = new System.Drawing.Point(174, 21);
            this.labelDevinfoNone.Name = "labelDevinfoNone";
            this.labelDevinfoNone.Size = new System.Drawing.Size(88, 38);
            this.labelDevinfoNone.TabIndex = 4;
            this.labelDevinfoNone.Text = "No data";
            this.labelDevinfoNone.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelDeviceInfo2
            // 
            this.labelDeviceInfo2.AutoSize = true;
            this.labelDeviceInfo2.Location = new System.Drawing.Point(14, 67);
            this.labelDeviceInfo2.Name = "labelDeviceInfo2";
            this.labelDeviceInfo2.Size = new System.Drawing.Size(204, 25);
            this.labelDeviceInfo2.TabIndex = 3;
            this.labelDeviceInfo2.Text = "Model: PIC24FJ64GB002";
            // 
            // comboBaudRate
            // 
            this.comboBaudRate.FormattingEnabled = true;
            this.comboBaudRate.Location = new System.Drawing.Point(158, 46);
            this.comboBaudRate.Name = "comboBaudRate";
            this.comboBaudRate.Size = new System.Drawing.Size(141, 33);
            this.comboBaudRate.TabIndex = 5;
            this.comboBaudRate.SelectedIndexChanged += new System.EventHandler(this.comboBaudRate_SelectedIndexChanged);
            // 
            // textBoxLog
            // 
            this.textBoxLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxLog.Location = new System.Drawing.Point(0, 0);
            this.textBoxLog.Multiline = true;
            this.textBoxLog.Name = "textBoxLog";
            this.textBoxLog.Size = new System.Drawing.Size(916, 919);
            this.textBoxLog.TabIndex = 8;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 25);
            this.label1.TabIndex = 9;
            this.label1.Text = "Port";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(158, 18);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(87, 25);
            this.label2.TabIndex = 10;
            this.label2.Text = "Baud rate";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.buttonDebugMode);
            this.splitContainer1.Panel1.Controls.Add(this.labelExecVersion);
            this.splitContainer1.Panel1.Controls.Add(this.buttonCheckExec);
            this.splitContainer1.Panel1.Controls.Add(this.buttonEnterEICSP);
            this.splitContainer1.Panel1.Controls.Add(this.buttonEnterICSP);
            this.splitContainer1.Panel1.Controls.Add(this.buttonExitICSP);
            this.splitContainer1.Panel1.Controls.Add(this.buttonLoadExec);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.groupInfo);
            this.splitContainer1.Panel1.Controls.Add(this.comboBaudRate);
            this.splitContainer1.Panel1.Controls.Add(this.buttonConnect);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.comboSerialPort);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.textBoxLog);
            this.splitContainer1.Size = new System.Drawing.Size(1245, 919);
            this.splitContainer1.SplitterDistance = 325;
            this.splitContainer1.TabIndex = 11;
            // 
            // buttonCheckExec
            // 
            this.buttonCheckExec.Location = new System.Drawing.Point(16, 272);
            this.buttonCheckExec.Name = "buttonCheckExec";
            this.buttonCheckExec.Size = new System.Drawing.Size(283, 40);
            this.buttonCheckExec.TabIndex = 16;
            this.buttonCheckExec.Text = "Check Application ID";
            this.buttonCheckExec.UseVisualStyleBackColor = true;
            this.buttonCheckExec.Click += new System.EventHandler(this.buttonCheckExec_Click);
            // 
            // buttonEnterEICSP
            // 
            this.buttonEnterEICSP.Location = new System.Drawing.Point(16, 456);
            this.buttonEnterEICSP.Name = "buttonEnterEICSP";
            this.buttonEnterEICSP.Size = new System.Drawing.Size(283, 40);
            this.buttonEnterEICSP.TabIndex = 15;
            this.buttonEnterEICSP.Text = "Enter enhanced ICSP mode";
            this.buttonEnterEICSP.UseVisualStyleBackColor = true;
            this.buttonEnterEICSP.Click += new System.EventHandler(this.ButtonEnterEICSP_Click);
            // 
            // buttonEnterICSP
            // 
            this.buttonEnterICSP.Location = new System.Drawing.Point(16, 410);
            this.buttonEnterICSP.Name = "buttonEnterICSP";
            this.buttonEnterICSP.Size = new System.Drawing.Size(283, 40);
            this.buttonEnterICSP.TabIndex = 14;
            this.buttonEnterICSP.Text = "Enter ICSP mode";
            this.buttonEnterICSP.UseVisualStyleBackColor = true;
            this.buttonEnterICSP.Click += new System.EventHandler(this.ButtonEnterICSP_Click);
            // 
            // buttonExitICSP
            // 
            this.buttonExitICSP.Location = new System.Drawing.Point(16, 364);
            this.buttonExitICSP.Name = "buttonExitICSP";
            this.buttonExitICSP.Size = new System.Drawing.Size(283, 40);
            this.buttonExitICSP.TabIndex = 13;
            this.buttonExitICSP.Text = "Exit ICSP mode";
            this.buttonExitICSP.UseVisualStyleBackColor = true;
            this.buttonExitICSP.Click += new System.EventHandler(this.ButtonExitICSP_Click);
            // 
            // buttonLoadExec
            // 
            this.buttonLoadExec.Location = new System.Drawing.Point(16, 318);
            this.buttonLoadExec.Name = "buttonLoadExec";
            this.buttonLoadExec.Size = new System.Drawing.Size(283, 40);
            this.buttonLoadExec.TabIndex = 11;
            this.buttonLoadExec.Text = "Load Programming Executive";
            this.buttonLoadExec.UseVisualStyleBackColor = true;
            this.buttonLoadExec.Click += new System.EventHandler(this.ButtonLoadPExec_Click);
            // 
            // buttonDebugMode
            // 
            this.buttonDebugMode.Location = new System.Drawing.Point(16, 502);
            this.buttonDebugMode.Name = "buttonDebugMode";
            this.buttonDebugMode.Size = new System.Drawing.Size(283, 40);
            this.buttonDebugMode.TabIndex = 17;
            this.buttonDebugMode.Text = "Enable debug mode";
            this.buttonDebugMode.UseVisualStyleBackColor = true;
            this.buttonDebugMode.Click += new System.EventHandler(this.buttonDebugMode_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1245, 919);
            this.Controls.Add(this.splitContainer1);
            this.Name = "MainForm";
            this.Text = "PIC24F Flash Programmer";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.groupInfo.ResumeLayout(false);
            this.groupInfo.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ComboBox comboSerialPort;
        private Button buttonConnect;
        private Label labelDeviceInfo1;
        private Label labelExecVersion;
        private GroupBox groupInfo;
        private ComboBox comboBaudRate;
        private TextBox textBoxLog;
        private Label label1;
        private Label label2;
        private SplitContainer splitContainer1;
        private Button buttonLoadExec;
        private Button buttonEnterEICSP;
        private Button buttonEnterICSP;
        private Button buttonExitICSP;
        private Button buttonCheckExec;
        private Label labelDeviceInfo2;
        private Label labelDevinfoNone;
        private Button buttonDebugMode;
    }
}