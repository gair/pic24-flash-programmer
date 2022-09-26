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
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.panelReadPage = new System.Windows.Forms.Panel();
            this.numericPageAddress = new System.Windows.Forms.NumericUpDown();
            this.buttonReadPage = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.panelReadWord = new System.Windows.Forms.Panel();
            this.textBoxWordAddress = new System.Windows.Forms.TextBox();
            this.buttonReadWord = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.panelErase = new System.Windows.Forms.Panel();
            this.numericBlockAddress = new System.Windows.Forms.NumericUpDown();
            this.buttonEraseExec = new System.Windows.Forms.Button();
            this.buttonEraseChip = new System.Windows.Forms.Button();
            this.buttonEraseBlock = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.panelBlankCheck = new System.Windows.Forms.Panel();
            this.textBoxBlankSize = new System.Windows.Forms.TextBox();
            this.buttonBlankCheck = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.buttonDebugMode = new System.Windows.Forms.Button();
            this.buttonCheckExec = new System.Windows.Forms.Button();
            this.buttonEnterEICSP = new System.Windows.Forms.Button();
            this.buttonEnterICSP = new System.Windows.Forms.Button();
            this.buttonExitICSP = new System.Windows.Forms.Button();
            this.buttonLoadExec = new System.Windows.Forms.Button();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.checkBoxErase = new System.Windows.Forms.CheckBox();
            this.buttonLoadFlash = new System.Windows.Forms.Button();
            this.buttonBrowseFlash = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxFlashFile = new System.Windows.Forms.TextBox();
            this.buttonBrowseExec = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxExecFile = new System.Windows.Forms.TextBox();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.groupInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.panelReadPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericPageAddress)).BeginInit();
            this.panelReadWord.SuspendLayout();
            this.panelErase.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericBlockAddress)).BeginInit();
            this.panelBlankCheck.SuspendLayout();
            this.panelBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // comboSerialPort
            // 
            this.comboSerialPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboSerialPort.FormattingEnabled = true;
            this.comboSerialPort.Location = new System.Drawing.Point(16, 46);
            this.comboSerialPort.Name = "comboSerialPort";
            this.comboSerialPort.Size = new System.Drawing.Size(136, 33);
            this.comboSerialPort.TabIndex = 1;
            this.comboSerialPort.SelectedIndexChanged += new System.EventHandler(this.comboSerialPort_SelectedIndexChanged);
            // 
            // buttonConnect
            // 
            this.buttonConnect.Location = new System.Drawing.Point(16, 97);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(283, 36);
            this.buttonConnect.TabIndex = 4;
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
            this.labelDeviceInfo1.TabIndex = 1;
            this.labelDeviceInfo1.Text = "Device ID: 0x4072";
            // 
            // labelExecVersion
            // 
            this.labelExecVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelExecVersion.AutoSize = true;
            this.labelExecVersion.Location = new System.Drawing.Point(16, 1011);
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
            this.groupInfo.TabIndex = 5;
            this.groupInfo.TabStop = false;
            this.groupInfo.Text = "Device information";
            // 
            // labelDevinfoNone
            // 
            this.labelDevinfoNone.Location = new System.Drawing.Point(174, 21);
            this.labelDevinfoNone.Name = "labelDevinfoNone";
            this.labelDevinfoNone.Size = new System.Drawing.Size(88, 38);
            this.labelDevinfoNone.TabIndex = 0;
            this.labelDevinfoNone.Text = "No data";
            this.labelDevinfoNone.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelDeviceInfo2
            // 
            this.labelDeviceInfo2.AutoSize = true;
            this.labelDeviceInfo2.Location = new System.Drawing.Point(14, 67);
            this.labelDeviceInfo2.Name = "labelDeviceInfo2";
            this.labelDeviceInfo2.Size = new System.Drawing.Size(204, 25);
            this.labelDeviceInfo2.TabIndex = 2;
            this.labelDeviceInfo2.Text = "Model: PIC24FJ64GB002";
            // 
            // comboBaudRate
            // 
            this.comboBaudRate.FormattingEnabled = true;
            this.comboBaudRate.Location = new System.Drawing.Point(158, 46);
            this.comboBaudRate.Name = "comboBaudRate";
            this.comboBaudRate.Size = new System.Drawing.Size(141, 33);
            this.comboBaudRate.TabIndex = 3;
            this.comboBaudRate.SelectedIndexChanged += new System.EventHandler(this.comboBaudRate_SelectedIndexChanged);
            // 
            // textBoxLog
            // 
            this.textBoxLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxLog.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.textBoxLog.Location = new System.Drawing.Point(0, 0);
            this.textBoxLog.Multiline = true;
            this.textBoxLog.Name = "textBoxLog";
            this.textBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxLog.Size = new System.Drawing.Size(1268, 1055);
            this.textBoxLog.TabIndex = 8;
            this.textBoxLog.WordWrap = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 25);
            this.label1.TabIndex = 0;
            this.label1.Text = "Port";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(158, 18);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(87, 25);
            this.label2.TabIndex = 2;
            this.label2.Text = "Baud rate";
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer.IsSplitterFixed = true;
            this.splitContainer.Location = new System.Drawing.Point(0, 0);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.panelReadPage);
            this.splitContainer.Panel1.Controls.Add(this.panelReadWord);
            this.splitContainer.Panel1.Controls.Add(this.panelErase);
            this.splitContainer.Panel1.Controls.Add(this.panelBlankCheck);
            this.splitContainer.Panel1.Controls.Add(this.buttonDebugMode);
            this.splitContainer.Panel1.Controls.Add(this.labelExecVersion);
            this.splitContainer.Panel1.Controls.Add(this.buttonCheckExec);
            this.splitContainer.Panel1.Controls.Add(this.buttonEnterEICSP);
            this.splitContainer.Panel1.Controls.Add(this.buttonEnterICSP);
            this.splitContainer.Panel1.Controls.Add(this.buttonExitICSP);
            this.splitContainer.Panel1.Controls.Add(this.label2);
            this.splitContainer.Panel1.Controls.Add(this.groupInfo);
            this.splitContainer.Panel1.Controls.Add(this.comboBaudRate);
            this.splitContainer.Panel1.Controls.Add(this.buttonConnect);
            this.splitContainer.Panel1.Controls.Add(this.label1);
            this.splitContainer.Panel1.Controls.Add(this.comboSerialPort);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.textBoxLog);
            this.splitContainer.Size = new System.Drawing.Size(1587, 1055);
            this.splitContainer.SplitterDistance = 315;
            this.splitContainer.TabIndex = 1;
            // 
            // panelReadPage
            // 
            this.panelReadPage.Controls.Add(this.numericPageAddress);
            this.panelReadPage.Controls.Add(this.buttonReadPage);
            this.panelReadPage.Controls.Add(this.label8);
            this.panelReadPage.Location = new System.Drawing.Point(3, 708);
            this.panelReadPage.Name = "panelReadPage";
            this.panelReadPage.Size = new System.Drawing.Size(308, 97);
            this.panelReadPage.TabIndex = 3;
            // 
            // numericPageAddress
            // 
            this.numericPageAddress.Hexadecimal = true;
            this.numericPageAddress.Increment = new decimal(new int[] {
            128,
            0,
            0,
            0});
            this.numericPageAddress.Location = new System.Drawing.Point(180, 50);
            this.numericPageAddress.Maximum = new decimal(new int[] {
            16777216,
            0,
            0,
            0});
            this.numericPageAddress.Name = "numericPageAddress";
            this.numericPageAddress.Size = new System.Drawing.Size(112, 31);
            this.numericPageAddress.TabIndex = 5;
            this.numericPageAddress.ValueChanged += new System.EventHandler(this.numericPageAddress_ValueChanged);
            // 
            // buttonReadPage
            // 
            this.buttonReadPage.Location = new System.Drawing.Point(13, 4);
            this.buttonReadPage.Name = "buttonReadPage";
            this.buttonReadPage.Size = new System.Drawing.Size(283, 40);
            this.buttonReadPage.TabIndex = 0;
            this.buttonReadPage.Text = "Read page";
            this.buttonReadPage.UseVisualStyleBackColor = true;
            this.buttonReadPage.Click += new System.EventHandler(this.buttonReadPage_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(13, 52);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(158, 25);
            this.label8.TabIndex = 1;
            this.label8.Text = "Hex page address:";
            // 
            // panelReadWord
            // 
            this.panelReadWord.Controls.Add(this.textBoxWordAddress);
            this.panelReadWord.Controls.Add(this.buttonReadWord);
            this.panelReadWord.Controls.Add(this.label7);
            this.panelReadWord.Location = new System.Drawing.Point(3, 607);
            this.panelReadWord.Name = "panelReadWord";
            this.panelReadWord.Size = new System.Drawing.Size(308, 95);
            this.panelReadWord.TabIndex = 1;
            // 
            // textBoxWordAddress
            // 
            this.textBoxWordAddress.Location = new System.Drawing.Point(155, 50);
            this.textBoxWordAddress.Name = "textBoxWordAddress";
            this.textBoxWordAddress.Size = new System.Drawing.Size(137, 31);
            this.textBoxWordAddress.TabIndex = 2;
            this.textBoxWordAddress.TextChanged += new System.EventHandler(this.textBoxWordAddress_TextChanged);
            // 
            // buttonReadWord
            // 
            this.buttonReadWord.Location = new System.Drawing.Point(13, 4);
            this.buttonReadWord.Name = "buttonReadWord";
            this.buttonReadWord.Size = new System.Drawing.Size(283, 40);
            this.buttonReadWord.TabIndex = 0;
            this.buttonReadWord.Text = "Read word";
            this.buttonReadWord.UseVisualStyleBackColor = true;
            this.buttonReadWord.Click += new System.EventHandler(this.buttonReadWord_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(13, 53);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(127, 25);
            this.label7.TabIndex = 1;
            this.label7.Text = "Word address:";
            // 
            // panelErase
            // 
            this.panelErase.Controls.Add(this.numericBlockAddress);
            this.panelErase.Controls.Add(this.buttonEraseExec);
            this.panelErase.Controls.Add(this.buttonEraseChip);
            this.panelErase.Controls.Add(this.buttonEraseBlock);
            this.panelErase.Controls.Add(this.label5);
            this.panelErase.Location = new System.Drawing.Point(3, 811);
            this.panelErase.Name = "panelErase";
            this.panelErase.Size = new System.Drawing.Size(308, 200);
            this.panelErase.TabIndex = 13;
            // 
            // numericBlockAddress
            // 
            this.numericBlockAddress.Hexadecimal = true;
            this.numericBlockAddress.Increment = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            this.numericBlockAddress.Location = new System.Drawing.Point(180, 145);
            this.numericBlockAddress.Maximum = new decimal(new int[] {
            43008,
            0,
            0,
            0});
            this.numericBlockAddress.Name = "numericBlockAddress";
            this.numericBlockAddress.Size = new System.Drawing.Size(112, 31);
            this.numericBlockAddress.TabIndex = 4;
            this.numericBlockAddress.ValueChanged += new System.EventHandler(this.numericBlockAddress_ValueChanged);
            // 
            // buttonEraseExec
            // 
            this.buttonEraseExec.Location = new System.Drawing.Point(13, 49);
            this.buttonEraseExec.Name = "buttonEraseExec";
            this.buttonEraseExec.Size = new System.Drawing.Size(283, 40);
            this.buttonEraseExec.TabIndex = 2;
            this.buttonEraseExec.Text = "Erase programming executive";
            this.buttonEraseExec.UseVisualStyleBackColor = true;
            this.buttonEraseExec.Click += new System.EventHandler(this.buttonEraseExec_Click);
            // 
            // buttonEraseChip
            // 
            this.buttonEraseChip.Location = new System.Drawing.Point(13, 3);
            this.buttonEraseChip.Name = "buttonEraseChip";
            this.buttonEraseChip.Size = new System.Drawing.Size(283, 40);
            this.buttonEraseChip.TabIndex = 1;
            this.buttonEraseChip.Text = "Erase chip";
            this.buttonEraseChip.UseVisualStyleBackColor = true;
            this.buttonEraseChip.Click += new System.EventHandler(this.buttonEraseChip_Click);
            // 
            // buttonEraseBlock
            // 
            this.buttonEraseBlock.Location = new System.Drawing.Point(13, 95);
            this.buttonEraseBlock.Name = "buttonEraseBlock";
            this.buttonEraseBlock.Size = new System.Drawing.Size(283, 40);
            this.buttonEraseBlock.TabIndex = 2;
            this.buttonEraseBlock.Text = "Erase block";
            this.buttonEraseBlock.UseVisualStyleBackColor = true;
            this.buttonEraseBlock.Click += new System.EventHandler(this.buttonEraseBlock_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(13, 147);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(161, 25);
            this.label5.TabIndex = 3;
            this.label5.Text = "Hex block address:";
            // 
            // panelBlankCheck
            // 
            this.panelBlankCheck.Controls.Add(this.textBoxBlankSize);
            this.panelBlankCheck.Controls.Add(this.buttonBlankCheck);
            this.panelBlankCheck.Controls.Add(this.label6);
            this.panelBlankCheck.Location = new System.Drawing.Point(3, 507);
            this.panelBlankCheck.Name = "panelBlankCheck";
            this.panelBlankCheck.Size = new System.Drawing.Size(308, 94);
            this.panelBlankCheck.TabIndex = 11;
            // 
            // textBoxBlankSize
            // 
            this.textBoxBlankSize.Location = new System.Drawing.Point(155, 50);
            this.textBoxBlankSize.Name = "textBoxBlankSize";
            this.textBoxBlankSize.Size = new System.Drawing.Size(137, 31);
            this.textBoxBlankSize.TabIndex = 2;
            this.textBoxBlankSize.TextChanged += new System.EventHandler(this.textBoxBlankSize_TextChanged);
            // 
            // buttonBlankCheck
            // 
            this.buttonBlankCheck.Location = new System.Drawing.Point(13, 4);
            this.buttonBlankCheck.Name = "buttonBlankCheck";
            this.buttonBlankCheck.Size = new System.Drawing.Size(283, 40);
            this.buttonBlankCheck.TabIndex = 0;
            this.buttonBlankCheck.Text = "Blank check";
            this.buttonBlankCheck.UseVisualStyleBackColor = true;
            this.buttonBlankCheck.Click += new System.EventHandler(this.buttonBlankCheck_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(13, 53);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(117, 25);
            this.label6.TabIndex = 1;
            this.label6.Text = "Memory size:";
            // 
            // buttonDebugMode
            // 
            this.buttonDebugMode.Location = new System.Drawing.Point(16, 460);
            this.buttonDebugMode.Name = "buttonDebugMode";
            this.buttonDebugMode.Size = new System.Drawing.Size(283, 40);
            this.buttonDebugMode.TabIndex = 10;
            this.buttonDebugMode.Text = "Enable debug mode";
            this.buttonDebugMode.UseVisualStyleBackColor = true;
            this.buttonDebugMode.Click += new System.EventHandler(this.buttonDebugMode_Click);
            // 
            // buttonCheckExec
            // 
            this.buttonCheckExec.Location = new System.Drawing.Point(16, 272);
            this.buttonCheckExec.Name = "buttonCheckExec";
            this.buttonCheckExec.Size = new System.Drawing.Size(283, 40);
            this.buttonCheckExec.TabIndex = 6;
            this.buttonCheckExec.Text = "Check application ID";
            this.buttonCheckExec.UseVisualStyleBackColor = true;
            this.buttonCheckExec.Click += new System.EventHandler(this.buttonCheckExec_Click);
            // 
            // buttonEnterEICSP
            // 
            this.buttonEnterEICSP.Location = new System.Drawing.Point(16, 413);
            this.buttonEnterEICSP.Name = "buttonEnterEICSP";
            this.buttonEnterEICSP.Size = new System.Drawing.Size(283, 40);
            this.buttonEnterEICSP.TabIndex = 9;
            this.buttonEnterEICSP.Text = "Enter enhanced ICSP mode";
            this.buttonEnterEICSP.UseVisualStyleBackColor = true;
            this.buttonEnterEICSP.Click += new System.EventHandler(this.ButtonEnterEICSP_Click);
            // 
            // buttonEnterICSP
            // 
            this.buttonEnterICSP.Location = new System.Drawing.Point(16, 366);
            this.buttonEnterICSP.Name = "buttonEnterICSP";
            this.buttonEnterICSP.Size = new System.Drawing.Size(283, 40);
            this.buttonEnterICSP.TabIndex = 8;
            this.buttonEnterICSP.Text = "Enter ICSP mode";
            this.buttonEnterICSP.UseVisualStyleBackColor = true;
            this.buttonEnterICSP.Click += new System.EventHandler(this.ButtonEnterICSP_Click);
            // 
            // buttonExitICSP
            // 
            this.buttonExitICSP.Location = new System.Drawing.Point(16, 319);
            this.buttonExitICSP.Name = "buttonExitICSP";
            this.buttonExitICSP.Size = new System.Drawing.Size(283, 40);
            this.buttonExitICSP.TabIndex = 7;
            this.buttonExitICSP.Text = "Exit ICSP mode";
            this.buttonExitICSP.UseVisualStyleBackColor = true;
            this.buttonExitICSP.Click += new System.EventHandler(this.ButtonExitICSP_Click);
            // 
            // buttonLoadExec
            // 
            this.buttonLoadExec.Location = new System.Drawing.Point(16, 101);
            this.buttonLoadExec.Name = "buttonLoadExec";
            this.buttonLoadExec.Size = new System.Drawing.Size(283, 40);
            this.buttonLoadExec.TabIndex = 11;
            this.buttonLoadExec.Text = "Load programming executive";
            this.buttonLoadExec.UseVisualStyleBackColor = true;
            this.buttonLoadExec.Click += new System.EventHandler(this.ButtonLoadExec_Click);
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.checkBoxErase);
            this.panelBottom.Controls.Add(this.buttonLoadFlash);
            this.panelBottom.Controls.Add(this.buttonBrowseFlash);
            this.panelBottom.Controls.Add(this.label4);
            this.panelBottom.Controls.Add(this.textBoxFlashFile);
            this.panelBottom.Controls.Add(this.buttonBrowseExec);
            this.panelBottom.Controls.Add(this.label3);
            this.panelBottom.Controls.Add(this.textBoxExecFile);
            this.panelBottom.Controls.Add(this.buttonLoadExec);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 1055);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(1587, 160);
            this.panelBottom.TabIndex = 12;
            // 
            // checkBoxErase
            // 
            this.checkBoxErase.AutoSize = true;
            this.checkBoxErase.Location = new System.Drawing.Point(20, 20);
            this.checkBoxErase.Name = "checkBoxErase";
            this.checkBoxErase.Size = new System.Drawing.Size(243, 29);
            this.checkBoxErase.TabIndex = 13;
            this.checkBoxErase.Text = "Erase flash before loading";
            this.checkBoxErase.UseVisualStyleBackColor = true;
            // 
            // buttonLoadFlash
            // 
            this.buttonLoadFlash.Location = new System.Drawing.Point(16, 55);
            this.buttonLoadFlash.Name = "buttonLoadFlash";
            this.buttonLoadFlash.Size = new System.Drawing.Size(283, 40);
            this.buttonLoadFlash.TabIndex = 12;
            this.buttonLoadFlash.Text = "Load flash";
            this.buttonLoadFlash.UseVisualStyleBackColor = true;
            this.buttonLoadFlash.Click += new System.EventHandler(this.buttonLoadFlash_Click);
            // 
            // buttonBrowseFlash
            // 
            this.buttonBrowseFlash.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBrowseFlash.Location = new System.Drawing.Point(1515, 110);
            this.buttonBrowseFlash.Name = "buttonBrowseFlash";
            this.buttonBrowseFlash.Size = new System.Drawing.Size(46, 31);
            this.buttonBrowseFlash.TabIndex = 5;
            this.buttonBrowseFlash.Text = "...";
            this.buttonBrowseFlash.UseVisualStyleBackColor = true;
            this.buttonBrowseFlash.Click += new System.EventHandler(this.buttonBrowseFlash_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(319, 20);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(196, 25);
            this.label4.TabIndex = 4;
            this.label4.Text = "Code memory flash file";
            // 
            // textBoxFlashFile
            // 
            this.textBoxFlashFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxFlashFile.Location = new System.Drawing.Point(319, 48);
            this.textBoxFlashFile.Name = "textBoxFlashFile";
            this.textBoxFlashFile.Size = new System.Drawing.Size(1190, 31);
            this.textBoxFlashFile.TabIndex = 3;
            this.textBoxFlashFile.TextChanged += new System.EventHandler(this.textBoxFlashFile_TextChanged);
            // 
            // buttonBrowseExec
            // 
            this.buttonBrowseExec.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBrowseExec.Location = new System.Drawing.Point(1515, 48);
            this.buttonBrowseExec.Name = "buttonBrowseExec";
            this.buttonBrowseExec.Size = new System.Drawing.Size(46, 31);
            this.buttonBrowseExec.TabIndex = 2;
            this.buttonBrowseExec.Text = "...";
            this.buttonBrowseExec.UseVisualStyleBackColor = true;
            this.buttonBrowseExec.Click += new System.EventHandler(this.buttonBrowseExec_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(315, 82);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(227, 25);
            this.label3.TabIndex = 1;
            this.label3.Text = "Programming executive file";
            // 
            // textBoxExecFile
            // 
            this.textBoxExecFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxExecFile.Location = new System.Drawing.Point(319, 110);
            this.textBoxExecFile.Name = "textBoxExecFile";
            this.textBoxExecFile.Size = new System.Drawing.Size(1190, 31);
            this.textBoxExecFile.TabIndex = 0;
            this.textBoxExecFile.TextChanged += new System.EventHandler(this.textBoxExecFile_TextChanged);
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "openFileDialog";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1587, 1215);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.panelBottom);
            this.MinimumSize = new System.Drawing.Size(1024, 1270);
            this.Name = "MainForm";
            this.Text = "PIC24F Flash Programmer";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.groupInfo.ResumeLayout(false);
            this.groupInfo.PerformLayout();
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel1.PerformLayout();
            this.splitContainer.Panel2.ResumeLayout(false);
            this.splitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.panelReadPage.ResumeLayout(false);
            this.panelReadPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericPageAddress)).EndInit();
            this.panelReadWord.ResumeLayout(false);
            this.panelReadWord.PerformLayout();
            this.panelErase.ResumeLayout(false);
            this.panelErase.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericBlockAddress)).EndInit();
            this.panelBlankCheck.ResumeLayout(false);
            this.panelBlankCheck.PerformLayout();
            this.panelBottom.ResumeLayout(false);
            this.panelBottom.PerformLayout();
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
        private SplitContainer splitContainer;
        private Button buttonLoadExec;
        private Button buttonEnterEICSP;
        private Button buttonEnterICSP;
        private Button buttonExitICSP;
        private Button buttonCheckExec;
        private Label labelDeviceInfo2;
        private Label labelDevinfoNone;
        private Button buttonDebugMode;
        private Panel panelBottom;
        private Button buttonLoadFlash;
        private Button buttonBrowseFlash;
        private Label label4;
        private TextBox textBoxFlashFile;
        private Button buttonBrowseExec;
        private Label label3;
        private TextBox textBoxExecFile;
        private CheckBox checkBoxErase;
        private OpenFileDialog openFileDialog;
        private Panel panelBlankCheck;
        private TextBox textBoxBlankSize;
        private Button buttonBlankCheck;
        private Label label6;
        private Panel panelErase;
        private Button buttonEraseChip;
        private Button buttonEraseBlock;
        private Label label5;
        private Button buttonEraseExec;
        private NumericUpDown numericBlockAddress;
        private Panel panelReadWord;
        private TextBox textBoxWordAddress;
        private Button buttonReadWord;
        private Label label7;
        private Panel panelReadPage;
        private Button buttonReadPage;
        private Label label8;
        private NumericUpDown numericPageAddress;
    }
}