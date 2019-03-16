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
            this.buttonSync = new System.Windows.Forms.Button();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.StatusPage = new System.Windows.Forms.TabPage();
            this.ConfigPage = new System.Windows.Forms.TabPage();
            this.label2 = new System.Windows.Forms.Label();
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.numericUpDownTestDurationSeconds = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownTestDurationMinutes = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownTestDurationHours = new System.Windows.Forms.NumericUpDown();
            this.labelTestDuration = new System.Windows.Forms.Label();
            this.numericUpDownSampleRate = new System.Windows.Forms.NumericUpDown();
            this.labelSampleRate = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.radioButtonTempC = new System.Windows.Forms.RadioButton();
            this.radioButtonTempK = new System.Windows.Forms.RadioButton();
            this.radioButtonTempF = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.SensorsPage = new System.Windows.Forms.TabPage();
            this.DataPage = new System.Windows.Forms.TabPage();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.buttonConnectDisconnect = new System.Windows.Forms.Button();
            this.arduinoList = new System.Windows.Forms.ComboBox();
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
            this.buttonArduinoSync = new System.Windows.Forms.Button();
            this.mainBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.serialInterfaceBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.tabControl.SuspendLayout();
            this.ConfigPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTestDurationSeconds)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTestDurationMinutes)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTestDurationHours)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSampleRate)).BeginInit();
            this.panel1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mainBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.serialInterfaceBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonSync
            // 
            this.buttonSync.Location = new System.Drawing.Point(171, 358);
            this.buttonSync.Name = "buttonSync";
            this.buttonSync.Size = new System.Drawing.Size(75, 23);
            this.buttonSync.TabIndex = 0;
            this.buttonSync.TabStop = false;
            this.buttonSync.Text = "Sync";
            this.buttonSync.UseVisualStyleBackColor = true;
            this.buttonSync.Click += new System.EventHandler(this.button1_Click);
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
            this.tabControl.Size = new System.Drawing.Size(435, 307);
            this.tabControl.TabIndex = 2;
            // 
            // StatusPage
            // 
            this.StatusPage.Location = new System.Drawing.Point(4, 22);
            this.StatusPage.Name = "StatusPage";
            this.StatusPage.Padding = new System.Windows.Forms.Padding(3);
            this.StatusPage.Size = new System.Drawing.Size(427, 281);
            this.StatusPage.TabIndex = 0;
            this.StatusPage.Text = "Status";
            this.StatusPage.UseVisualStyleBackColor = true;
            // 
            // ConfigPage
            // 
            this.ConfigPage.Controls.Add(this.label2);
            this.ConfigPage.Controls.Add(this.dateTimePicker1);
            this.ConfigPage.Controls.Add(this.numericUpDownTestDurationSeconds);
            this.ConfigPage.Controls.Add(this.numericUpDownTestDurationMinutes);
            this.ConfigPage.Controls.Add(this.numericUpDownTestDurationHours);
            this.ConfigPage.Controls.Add(this.labelTestDuration);
            this.ConfigPage.Controls.Add(this.numericUpDownSampleRate);
            this.ConfigPage.Controls.Add(this.labelSampleRate);
            this.ConfigPage.Controls.Add(this.panel1);
            this.ConfigPage.Controls.Add(this.label1);
            this.ConfigPage.Location = new System.Drawing.Point(4, 22);
            this.ConfigPage.Name = "ConfigPage";
            this.ConfigPage.Padding = new System.Windows.Forms.Padding(3);
            this.ConfigPage.Size = new System.Drawing.Size(427, 281);
            this.ConfigPage.TabIndex = 1;
            this.ConfigPage.Text = "Config";
            this.ConfigPage.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(30, 199);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(215, 26);
            this.label2.TabIndex = 20;
            this.label2.Text = "Date/Time (in final product, \r\nprobably just sync system time automatically)";
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.CustomFormat = "";
            this.dateTimePicker1.Location = new System.Drawing.Point(51, 228);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(200, 20);
            this.dateTimePicker1.TabIndex = 19;
            // 
            // numericUpDownTestDurationSeconds
            // 
            this.numericUpDownTestDurationSeconds.Location = new System.Drawing.Point(218, 46);
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
            this.numericUpDownTestDurationMinutes.Location = new System.Drawing.Point(179, 45);
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
            this.numericUpDownTestDurationHours.Location = new System.Drawing.Point(140, 45);
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
            this.labelTestDuration.Location = new System.Drawing.Point(137, 17);
            this.labelTestDuration.Name = "labelTestDuration";
            this.labelTestDuration.Size = new System.Drawing.Size(108, 26);
            this.labelTestDuration.TabIndex = 15;
            this.labelTestDuration.Text = "Test Duration\r\nHr.        Min.       Sec.";
            // 
            // numericUpDownSampleRate
            // 
            this.numericUpDownSampleRate.DecimalPlaces = 3;
            this.numericUpDownSampleRate.Increment = new decimal(new int[] {
            125,
            0,
            0,
            196608});
            this.numericUpDownSampleRate.Location = new System.Drawing.Point(33, 34);
            this.numericUpDownSampleRate.Maximum = new decimal(new int[] {
            600,
            0,
            0,
            65536});
            this.numericUpDownSampleRate.Minimum = new decimal(new int[] {
            125,
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
            // labelSampleRate
            // 
            this.labelSampleRate.AutoSize = true;
            this.labelSampleRate.Location = new System.Drawing.Point(30, 17);
            this.labelSampleRate.Name = "labelSampleRate";
            this.labelSampleRate.Size = new System.Drawing.Size(89, 13);
            this.labelSampleRate.TabIndex = 13;
            this.labelSampleRate.Text = "Sample Period (s)";
            this.labelSampleRate.Click += new System.EventHandler(this.label2_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.radioButtonTempC);
            this.panel1.Controls.Add(this.radioButtonTempK);
            this.panel1.Controls.Add(this.radioButtonTempF);
            this.panel1.Location = new System.Drawing.Point(55, 105);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(113, 24);
            this.panel1.TabIndex = 12;
            // 
            // radioButtonTempC
            // 
            this.radioButtonTempC.AutoSize = true;
            this.radioButtonTempC.Checked = true;
            this.radioButtonTempC.Location = new System.Drawing.Point(3, 3);
            this.radioButtonTempC.Name = "radioButtonTempC";
            this.radioButtonTempC.Size = new System.Drawing.Size(32, 17);
            this.radioButtonTempC.TabIndex = 6;
            this.radioButtonTempC.TabStop = true;
            this.radioButtonTempC.Text = "C";
            this.radioButtonTempC.UseVisualStyleBackColor = true;
            // 
            // radioButtonTempK
            // 
            this.radioButtonTempK.AutoSize = true;
            this.radioButtonTempK.Location = new System.Drawing.Point(78, 3);
            this.radioButtonTempK.Name = "radioButtonTempK";
            this.radioButtonTempK.Size = new System.Drawing.Size(32, 17);
            this.radioButtonTempK.TabIndex = 8;
            this.radioButtonTempK.Text = "K";
            this.radioButtonTempK.UseVisualStyleBackColor = true;
            // 
            // radioButtonTempF
            // 
            this.radioButtonTempF.AutoSize = true;
            this.radioButtonTempF.Location = new System.Drawing.Point(41, 3);
            this.radioButtonTempF.Name = "radioButtonTempF";
            this.radioButtonTempF.Size = new System.Drawing.Size(31, 17);
            this.radioButtonTempF.TabIndex = 7;
            this.radioButtonTempF.Text = "F";
            this.radioButtonTempF.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(52, 89);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(94, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "Temperature Units";
            // 
            // SensorsPage
            // 
            this.SensorsPage.Location = new System.Drawing.Point(4, 22);
            this.SensorsPage.Name = "SensorsPage";
            this.SensorsPage.Size = new System.Drawing.Size(427, 281);
            this.SensorsPage.TabIndex = 2;
            this.SensorsPage.Text = "Sensors";
            this.SensorsPage.UseVisualStyleBackColor = true;
            // 
            // DataPage
            // 
            this.DataPage.Location = new System.Drawing.Point(4, 22);
            this.DataPage.Name = "DataPage";
            this.DataPage.Padding = new System.Windows.Forms.Padding(3);
            this.DataPage.Size = new System.Drawing.Size(427, 281);
            this.DataPage.TabIndex = 3;
            this.DataPage.Text = "Data";
            this.DataPage.UseVisualStyleBackColor = true;
            this.DataPage.Click += new System.EventHandler(this.tabPage1_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(23, 416);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(435, 23);
            this.progressBar1.TabIndex = 3;
            // 
            // buttonConnectDisconnect
            // 
            this.buttonConnectDisconnect.Location = new System.Drawing.Point(27, 358);
            this.buttonConnectDisconnect.Name = "buttonConnectDisconnect";
            this.buttonConnectDisconnect.Size = new System.Drawing.Size(124, 23);
            this.buttonConnectDisconnect.TabIndex = 2;
            this.buttonConnectDisconnect.TabStop = false;
            this.buttonConnectDisconnect.Text = "Connect/Disconnect";
            this.buttonConnectDisconnect.UseVisualStyleBackColor = true;
            // 
            // arduinoList
            // 
            this.arduinoList.DataBindings.Add(new System.Windows.Forms.Binding("DropDownStyle", global::LCA_SYNC.Properties.Settings.Default, "DropDownList", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.arduinoList.DropDownStyle = global::LCA_SYNC.Properties.Settings.Default.DropDownList;
            this.arduinoList.FormattingEnabled = true;
            this.arduinoList.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.arduinoList.Location = new System.Drawing.Point(237, 42);
            this.arduinoList.Name = "arduinoList";
            this.arduinoList.Size = new System.Drawing.Size(184, 21);
            this.arduinoList.TabIndex = 0;
            this.arduinoList.TabStop = false;
            this.arduinoList.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.optionsToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(484, 24);
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
            this.cToolStripMenuItem.Name = "cToolStripMenuItem";
            this.cToolStripMenuItem.Size = new System.Drawing.Size(82, 22);
            this.cToolStripMenuItem.Text = "C";
            // 
            // fToolStripMenuItem
            // 
            this.fToolStripMenuItem.Name = "fToolStripMenuItem";
            this.fToolStripMenuItem.Size = new System.Drawing.Size(82, 22);
            this.fToolStripMenuItem.Text = "F";
            // 
            // kToolStripMenuItem
            // 
            this.kToolStripMenuItem.Name = "kToolStripMenuItem";
            this.kToolStripMenuItem.Size = new System.Drawing.Size(82, 22);
            this.kToolStripMenuItem.Text = "K";
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
            this.mMDDYYYYToolStripMenuItem.Name = "mMDDYYYYToolStripMenuItem";
            this.mMDDYYYYToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.mMDDYYYYToolStripMenuItem.Text = "mm/dd/yyyy";
            // 
            // dDMMYYYYToolStripMenuItem
            // 
            this.dDMMYYYYToolStripMenuItem.Name = "dDMMYYYYToolStripMenuItem";
            this.dDMMYYYYToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.dDMMYYYYToolStripMenuItem.Text = "dd/mm/yyyy";
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
            // 
            // TwentyFourHourToolStripMenuItem
            // 
            this.TwentyFourHourToolStripMenuItem.Name = "TwentyFourHourToolStripMenuItem";
            this.TwentyFourHourToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.TwentyFourHourToolStripMenuItem.Text = "24-hour";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
            this.aboutToolStripMenuItem.Text = "About";
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
            this.imageComboLanguage.Location = new System.Drawing.Point(363, 0);
            this.imageComboLanguage.Name = "imageComboLanguage";
            this.imageComboLanguage.Size = new System.Drawing.Size(121, 29);
            this.imageComboLanguage.TabIndex = 2;
            this.imageComboLanguage.TabStop = false;
            // 
            // buttonArduinoSync
            // 
            this.buttonArduinoSync.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.buttonArduinoSync.BackgroundImage = global::LCA_SYNC.Properties.Resources.sync;
            this.buttonArduinoSync.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.buttonArduinoSync.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.buttonArduinoSync.Location = new System.Drawing.Point(424, 33);
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
            this.ClientSize = new System.Drawing.Size(484, 461);
            this.Controls.Add(this.imageComboLanguage);
            this.Controls.Add(this.buttonArduinoSync);
            this.Controls.Add(this.arduinoList);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.buttonConnectDisconnect);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.buttonSync);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Main";
            this.Text = "Low-Cost Array Sync";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tabControl.ResumeLayout(false);
            this.ConfigPage.ResumeLayout(false);
            this.ConfigPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTestDurationSeconds)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTestDurationMinutes)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTestDurationHours)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSampleRate)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
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
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RadioButton radioButtonTempC;
        private System.Windows.Forms.RadioButton radioButtonTempK;
        private System.Windows.Forms.RadioButton radioButtonTempF;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage DataPage;
        private System.Windows.Forms.Button buttonConnectDisconnect;
        private System.Windows.Forms.Button buttonArduinoSync;
        private System.Windows.Forms.ComboBox arduinoList;
        private System.Windows.Forms.BindingSource mainBindingSource;
        private System.Windows.Forms.BindingSource serialInterfaceBindingSource;
        private System.Windows.Forms.Label labelSampleRate;
        private System.Windows.Forms.NumericUpDown numericUpDownSampleRate;
        private System.Windows.Forms.NumericUpDown numericUpDownTestDurationSeconds;
        private System.Windows.Forms.NumericUpDown numericUpDownTestDurationMinutes;
        private System.Windows.Forms.NumericUpDown numericUpDownTestDurationHours;
        private System.Windows.Forms.Label labelTestDuration;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DateTimePicker dateTimePicker1;
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
    }
}

