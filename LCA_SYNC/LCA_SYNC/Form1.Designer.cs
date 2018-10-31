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
            this.buttonSync = new System.Windows.Forms.Button();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.ConfigPage = new System.Windows.Forms.TabPage();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.radioButtonTempC = new System.Windows.Forms.RadioButton();
            this.radioButtonTempK = new System.Windows.Forms.RadioButton();
            this.radioButtonTempF = new System.Windows.Forms.RadioButton();
            this.buttonConfigOpen = new System.Windows.Forms.Button();
            this.SensorsPage = new System.Windows.Forms.TabPage();
            this.buttonSensorsOpen = new System.Windows.Forms.Button();
            this.DataPage = new System.Windows.Forms.TabPage();
            this.richTextBoxData = new System.Windows.Forms.RichTextBox();
            this.buttonDataSave = new System.Windows.Forms.Button();
            this.buttonDataOpen = new System.Windows.Forms.Button();
            this.buttonConnectDisconnect = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.labelApplicationLabel = new System.Windows.Forms.Label();
            this.buttonBLETest = new System.Windows.Forms.Button();
            this.tabControl.SuspendLayout();
            this.ConfigPage.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SensorsPage.SuspendLayout();
            this.DataPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonSync
            // 
            this.buttonSync.Location = new System.Drawing.Point(198, 348);
            this.buttonSync.Name = "buttonSync";
            this.buttonSync.Size = new System.Drawing.Size(75, 23);
            this.buttonSync.TabIndex = 0;
            this.buttonSync.Text = "Sync";
            this.buttonSync.UseVisualStyleBackColor = true;
            this.buttonSync.Click += new System.EventHandler(this.button1_Click);
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.ConfigPage);
            this.tabControl.Controls.Add(this.SensorsPage);
            this.tabControl.Controls.Add(this.DataPage);
            this.tabControl.Location = new System.Drawing.Point(23, 35);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(435, 307);
            this.tabControl.TabIndex = 1;
            // 
            // ConfigPage
            // 
            this.ConfigPage.Controls.Add(this.label1);
            this.ConfigPage.Controls.Add(this.panel1);
            this.ConfigPage.Controls.Add(this.buttonConfigOpen);
            this.ConfigPage.Location = new System.Drawing.Point(4, 22);
            this.ConfigPage.Name = "ConfigPage";
            this.ConfigPage.Padding = new System.Windows.Forms.Padding(3);
            this.ConfigPage.Size = new System.Drawing.Size(427, 281);
            this.ConfigPage.TabIndex = 0;
            this.ConfigPage.Text = "Config";
            this.ConfigPage.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(94, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Temperature Units";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.radioButtonTempC);
            this.panel1.Controls.Add(this.radioButtonTempK);
            this.panel1.Controls.Add(this.radioButtonTempF);
            this.panel1.Location = new System.Drawing.Point(24, 40);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(113, 24);
            this.panel1.TabIndex = 9;
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
            this.radioButtonTempC.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
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
            // buttonConfigOpen
            // 
            this.buttonConfigOpen.Location = new System.Drawing.Point(291, 252);
            this.buttonConfigOpen.Name = "buttonConfigOpen";
            this.buttonConfigOpen.Size = new System.Drawing.Size(130, 23);
            this.buttonConfigOpen.TabIndex = 5;
            this.buttonConfigOpen.Text = "Open in Text Editor";
            this.buttonConfigOpen.UseVisualStyleBackColor = true;
            this.buttonConfigOpen.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // SensorsPage
            // 
            this.SensorsPage.Controls.Add(this.buttonSensorsOpen);
            this.SensorsPage.Location = new System.Drawing.Point(4, 22);
            this.SensorsPage.Name = "SensorsPage";
            this.SensorsPage.Padding = new System.Windows.Forms.Padding(3);
            this.SensorsPage.Size = new System.Drawing.Size(427, 281);
            this.SensorsPage.TabIndex = 1;
            this.SensorsPage.Text = "Sensors";
            this.SensorsPage.UseVisualStyleBackColor = true;
            // 
            // buttonSensorsOpen
            // 
            this.buttonSensorsOpen.Location = new System.Drawing.Point(291, 252);
            this.buttonSensorsOpen.Name = "buttonSensorsOpen";
            this.buttonSensorsOpen.Size = new System.Drawing.Size(130, 23);
            this.buttonSensorsOpen.TabIndex = 6;
            this.buttonSensorsOpen.Text = "Open in Text Editor";
            this.buttonSensorsOpen.UseVisualStyleBackColor = true;
            // 
            // DataPage
            // 
            this.DataPage.Controls.Add(this.richTextBoxData);
            this.DataPage.Controls.Add(this.buttonDataSave);
            this.DataPage.Controls.Add(this.buttonDataOpen);
            this.DataPage.Location = new System.Drawing.Point(4, 22);
            this.DataPage.Name = "DataPage";
            this.DataPage.Size = new System.Drawing.Size(427, 281);
            this.DataPage.TabIndex = 2;
            this.DataPage.Text = "Data";
            this.DataPage.UseVisualStyleBackColor = true;
            // 
            // richTextBoxData
            // 
            this.richTextBoxData.AcceptsTab = true;
            this.richTextBoxData.DetectUrls = false;
            this.richTextBoxData.Location = new System.Drawing.Point(4, 4);
            this.richTextBoxData.Name = "richTextBoxData";
            this.richTextBoxData.ReadOnly = true;
            this.richTextBoxData.Size = new System.Drawing.Size(420, 245);
            this.richTextBoxData.TabIndex = 8;
            this.richTextBoxData.Text = "Hello world";
            this.richTextBoxData.WordWrap = false;
            // 
            // buttonDataSave
            // 
            this.buttonDataSave.Location = new System.Drawing.Point(4, 255);
            this.buttonDataSave.Name = "buttonDataSave";
            this.buttonDataSave.Size = new System.Drawing.Size(130, 23);
            this.buttonDataSave.TabIndex = 7;
            this.buttonDataSave.Text = "Save Data As...";
            this.buttonDataSave.UseVisualStyleBackColor = true;
            // 
            // buttonDataOpen
            // 
            this.buttonDataOpen.Location = new System.Drawing.Point(294, 255);
            this.buttonDataOpen.Name = "buttonDataOpen";
            this.buttonDataOpen.Size = new System.Drawing.Size(130, 23);
            this.buttonDataOpen.TabIndex = 6;
            this.buttonDataOpen.Text = "Open in Text Editor";
            this.buttonDataOpen.UseVisualStyleBackColor = true;
            // 
            // buttonConnectDisconnect
            // 
            this.buttonConnectDisconnect.Location = new System.Drawing.Point(27, 348);
            this.buttonConnectDisconnect.Name = "buttonConnectDisconnect";
            this.buttonConnectDisconnect.Size = new System.Drawing.Size(124, 23);
            this.buttonConnectDisconnect.TabIndex = 2;
            this.buttonConnectDisconnect.Text = "Connect/Disconnect";
            this.buttonConnectDisconnect.UseVisualStyleBackColor = true;
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
            // buttonBLETest
            // 
            this.buttonBLETest.Location = new System.Drawing.Point(349, 348);
            this.buttonBLETest.Name = "buttonBLETest";
            this.buttonBLETest.Size = new System.Drawing.Size(75, 23);
            this.buttonBLETest.TabIndex = 5;
            this.buttonBLETest.Text = "BLE Test";
            this.buttonBLETest.UseVisualStyleBackColor = true;
            this.buttonBLETest.Click += new System.EventHandler(this.button1_Click_2);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 461);
            this.Controls.Add(this.buttonBLETest);
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
            this.SensorsPage.ResumeLayout(false);
            this.DataPage.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonSync;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage ConfigPage;
        private System.Windows.Forms.TabPage SensorsPage;
        private System.Windows.Forms.TabPage DataPage;
        private System.Windows.Forms.Button buttonConnectDisconnect;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button buttonConfigOpen;
        private System.Windows.Forms.Button buttonSensorsOpen;
        private System.Windows.Forms.Button buttonDataSave;
        private System.Windows.Forms.Button buttonDataOpen;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RadioButton radioButtonTempC;
        private System.Windows.Forms.RadioButton radioButtonTempK;
        private System.Windows.Forms.RadioButton radioButtonTempF;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RichTextBox richTextBoxData;
        private System.Windows.Forms.Label labelApplicationLabel;
        private System.Windows.Forms.Button buttonBLETest;
    }
}

