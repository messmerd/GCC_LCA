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
            this.buttonConfigOpen = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.radioButtonTempC = new System.Windows.Forms.RadioButton();
            this.radioButtonTempK = new System.Windows.Forms.RadioButton();
            this.radioButtonTempF = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.SensorsPage = new System.Windows.Forms.TabPage();
            this.DataPage = new System.Windows.Forms.TabPage();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.labelApplicationLabel = new System.Windows.Forms.Label();
            this.buttonConnectDisconnect = new System.Windows.Forms.Button();
            this.arduinoList = new System.Windows.Forms.ComboBox();
            this.buttonArduinoSync = new System.Windows.Forms.Button();
            this.mainBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.serialInterfaceBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.tabControl.SuspendLayout();
            this.ConfigPage.SuspendLayout();
            this.panel1.SuspendLayout();
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
            this.tabControl.TabIndex = 1;
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
            this.ConfigPage.Controls.Add(this.buttonConfigOpen);
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
            // buttonConfigOpen
            // 
            this.buttonConfigOpen.Location = new System.Drawing.Point(291, 252);
            this.buttonConfigOpen.Name = "buttonConfigOpen";
            this.buttonConfigOpen.Size = new System.Drawing.Size(130, 23);
            this.buttonConfigOpen.TabIndex = 13;
            this.buttonConfigOpen.Text = "Open in Text Editor";
            this.buttonConfigOpen.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.radioButtonTempC);
            this.panel1.Controls.Add(this.radioButtonTempK);
            this.panel1.Controls.Add(this.radioButtonTempF);
            this.panel1.Location = new System.Drawing.Point(33, 45);
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
            this.label1.Location = new System.Drawing.Point(30, 29);
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
            // labelApplicationLabel
            // 
            this.labelApplicationLabel.AutoSize = true;
            this.labelApplicationLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelApplicationLabel.Location = new System.Drawing.Point(88, 9);
            this.labelApplicationLabel.Name = "labelApplicationLabel";
            this.labelApplicationLabel.Size = new System.Drawing.Size(316, 20);
            this.labelApplicationLabel.TabIndex = 4;
            this.labelApplicationLabel.Text = "Low-Cost Array Synchronization Application";
            // 
            // buttonConnectDisconnect
            // 
            this.buttonConnectDisconnect.Location = new System.Drawing.Point(27, 358);
            this.buttonConnectDisconnect.Name = "buttonConnectDisconnect";
            this.buttonConnectDisconnect.Size = new System.Drawing.Size(124, 23);
            this.buttonConnectDisconnect.TabIndex = 2;
            this.buttonConnectDisconnect.Text = "Connect/Disconnect";
            this.buttonConnectDisconnect.UseVisualStyleBackColor = true;
            // 
            // arduinoList
            // 
            this.arduinoList.DataBindings.Add(new System.Windows.Forms.Binding("DropDownStyle", global::LCA_SYNC.Properties.Settings.Default, "DropDownList", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.arduinoList.DropDownStyle = global::LCA_SYNC.Properties.Settings.Default.DropDownList;
            this.arduinoList.FormattingEnabled = true;
            this.arduinoList.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.arduinoList.Location = new System.Drawing.Point(211, 42);
            this.arduinoList.Name = "arduinoList";
            this.arduinoList.Size = new System.Drawing.Size(210, 21);
            this.arduinoList.TabIndex = 0;
            this.arduinoList.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
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
            this.buttonArduinoSync.UseVisualStyleBackColor = false;
            // 
            // mainBindingSource
            // 
            this.mainBindingSource.DataSource = typeof(LCA_SYNC.Main);
            // 
            // serialInterfaceBindingSource
            // 
            this.serialInterfaceBindingSource.DataSource = typeof(Arduino_Serial_Interface.SerialInterface);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 461);
            this.Controls.Add(this.buttonArduinoSync);
            this.Controls.Add(this.arduinoList);
            this.Controls.Add(this.labelApplicationLabel);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.buttonConnectDisconnect);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.buttonSync);
            this.Name = "Main";
            this.Text = "Low-Cost Array Sync";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tabControl.ResumeLayout(false);
            this.ConfigPage.ResumeLayout(false);
            this.ConfigPage.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
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
        private System.Windows.Forms.Label labelApplicationLabel;
        private System.Windows.Forms.Button buttonConfigOpen;
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
    }
}

