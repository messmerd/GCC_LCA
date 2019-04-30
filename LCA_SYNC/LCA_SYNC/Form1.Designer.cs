namespace LCA_SYNC
{
    partial class Main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.buttonSync = new System.Windows.Forms.Button();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.StatusPage = new System.Windows.Forms.TabPage();
            this.label4 = new System.Windows.Forms.Label();
            this.buttonStatusStartStop = new System.Windows.Forms.Button();
            this.labelStatusDataFile = new System.Windows.Forms.Label();
            this.labelStatusElapsedTime = new System.Windows.Forms.Label();
            this.labelStatusMode = new System.Windows.Forms.Label();
            this.labelStatusTotalSensors = new System.Windows.Forms.Label();
            this.labelStatusSerialPort = new System.Windows.Forms.Label();
            this.labelStatusPackageName = new System.Windows.Forms.Label();
            this.ConfigPage = new System.Windows.Forms.TabPage();
            this.panelStartDelay = new System.Windows.Forms.Panel();
            this.labelStartDelay = new System.Windows.Forms.Label();
            this.radioButtonStartDelayNone = new System.Windows.Forms.RadioButton();
            this.radioButtonStartDelayThreeMin = new System.Windows.Forms.RadioButton();
            this.radioButtonStartDelayOneMin = new System.Windows.Forms.RadioButton();
            this.labelConfigSec2 = new System.Windows.Forms.Label();
            this.radioButtonUseCustomTime = new System.Windows.Forms.RadioButton();
            this.radioButtonUseSysTime = new System.Windows.Forms.RadioButton();
            this.checkBoxSyncTimeDate = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.labelPackageName = new System.Windows.Forms.Label();
            this.textBoxPackageName = new System.Windows.Forms.TextBox();
            this.labelConfigSec = new System.Windows.Forms.Label();
            this.labelConfigMin = new System.Windows.Forms.Label();
            this.labelConfigHr = new System.Windows.Forms.Label();
            this.buttonConfigDiscardChanges = new System.Windows.Forms.Button();
            this.dateTimePickerCustomTime = new System.Windows.Forms.DateTimePicker();
            this.numericUpDownTestDurationSeconds = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownTestDurationMinutes = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownTestDurationHours = new System.Windows.Forms.NumericUpDown();
            this.labelTestDuration = new System.Windows.Forms.Label();
            this.numericUpDownSampleRate = new System.Windows.Forms.NumericUpDown();
            this.labelSamplePeriod = new System.Windows.Forms.Label();
            this.SensorsPage = new System.Windows.Forms.TabPage();
            this.DataPage = new System.Windows.Forms.TabPage();
            this.buttonConnectDisconnect = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.temperatureUnitsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.kToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dateFormatToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mMDDYYYYToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dDMMYYYYToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timeFormatToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.TwelveHourToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.TwentyFourHourToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.LanguageIcons = new System.Windows.Forms.ImageList(this.components);
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.imageComboLanguage = new System.Windows.Forms.ImageCombo();
            this.textBoxDebugWindow = new System.Windows.Forms.TextBox();
            this.buttonClearWindow = new System.Windows.Forms.Button();
            this.arduinoList = new System.Windows.Forms.ComboBox();
            this.buttonArduinoSync = new System.Windows.Forms.Button();
            this.mainBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.serialInterfaceBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.tabControl.SuspendLayout();
            this.StatusPage.SuspendLayout();
            this.ConfigPage.SuspendLayout();
            this.panelStartDelay.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTestDurationSeconds)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTestDurationMinutes)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTestDurationHours)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSampleRate)).BeginInit();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mainBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.serialInterfaceBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonSync
            // 
            this.buttonSync.Location = new System.Drawing.Point(182, 358);
            this.buttonSync.Name = "buttonSync";
            this.buttonSync.Size = new System.Drawing.Size(148, 23);
            this.buttonSync.TabIndex = 0;
            this.buttonSync.TabStop = false;
            this.buttonSync.Text = "Sync";
            this.buttonSync.UseVisualStyleBackColor = true;
            this.buttonSync.Click += new System.EventHandler(this.buttonSync_Click);
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.StatusPage);
            this.tabControl.Controls.Add(this.ConfigPage);
            this.tabControl.Controls.Add(this.SensorsPage);
            this.tabControl.Controls.Add(this.DataPage);
            this.tabControl.Location = new System.Drawing.Point(23, 45);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(466, 307);
            this.tabControl.TabIndex = 2;
            // 
            // StatusPage
            // 
            this.StatusPage.Controls.Add(this.label4);
            this.StatusPage.Controls.Add(this.buttonStatusStartStop);
            this.StatusPage.Controls.Add(this.labelStatusDataFile);
            this.StatusPage.Controls.Add(this.labelStatusElapsedTime);
            this.StatusPage.Controls.Add(this.labelStatusMode);
            this.StatusPage.Controls.Add(this.labelStatusTotalSensors);
            this.StatusPage.Controls.Add(this.labelStatusSerialPort);
            this.StatusPage.Controls.Add(this.labelStatusPackageName);
            this.StatusPage.Location = new System.Drawing.Point(4, 22);
            this.StatusPage.Name = "StatusPage";
            this.StatusPage.Padding = new System.Windows.Forms.Padding(3);
            this.StatusPage.Size = new System.Drawing.Size(458, 281);
            this.StatusPage.TabIndex = 0;
            this.StatusPage.Text = "Status";
            this.StatusPage.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label4.Location = new System.Drawing.Point(-5, 169);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(462, 2);
            this.label4.TabIndex = 28;
            // 
            // buttonStatusStartStop
            // 
            this.buttonStatusStartStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonStatusStartStop.AutoSize = true;
            this.buttonStatusStartStop.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.buttonStatusStartStop.Location = new System.Drawing.Point(365, 191);
            this.buttonStatusStartStop.Name = "buttonStatusStartStop";
            this.buttonStatusStartStop.Size = new System.Drawing.Size(63, 23);
            this.buttonStatusStartStop.TabIndex = 6;
            this.buttonStatusStartStop.Text = "Start Test";
            this.buttonStatusStartStop.UseVisualStyleBackColor = true;
            this.buttonStatusStartStop.Click += new System.EventHandler(this.buttonStatusStartStop_Click);
            // 
            // labelStatusDataFile
            // 
            this.labelStatusDataFile.AutoSize = true;
            this.labelStatusDataFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStatusDataFile.Location = new System.Drawing.Point(24, 228);
            this.labelStatusDataFile.Name = "labelStatusDataFile";
            this.labelStatusDataFile.Size = new System.Drawing.Size(180, 24);
            this.labelStatusDataFile.TabIndex = 5;
            this.labelStatusDataFile.Text = "Current Data File: __";
            // 
            // labelStatusElapsedTime
            // 
            this.labelStatusElapsedTime.AutoSize = true;
            this.labelStatusElapsedTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStatusElapsedTime.Location = new System.Drawing.Point(24, 191);
            this.labelStatusElapsedTime.Name = "labelStatusElapsedTime";
            this.labelStatusElapsedTime.Size = new System.Drawing.Size(182, 24);
            this.labelStatusElapsedTime.TabIndex = 4;
            this.labelStatusElapsedTime.Text = "Elapsed Time: __/__";
            // 
            // labelStatusMode
            // 
            this.labelStatusMode.AutoSize = true;
            this.labelStatusMode.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStatusMode.Location = new System.Drawing.Point(249, 62);
            this.labelStatusMode.Name = "labelStatusMode";
            this.labelStatusMode.Size = new System.Drawing.Size(69, 24);
            this.labelStatusMode.TabIndex = 3;
            this.labelStatusMode.Text = "Mode: ";
            // 
            // labelStatusTotalSensors
            // 
            this.labelStatusTotalSensors.AutoSize = true;
            this.labelStatusTotalSensors.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStatusTotalSensors.Location = new System.Drawing.Point(24, 62);
            this.labelStatusTotalSensors.Name = "labelStatusTotalSensors";
            this.labelStatusTotalSensors.Size = new System.Drawing.Size(155, 24);
            this.labelStatusTotalSensors.TabIndex = 2;
            this.labelStatusTotalSensors.Text = "Total Sensors: __";
            // 
            // labelStatusSerialPort
            // 
            this.labelStatusSerialPort.AutoSize = true;
            this.labelStatusSerialPort.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStatusSerialPort.Location = new System.Drawing.Point(247, 19);
            this.labelStatusSerialPort.Name = "labelStatusSerialPort";
            this.labelStatusSerialPort.Size = new System.Drawing.Size(140, 31);
            this.labelStatusSerialPort.TabIndex = 1;
            this.labelStatusSerialPort.Text = "Serial Port";
            // 
            // labelStatusPackageName
            // 
            this.labelStatusPackageName.AutoSize = true;
            this.labelStatusPackageName.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStatusPackageName.Location = new System.Drawing.Point(20, 19);
            this.labelStatusPackageName.Name = "labelStatusPackageName";
            this.labelStatusPackageName.Size = new System.Drawing.Size(199, 31);
            this.labelStatusPackageName.TabIndex = 0;
            this.labelStatusPackageName.Text = "Package Name";
            this.labelStatusPackageName.Click += new System.EventHandler(this.label4_Click);
            // 
            // ConfigPage
            // 
            this.ConfigPage.Controls.Add(this.panelStartDelay);
            this.ConfigPage.Controls.Add(this.labelConfigSec2);
            this.ConfigPage.Controls.Add(this.radioButtonUseCustomTime);
            this.ConfigPage.Controls.Add(this.radioButtonUseSysTime);
            this.ConfigPage.Controls.Add(this.checkBoxSyncTimeDate);
            this.ConfigPage.Controls.Add(this.label3);
            this.ConfigPage.Controls.Add(this.label1);
            this.ConfigPage.Controls.Add(this.labelPackageName);
            this.ConfigPage.Controls.Add(this.textBoxPackageName);
            this.ConfigPage.Controls.Add(this.labelConfigSec);
            this.ConfigPage.Controls.Add(this.labelConfigMin);
            this.ConfigPage.Controls.Add(this.labelConfigHr);
            this.ConfigPage.Controls.Add(this.buttonConfigDiscardChanges);
            this.ConfigPage.Controls.Add(this.dateTimePickerCustomTime);
            this.ConfigPage.Controls.Add(this.numericUpDownTestDurationSeconds);
            this.ConfigPage.Controls.Add(this.numericUpDownTestDurationMinutes);
            this.ConfigPage.Controls.Add(this.numericUpDownTestDurationHours);
            this.ConfigPage.Controls.Add(this.labelTestDuration);
            this.ConfigPage.Controls.Add(this.numericUpDownSampleRate);
            this.ConfigPage.Controls.Add(this.labelSamplePeriod);
            this.ConfigPage.Location = new System.Drawing.Point(4, 22);
            this.ConfigPage.Name = "ConfigPage";
            this.ConfigPage.Padding = new System.Windows.Forms.Padding(3);
            this.ConfigPage.Size = new System.Drawing.Size(458, 281);
            this.ConfigPage.TabIndex = 1;
            this.ConfigPage.Text = "Config";
            this.ConfigPage.UseVisualStyleBackColor = true;
            // 
            // panelStartDelay
            // 
            this.panelStartDelay.Controls.Add(this.labelStartDelay);
            this.panelStartDelay.Controls.Add(this.radioButtonStartDelayNone);
            this.panelStartDelay.Controls.Add(this.radioButtonStartDelayThreeMin);
            this.panelStartDelay.Controls.Add(this.radioButtonStartDelayOneMin);
            this.panelStartDelay.Location = new System.Drawing.Point(323, 104);
            this.panelStartDelay.Name = "panelStartDelay";
            this.panelStartDelay.Size = new System.Drawing.Size(110, 73);
            this.panelStartDelay.TabIndex = 36;
            // 
            // labelStartDelay
            // 
            this.labelStartDelay.AutoSize = true;
            this.labelStartDelay.Location = new System.Drawing.Point(14, 0);
            this.labelStartDelay.Name = "labelStartDelay";
            this.labelStartDelay.Size = new System.Drawing.Size(59, 13);
            this.labelStartDelay.TabIndex = 36;
            this.labelStartDelay.Text = "Start Delay";
            // 
            // radioButtonStartDelayNone
            // 
            this.radioButtonStartDelayNone.AutoSize = true;
            this.radioButtonStartDelayNone.Checked = true;
            this.radioButtonStartDelayNone.Location = new System.Drawing.Point(14, 18);
            this.radioButtonStartDelayNone.Name = "radioButtonStartDelayNone";
            this.radioButtonStartDelayNone.Size = new System.Drawing.Size(51, 17);
            this.radioButtonStartDelayNone.TabIndex = 33;
            this.radioButtonStartDelayNone.TabStop = true;
            this.radioButtonStartDelayNone.Text = "None";
            this.radioButtonStartDelayNone.UseVisualStyleBackColor = true;
            // 
            // radioButtonStartDelayThreeMin
            // 
            this.radioButtonStartDelayThreeMin.AutoSize = true;
            this.radioButtonStartDelayThreeMin.Location = new System.Drawing.Point(14, 52);
            this.radioButtonStartDelayThreeMin.Name = "radioButtonStartDelayThreeMin";
            this.radioButtonStartDelayThreeMin.Size = new System.Drawing.Size(54, 17);
            this.radioButtonStartDelayThreeMin.TabIndex = 35;
            this.radioButtonStartDelayThreeMin.Text = "3 Min.";
            this.radioButtonStartDelayThreeMin.UseVisualStyleBackColor = true;
            // 
            // radioButtonStartDelayOneMin
            // 
            this.radioButtonStartDelayOneMin.AutoSize = true;
            this.radioButtonStartDelayOneMin.Location = new System.Drawing.Point(14, 35);
            this.radioButtonStartDelayOneMin.Name = "radioButtonStartDelayOneMin";
            this.radioButtonStartDelayOneMin.Size = new System.Drawing.Size(54, 17);
            this.radioButtonStartDelayOneMin.TabIndex = 34;
            this.radioButtonStartDelayOneMin.Text = "1 Min.";
            this.radioButtonStartDelayOneMin.UseVisualStyleBackColor = true;
            // 
            // labelConfigSec2
            // 
            this.labelConfigSec2.AutoSize = true;
            this.labelConfigSec2.Location = new System.Drawing.Point(38, 122);
            this.labelConfigSec2.Name = "labelConfigSec2";
            this.labelConfigSec2.Size = new System.Drawing.Size(29, 13);
            this.labelConfigSec2.TabIndex = 32;
            this.labelConfigSec2.Text = "Sec.";
            // 
            // radioButtonUseCustomTime
            // 
            this.radioButtonUseCustomTime.AutoSize = true;
            this.radioButtonUseCustomTime.Enabled = false;
            this.radioButtonUseCustomTime.Location = new System.Drawing.Point(69, 249);
            this.radioButtonUseCustomTime.Name = "radioButtonUseCustomTime";
            this.radioButtonUseCustomTime.Size = new System.Drawing.Size(14, 13);
            this.radioButtonUseCustomTime.TabIndex = 31;
            this.radioButtonUseCustomTime.UseVisualStyleBackColor = true;
            // 
            // radioButtonUseSysTime
            // 
            this.radioButtonUseSysTime.AutoSize = true;
            this.radioButtonUseSysTime.Checked = true;
            this.radioButtonUseSysTime.Enabled = false;
            this.radioButtonUseSysTime.Location = new System.Drawing.Point(69, 226);
            this.radioButtonUseSysTime.Name = "radioButtonUseSysTime";
            this.radioButtonUseSysTime.Size = new System.Drawing.Size(135, 17);
            this.radioButtonUseSysTime.TabIndex = 30;
            this.radioButtonUseSysTime.TabStop = true;
            this.radioButtonUseSysTime.Text = "Use System Time/Date";
            this.radioButtonUseSysTime.UseVisualStyleBackColor = true;
            // 
            // checkBoxSyncTimeDate
            // 
            this.checkBoxSyncTimeDate.AutoSize = true;
            this.checkBoxSyncTimeDate.Location = new System.Drawing.Point(41, 202);
            this.checkBoxSyncTimeDate.Name = "checkBoxSyncTimeDate";
            this.checkBoxSyncTimeDate.Size = new System.Drawing.Size(157, 17);
            this.checkBoxSyncTimeDate.TabIndex = 29;
            this.checkBoxSyncTimeDate.Text = "Synchronize Time and Date";
            this.checkBoxSyncTimeDate.UseVisualStyleBackColor = true;
            this.checkBoxSyncTimeDate.CheckedChanged += new System.EventHandler(this.checkBoxSyncTimeDate_CheckedChanged);
            // 
            // label3
            // 
            this.label3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label3.Location = new System.Drawing.Point(-5, 180);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(462, 2);
            this.label3.TabIndex = 28;
            // 
            // label1
            // 
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label1.Location = new System.Drawing.Point(-5, 87);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(462, 2);
            this.label1.TabIndex = 27;
            // 
            // labelPackageName
            // 
            this.labelPackageName.AutoSize = true;
            this.labelPackageName.Location = new System.Drawing.Point(38, 15);
            this.labelPackageName.Name = "labelPackageName";
            this.labelPackageName.Size = new System.Drawing.Size(81, 13);
            this.labelPackageName.TabIndex = 22;
            this.labelPackageName.Text = "Package Name";
            // 
            // textBoxPackageName
            // 
            this.textBoxPackageName.Location = new System.Drawing.Point(41, 50);
            this.textBoxPackageName.MaxLength = 32;
            this.textBoxPackageName.Name = "textBoxPackageName";
            this.textBoxPackageName.Size = new System.Drawing.Size(280, 20);
            this.textBoxPackageName.TabIndex = 21;
            // 
            // labelConfigSec
            // 
            this.labelConfigSec.AutoSize = true;
            this.labelConfigSec.Location = new System.Drawing.Point(253, 122);
            this.labelConfigSec.Name = "labelConfigSec";
            this.labelConfigSec.Size = new System.Drawing.Size(29, 13);
            this.labelConfigSec.TabIndex = 26;
            this.labelConfigSec.Text = "Sec.";
            // 
            // labelConfigMin
            // 
            this.labelConfigMin.AutoSize = true;
            this.labelConfigMin.Location = new System.Drawing.Point(214, 122);
            this.labelConfigMin.Name = "labelConfigMin";
            this.labelConfigMin.Size = new System.Drawing.Size(27, 13);
            this.labelConfigMin.TabIndex = 25;
            this.labelConfigMin.Text = "Min.";
            // 
            // labelConfigHr
            // 
            this.labelConfigHr.AutoSize = true;
            this.labelConfigHr.Location = new System.Drawing.Point(175, 122);
            this.labelConfigHr.Name = "labelConfigHr";
            this.labelConfigHr.Size = new System.Drawing.Size(21, 13);
            this.labelConfigHr.TabIndex = 24;
            this.labelConfigHr.Text = "Hr.";
            // 
            // buttonConfigDiscardChanges
            // 
            this.buttonConfigDiscardChanges.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonConfigDiscardChanges.AutoSize = true;
            this.buttonConfigDiscardChanges.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.buttonConfigDiscardChanges.Location = new System.Drawing.Point(323, 228);
            this.buttonConfigDiscardChanges.Name = "buttonConfigDiscardChanges";
            this.buttonConfigDiscardChanges.Size = new System.Drawing.Size(101, 23);
            this.buttonConfigDiscardChanges.TabIndex = 23;
            this.buttonConfigDiscardChanges.Text = "Discard Changes ";
            this.buttonConfigDiscardChanges.UseVisualStyleBackColor = true;
            this.buttonConfigDiscardChanges.Click += new System.EventHandler(this.buttonConfigDiscardChanges_Click);
            // 
            // dateTimePickerCustomTime
            // 
            this.dateTimePickerCustomTime.CustomFormat = "";
            this.dateTimePickerCustomTime.Enabled = false;
            this.dateTimePickerCustomTime.Location = new System.Drawing.Point(87, 245);
            this.dateTimePickerCustomTime.Name = "dateTimePickerCustomTime";
            this.dateTimePickerCustomTime.Size = new System.Drawing.Size(186, 20);
            this.dateTimePickerCustomTime.TabIndex = 19;
            // 
            // numericUpDownTestDurationSeconds
            // 
            this.numericUpDownTestDurationSeconds.Location = new System.Drawing.Point(256, 139);
            this.numericUpDownTestDurationSeconds.Maximum = new decimal(new int[] {
            59,
            0,
            0,
            0});
            this.numericUpDownTestDurationSeconds.Name = "numericUpDownTestDurationSeconds";
            this.numericUpDownTestDurationSeconds.Size = new System.Drawing.Size(33, 20);
            this.numericUpDownTestDurationSeconds.TabIndex = 18;
            this.numericUpDownTestDurationSeconds.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // numericUpDownTestDurationMinutes
            // 
            this.numericUpDownTestDurationMinutes.Location = new System.Drawing.Point(217, 139);
            this.numericUpDownTestDurationMinutes.Maximum = new decimal(new int[] {
            59,
            0,
            0,
            0});
            this.numericUpDownTestDurationMinutes.Name = "numericUpDownTestDurationMinutes";
            this.numericUpDownTestDurationMinutes.Size = new System.Drawing.Size(33, 20);
            this.numericUpDownTestDurationMinutes.TabIndex = 17;
            // 
            // numericUpDownTestDurationHours
            // 
            this.numericUpDownTestDurationHours.Location = new System.Drawing.Point(178, 139);
            this.numericUpDownTestDurationHours.Maximum = new decimal(new int[] {
            72,
            0,
            0,
            0});
            this.numericUpDownTestDurationHours.Name = "numericUpDownTestDurationHours";
            this.numericUpDownTestDurationHours.Size = new System.Drawing.Size(33, 20);
            this.numericUpDownTestDurationHours.TabIndex = 16;
            // 
            // labelTestDuration
            // 
            this.labelTestDuration.AutoSize = true;
            this.labelTestDuration.Location = new System.Drawing.Point(175, 104);
            this.labelTestDuration.Name = "labelTestDuration";
            this.labelTestDuration.Size = new System.Drawing.Size(71, 13);
            this.labelTestDuration.TabIndex = 15;
            this.labelTestDuration.Text = "Test Duration";
            // 
            // numericUpDownSampleRate
            // 
            this.numericUpDownSampleRate.DecimalPlaces = 3;
            this.numericUpDownSampleRate.Increment = new decimal(new int[] {
            125,
            0,
            0,
            196608});
            this.numericUpDownSampleRate.Location = new System.Drawing.Point(41, 139);
            this.numericUpDownSampleRate.Maximum = new decimal(new int[] {
            600,
            0,
            0,
            65536});
            this.numericUpDownSampleRate.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            196608});
            this.numericUpDownSampleRate.Name = "numericUpDownSampleRate";
            this.numericUpDownSampleRate.Size = new System.Drawing.Size(57, 20);
            this.numericUpDownSampleRate.TabIndex = 14;
            this.numericUpDownSampleRate.Value = new decimal(new int[] {
            1000,
            0,
            0,
            196608});
            // 
            // labelSamplePeriod
            // 
            this.labelSamplePeriod.AutoSize = true;
            this.labelSamplePeriod.Location = new System.Drawing.Point(38, 104);
            this.labelSamplePeriod.Name = "labelSamplePeriod";
            this.labelSamplePeriod.Size = new System.Drawing.Size(75, 13);
            this.labelSamplePeriod.TabIndex = 13;
            this.labelSamplePeriod.Text = "Sample Period";
            this.labelSamplePeriod.Click += new System.EventHandler(this.label2_Click);
            // 
            // SensorsPage
            // 
            this.SensorsPage.Location = new System.Drawing.Point(4, 22);
            this.SensorsPage.Name = "SensorsPage";
            this.SensorsPage.Size = new System.Drawing.Size(458, 281);
            this.SensorsPage.TabIndex = 2;
            this.SensorsPage.Text = "Sensors";
            this.SensorsPage.UseVisualStyleBackColor = true;
            // 
            // DataPage
            // 
            this.DataPage.Location = new System.Drawing.Point(4, 22);
            this.DataPage.Name = "DataPage";
            this.DataPage.Padding = new System.Windows.Forms.Padding(3);
            this.DataPage.Size = new System.Drawing.Size(458, 281);
            this.DataPage.TabIndex = 3;
            this.DataPage.Text = "Data";
            this.DataPage.UseVisualStyleBackColor = true;
            this.DataPage.Click += new System.EventHandler(this.tabPage1_Click);
            // 
            // buttonConnectDisconnect
            // 
            this.buttonConnectDisconnect.Location = new System.Drawing.Point(26, 358);
            this.buttonConnectDisconnect.Name = "buttonConnectDisconnect";
            this.buttonConnectDisconnect.Size = new System.Drawing.Size(148, 23);
            this.buttonConnectDisconnect.TabIndex = 2;
            this.buttonConnectDisconnect.TabStop = false;
            this.buttonConnectDisconnect.Text = "Connect/Disconnect";
            this.buttonConnectDisconnect.UseVisualStyleBackColor = true;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.optionsToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(512, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadConfigurationToolStripMenuItem,
            this.saveConfigurationToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // loadConfigurationToolStripMenuItem
            // 
            this.loadConfigurationToolStripMenuItem.Name = "loadConfigurationToolStripMenuItem";
            this.loadConfigurationToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.loadConfigurationToolStripMenuItem.Text = "Load Configuration";
            // 
            // saveConfigurationToolStripMenuItem
            // 
            this.saveConfigurationToolStripMenuItem.Name = "saveConfigurationToolStripMenuItem";
            this.saveConfigurationToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.saveConfigurationToolStripMenuItem.Text = "Save Configuration";
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.temperatureUnitsToolStripMenuItem,
            this.dateFormatToolStripMenuItem,
            this.timeFormatToolStripMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.optionsToolStripMenuItem.Text = "Options";
            // 
            // temperatureUnitsToolStripMenuItem
            // 
            this.temperatureUnitsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cToolStripMenuItem,
            this.fToolStripMenuItem,
            this.kToolStripMenuItem});
            this.temperatureUnitsToolStripMenuItem.Name = "temperatureUnitsToolStripMenuItem";
            this.temperatureUnitsToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.temperatureUnitsToolStripMenuItem.Text = "Temperature Units";
            // 
            // cToolStripMenuItem
            // 
            this.cToolStripMenuItem.Checked = true;
            this.cToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cToolStripMenuItem.Name = "cToolStripMenuItem";
            this.cToolStripMenuItem.Size = new System.Drawing.Size(82, 22);
            this.cToolStripMenuItem.Text = "C";
            this.cToolStripMenuItem.Click += new System.EventHandler(this.TempUnitsToolStripMenuItem_Click);
            // 
            // fToolStripMenuItem
            // 
            this.fToolStripMenuItem.Name = "fToolStripMenuItem";
            this.fToolStripMenuItem.Size = new System.Drawing.Size(82, 22);
            this.fToolStripMenuItem.Text = "F";
            this.fToolStripMenuItem.Click += new System.EventHandler(this.TempUnitsToolStripMenuItem_Click);
            // 
            // kToolStripMenuItem
            // 
            this.kToolStripMenuItem.Name = "kToolStripMenuItem";
            this.kToolStripMenuItem.Size = new System.Drawing.Size(82, 22);
            this.kToolStripMenuItem.Text = "K";
            this.kToolStripMenuItem.Click += new System.EventHandler(this.TempUnitsToolStripMenuItem_Click);
            // 
            // dateFormatToolStripMenuItem
            // 
            this.dateFormatToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mMDDYYYYToolStripMenuItem,
            this.dDMMYYYYToolStripMenuItem});
            this.dateFormatToolStripMenuItem.Name = "dateFormatToolStripMenuItem";
            this.dateFormatToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.dateFormatToolStripMenuItem.Text = "Date Format";
            // 
            // mMDDYYYYToolStripMenuItem
            // 
            this.mMDDYYYYToolStripMenuItem.Checked = true;
            this.mMDDYYYYToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mMDDYYYYToolStripMenuItem.Name = "mMDDYYYYToolStripMenuItem";
            this.mMDDYYYYToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.mMDDYYYYToolStripMenuItem.Text = "mm/dd/yyyy";
            this.mMDDYYYYToolStripMenuItem.Click += new System.EventHandler(this.DateFormatOptionToolStripMenuItem_Click);
            // 
            // dDMMYYYYToolStripMenuItem
            // 
            this.dDMMYYYYToolStripMenuItem.Name = "dDMMYYYYToolStripMenuItem";
            this.dDMMYYYYToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.dDMMYYYYToolStripMenuItem.Text = "dd/mm/yyyy";
            this.dDMMYYYYToolStripMenuItem.Click += new System.EventHandler(this.DateFormatOptionToolStripMenuItem_Click);
            // 
            // timeFormatToolStripMenuItem
            // 
            this.timeFormatToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TwelveHourToolStripMenuItem,
            this.TwentyFourHourToolStripMenuItem});
            this.timeFormatToolStripMenuItem.Name = "timeFormatToolStripMenuItem";
            this.timeFormatToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.timeFormatToolStripMenuItem.Text = "Time Format";
            // 
            // TwelveHourToolStripMenuItem
            // 
            this.TwelveHourToolStripMenuItem.Name = "TwelveHourToolStripMenuItem";
            this.TwelveHourToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.TwelveHourToolStripMenuItem.Text = "12-hour";
            this.TwelveHourToolStripMenuItem.Click += new System.EventHandler(this.TimeFormatOptionToolStripMenuItem_Click);
            // 
            // TwentyFourHourToolStripMenuItem
            // 
            this.TwentyFourHourToolStripMenuItem.Checked = true;
            this.TwentyFourHourToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.TwentyFourHourToolStripMenuItem.Name = "TwentyFourHourToolStripMenuItem";
            this.TwentyFourHourToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.TwentyFourHourToolStripMenuItem.Text = "24-hour";
            this.TwentyFourHourToolStripMenuItem.Click += new System.EventHandler(this.TimeFormatOptionToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // LanguageIcons
            // 
            this.LanguageIcons.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.LanguageIcons.ImageSize = new System.Drawing.Size(24, 24);
            this.LanguageIcons.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // imageComboLanguage
            // 
            this.imageComboLanguage.CausesValidation = false;
            this.imageComboLanguage.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.imageComboLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.imageComboLanguage.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.imageComboLanguage.FormattingEnabled = true;
            this.imageComboLanguage.ImageList = this.LanguageIcons;
            this.imageComboLanguage.ImageSide = System.Windows.Forms.ImageCombo.IMAGESIDE.Right;
            this.imageComboLanguage.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.imageComboLanguage.ItemHeight = 23;
            this.imageComboLanguage.Location = new System.Drawing.Point(391, 0);
            this.imageComboLanguage.Name = "imageComboLanguage";
            this.imageComboLanguage.Size = new System.Drawing.Size(121, 29);
            this.imageComboLanguage.TabIndex = 2;
            this.imageComboLanguage.TabStop = false;
            // 
            // textBoxDebugWindow
            // 
            this.textBoxDebugWindow.BackColor = System.Drawing.SystemColors.Control;
            this.textBoxDebugWindow.Location = new System.Drawing.Point(27, 387);
            this.textBoxDebugWindow.Multiline = true;
            this.textBoxDebugWindow.Name = "textBoxDebugWindow";
            this.textBoxDebugWindow.ReadOnly = true;
            this.textBoxDebugWindow.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxDebugWindow.Size = new System.Drawing.Size(458, 81);
            this.textBoxDebugWindow.TabIndex = 6;
            // 
            // buttonClearWindow
            // 
            this.buttonClearWindow.Location = new System.Drawing.Point(337, 358);
            this.buttonClearWindow.Name = "buttonClearWindow";
            this.buttonClearWindow.Size = new System.Drawing.Size(148, 23);
            this.buttonClearWindow.TabIndex = 7;
            this.buttonClearWindow.TabStop = false;
            this.buttonClearWindow.Text = "Clear Window";
            this.buttonClearWindow.UseVisualStyleBackColor = true;
            // 
            // arduinoList
            // 
            this.arduinoList.DataBindings.Add(new System.Windows.Forms.Binding("DropDownStyle", global::LCA_SYNC.Properties.Settings.Default, "DropDownList", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.arduinoList.DropDownStyle = global::LCA_SYNC.Properties.Settings.Default.DropDownList;
            this.arduinoList.FormattingEnabled = true;
            this.arduinoList.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.arduinoList.Location = new System.Drawing.Point(271, 42);
            this.arduinoList.Name = "arduinoList";
            this.arduinoList.Size = new System.Drawing.Size(184, 21);
            this.arduinoList.TabIndex = 0;
            this.arduinoList.TabStop = false;
            this.arduinoList.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // buttonArduinoSync
            // 
            this.buttonArduinoSync.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.buttonArduinoSync.BackgroundImage = global::LCA_SYNC.Properties.Resources.sync;
            this.buttonArduinoSync.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.buttonArduinoSync.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.buttonArduinoSync.Location = new System.Drawing.Point(457, 33);
            this.buttonArduinoSync.Name = "buttonArduinoSync";
            this.buttonArduinoSync.Size = new System.Drawing.Size(31, 31);
            this.buttonArduinoSync.TabIndex = 5;
            this.buttonArduinoSync.TabStop = false;
            this.buttonArduinoSync.UseVisualStyleBackColor = false;
            this.buttonArduinoSync.Click += new System.EventHandler(this.buttonArduinoSync_Click);
            // 
            // mainBindingSource
            // 
            this.mainBindingSource.DataSource = typeof(LCA_SYNC.Main);
            // 
            // serialInterfaceBindingSource
            // 
            this.serialInterfaceBindingSource.DataSource = typeof(LCA_SYNC.SerialInterface);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(512, 489);
            this.Controls.Add(this.buttonClearWindow);
            this.Controls.Add(this.textBoxDebugWindow);
            this.Controls.Add(this.imageComboLanguage);
            this.Controls.Add(this.buttonArduinoSync);
            this.Controls.Add(this.arduinoList);
            this.Controls.Add(this.buttonConnectDisconnect);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.buttonSync);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Main";
            this.Text = "Low-Cost Array Sync";
            this.tabControl.ResumeLayout(false);
            this.StatusPage.ResumeLayout(false);
            this.StatusPage.PerformLayout();
            this.ConfigPage.ResumeLayout(false);
            this.ConfigPage.PerformLayout();
            this.panelStartDelay.ResumeLayout(false);
            this.panelStartDelay.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTestDurationSeconds)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTestDurationMinutes)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTestDurationHours)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSampleRate)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mainBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.serialInterfaceBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonSync;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage StatusPage;
        private System.Windows.Forms.TabPage ConfigPage;
        private System.Windows.Forms.TabPage SensorsPage;
        private System.Windows.Forms.TabPage DataPage;
        private System.Windows.Forms.Button buttonConnectDisconnect;
        private System.Windows.Forms.Button buttonArduinoSync;
        private System.Windows.Forms.ComboBox arduinoList;
        private System.Windows.Forms.BindingSource mainBindingSource;
        private System.Windows.Forms.BindingSource serialInterfaceBindingSource;
        private System.Windows.Forms.Label labelSamplePeriod;
        private System.Windows.Forms.NumericUpDown numericUpDownSampleRate;
        private System.Windows.Forms.NumericUpDown numericUpDownTestDurationSeconds;
        private System.Windows.Forms.NumericUpDown numericUpDownTestDurationMinutes;
        private System.Windows.Forms.NumericUpDown numericUpDownTestDurationHours;
        private System.Windows.Forms.Label labelTestDuration;
        private System.Windows.Forms.DateTimePicker dateTimePickerCustomTime;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadConfigurationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveConfigurationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem temperatureUnitsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dateFormatToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem timeFormatToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ImageCombo imageComboLanguage;
        private System.Windows.Forms.ToolStripMenuItem cToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem kToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mMDDYYYYToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dDMMYYYYToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem TwelveHourToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem TwentyFourHourToolStripMenuItem;
        private System.Windows.Forms.Label labelPackageName;
        private System.Windows.Forms.TextBox textBoxPackageName;
        private System.Windows.Forms.Button buttonConfigDiscardChanges;
        private System.Windows.Forms.Label labelConfigSec;
        private System.Windows.Forms.Label labelConfigMin;
        private System.Windows.Forms.Label labelConfigHr;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxDebugWindow;
        private System.Windows.Forms.Button buttonClearWindow;
        private System.Windows.Forms.Label labelStatusPackageName;
        private System.Windows.Forms.Label labelStatusSerialPort;
        private System.Windows.Forms.Label labelStatusMode;
        private System.Windows.Forms.Label labelStatusTotalSensors;
        private System.Windows.Forms.Label labelStatusDataFile;
        private System.Windows.Forms.Label labelStatusElapsedTime;
        private System.Windows.Forms.Button buttonStatusStartStop;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.RadioButton radioButtonUseCustomTime;
        private System.Windows.Forms.RadioButton radioButtonUseSysTime;
        private System.Windows.Forms.CheckBox checkBoxSyncTimeDate;
        private System.Windows.Forms.Label labelConfigSec2;
        private System.Windows.Forms.Panel panelStartDelay;
        private System.Windows.Forms.Label labelStartDelay;
        private System.Windows.Forms.RadioButton radioButtonStartDelayNone;
        private System.Windows.Forms.RadioButton radioButtonStartDelayThreeMin;
        private System.Windows.Forms.RadioButton radioButtonStartDelayOneMin;
    }
}

