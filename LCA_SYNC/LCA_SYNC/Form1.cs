using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace LCA_SYNC
{

    public partial class Main : Form
    {
        SerialInterface serial;
        object[] deviceList;
        private BindingSource arduinoListBinding;


        // Language variables: 
        private Dictionary<string, string> LanguageText;
        //private Dictionary<string, Image> LanguageIcons;
        private ImageList LanguageIcons;
        private SortedSet<string> AvailableLanguages;
        private string CurrentLanguage = "";
        private readonly string DefaultLanguage = "en";             // The hard-coded language of the program 
        private readonly string DefaultLanguageLong = "English";    // The hard-coded language of the program 
        private string UserDefaultLanguage = "en";                  // The language the user chose to use as a default 


        public Main()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(Main_UnhandledException);

            serial = SerialInterface.Create();

            FormClosing += Main_FormClosing;  // Trying w/o this. B/c PnP watcher stopped working again for some reason

            InitializeComponent();

            // Unfocus controls as necessary when the user clicks any of these controls:
            this.MouseDown += UnfocusControls;
            tabControl.MouseDown += UnfocusControls;
            menuStrip1.MouseDown += UnfocusControls;
            StatusPage.MouseDown += UnfocusControls;
            ConfigPage.MouseDown += UnfocusControls;
            SensorsPage.MouseDown += UnfocusControls;
            DataPage.MouseDown += UnfocusControls;
            panelStartDelay.MouseDown += UnfocusControls;
            labelConfigSec2.MouseDown += UnfocusControls;
            labelConfigSec.MouseDown += UnfocusControls;
            labelConfigMin.MouseDown += UnfocusControls;
            labelConfigHr.MouseDown += UnfocusControls;
            labelPackageName.MouseDown += UnfocusControls;
            labelSamplePeriod.MouseDown += UnfocusControls;
            labelTestDuration.MouseDown += UnfocusControls;
            labelStartDelay.MouseDown += UnfocusControls;

            // Config Page setup: 
            checkBoxSyncTimeDate.Tag = false;                   // Unchecked by default 
            textBoxPackageName.TextChanged += TextBoxPackageName_TextChanged;
            numericUpDownSamplePeriod.ValueChanged += NumericUpDownSampleRate_ValueChanged;
            numericUpDownTestDurationHours.ValueChanged += NumericUpDownTestDuration_ValueChanged;
            numericUpDownTestDurationMinutes.ValueChanged += NumericUpDownTestDuration_ValueChanged;
            numericUpDownTestDurationSeconds.ValueChanged += NumericUpDownTestDuration_ValueChanged;
            radioButtonStartDelayNone.CheckedChanged += RadioButtonStartDelayOption_CheckedChanged;
            radioButtonStartDelayOneMin.CheckedChanged += RadioButtonStartDelayOption_CheckedChanged;
            radioButtonStartDelayThreeMin.CheckedChanged += RadioButtonStartDelayOption_CheckedChanged;

            // Languages setup: 
            imageComboLanguage.DropDownClosed += imageComboLanguage_DropDownClosed;  // To unhighlight the selection 
            imageComboLanguage.KeyDown += imageComboLanguage_KeyDown;
            imageComboLanguage.SelectedIndexChanged += imageComboLanguage_SelectedIndexChanged;
            LanguageText = new Dictionary<string, string>();
            AvailableLanguages = new SortedSet<string>();
            LanguageIcons = new ImageList();
            LanguageIcons.ImageSize = new Size(imageComboLanguage.ImageList.ImageSize.Height, imageComboLanguage.ImageList.ImageSize.Height);
            CurrentLanguage = "";
            LoadLanguages();

            serial.ArduinoChanged += Serial_ArduinoChanged;
            serial.ArduinoDataChanged += serial_ArduinoDataChanged;
            Console.WriteLine("\n\n\n");

            // Arduino List setup: 
            deviceList = new object[] { "<No Device>" };
            //arduinoList.Items.AddRange(deviceList);
            arduinoListBinding = new BindingSource();
            arduinoListBinding.DataSource = serial.LCAArduinos;
            arduinoListBinding.ListChanged += serial_LCAArduinos_Changed;
            arduinoList.DataSource = arduinoListBinding;
            arduinoList.DisplayMember = "displayName";
            //arduinoList.ValueMember = "Self";   // What is the default value of this? Is it self? 
            //arduinoList.SelectedIndex = 0;

            // Start watching for USB PnP devices to be added/removed/modified:
            serial.StartPnPWatcher();

            // Find LCA arduinos: 
            serial.ActivateAllArduinos();
            RefreshControlsEnable();

            textBoxPackageName.Enabled = true;

        }

        #region Controls Event Handlers

        private void RadioButtonStartDelayOption_CheckedChanged(object sender, EventArgs e)
        {
            UpdateTabText();
        }

        private void NumericUpDownTestDuration_ValueChanged(object sender, EventArgs e)
        {

            if (numericUpDownTestDurationHours.Value != Math.Truncate(numericUpDownTestDurationHours.Value))
            {
                numericUpDownTestDurationHours.Value = Math.Round(numericUpDownTestDurationHours.Value, 0, MidpointRounding.AwayFromZero);
            }
            if (numericUpDownTestDurationMinutes.Value != Math.Truncate(numericUpDownTestDurationMinutes.Value))
            {
                numericUpDownTestDurationMinutes.Value = Math.Round(numericUpDownTestDurationMinutes.Value, 0, MidpointRounding.AwayFromZero);
            }
            if (numericUpDownTestDurationSeconds.Value != Math.Truncate(numericUpDownTestDurationSeconds.Value))
            {
                numericUpDownTestDurationSeconds.Value = Math.Round(numericUpDownTestDurationSeconds.Value, 0, MidpointRounding.AwayFromZero);
            }

            UpdateTabText();

        }

        private void TextBoxPackageName_TextChanged(object sender, EventArgs e)
        {

            bool validInput = true;
            foreach (char c in textBoxPackageName.Text)
            {
                if (c > 255 || c < 32)  // If a character is not a character supported by the arduino 
                {
                    validInput = false;
                }
            }

            if (validInput)
            {
                // Note: Other methods (like UpdateTabText) check this color to know if the input is valid
                textBoxPackageName.BackColor = Color.White;
            }
            else
            {
                textBoxPackageName.BackColor = Color.Red;
            }

            UpdateTabText();
        }

        private void NumericUpDownSampleRate_ValueChanged(object sender, EventArgs e)
        {
            if (Math.Floor(numericUpDownSamplePeriod.Value * 8) != numericUpDownSamplePeriod.Value * 8) // If the sample rate is not a multiple of 0.125 
            {
                //Console.WriteLine("numericUpDownSampleRate: Not a multiple of 0.125");
                numericUpDownSamplePeriod.Value = Math.Round(numericUpDownSamplePeriod.Value * 8) / 8; // Set it to the nearest multiple of 0.125 
            }
            else  // Valid input
            {
                //Console.WriteLine("numericUpDownSampleRate: Just right");
            }

            UpdateTabText();
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Maybe this will fix the problem...
            if (serial != null && serial.pnpWatcher != null)
            {
                serial.pnpWatcher.EventArrived -= serial.pnpWatcherHandler; // According to Stack Overflow, unsubscribing twice won't cause an error 
                serial.pnpWatcher.Stop();
                serial.pnpWatcher.Dispose();
            }
        }

        private void Main_UnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            // Another attempt at stopping the pnpWatcher from continuing to run after the program ends.
            Exception e = (Exception)args.ExceptionObject;
            Console.WriteLine("MyHandler caught : " + e.Message);
            Console.WriteLine("Runtime terminating: {0}", args.IsTerminating);

            // Maybe this will fix the problem...
            if (serial != null && serial.pnpWatcher != null)
            {
                serial.pnpWatcher.EventArrived -= serial.pnpWatcherHandler; // According to Stack Overflow, unsubscribing twice won't cause an error 

                serial.pnpWatcher.Stop();
                serial.pnpWatcher.Dispose();
            }
        }

        private void imageComboLanguage_DropDownClosed(object sender, EventArgs e)
        {
            // To unhighlight the selection 
            this.ActiveControl = null;

            //imageComboLanguage.Refresh();
            //imageComboLanguage.Invalidate();
            //imageComboLanguage.Update();

        }

        private void imageComboLanguage_KeyDown(object sender, KeyEventArgs e)
        {
            // Prevents the arrowkeys from changing the selection except when it is dropped down
            if (!((ImageCombo)sender).DroppedDown && (e.KeyCode == Keys.Right || e.KeyCode == Keys.Left || e.KeyCode == Keys.Up || e.KeyCode == Keys.Down))
            {
                //TxtPass.Focus();
                this.ActiveControl = null;
                e.Handled = true;
                return;
            }
        }

        private void imageComboLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            CurrentLanguage = (string)((ImageComboItem)imageComboLanguage.SelectedItem).Tag;
            Console.WriteLine("Switched to the language: {0}.", LanguageText?[CurrentLanguage + "LangLong"]);
            RefreshLanguage();
        }

        // Click 'Sync'
        private async void buttonSync_Click(object sender, EventArgs e)
        {
            if (serial.Arduino == null)
            {
                return;  // Just to be safe 
            }

            // This method currently doesn't check to see if it syncs successfully... Might be a problem
            UpdateTabText();
            try
            {
                if (tabControl.TabPages[1].Text.Last() == '*')  // If config page needs synced 
                {
                    this.UseWaitCursor = true; // This isn't working for some reason 
                    buttonSync.Enabled = false;
                    if ((decimal?)serial.Arduino.SamplePeriod != numericUpDownSamplePeriod.Value)
                        await serial.Arduino.Communicate(DATACATEGORY.CONFIG, SUBCATEGORY.SAMPLE_PERIOD, ACTION.WRITEVAR, (float)numericUpDownSamplePeriod.Value);
                    if (Math.Floor((decimal)serial.Arduino.TestDuration / 60 / 60) != numericUpDownTestDurationHours.Value || Math.Floor((decimal)serial.Arduino.TestDuration / 60) % 60 != numericUpDownTestDurationMinutes.Value || (decimal)serial.Arduino.TestDuration % 60 != numericUpDownTestDurationSeconds.Value)
                        await serial.Arduino.Communicate(DATACATEGORY.CONFIG, SUBCATEGORY.TEST_DUR, ACTION.WRITEVAR, (uint)numericUpDownTestDurationHours.Value * 60 * 60 + (uint)numericUpDownTestDurationMinutes.Value * 60 + (uint)numericUpDownTestDurationSeconds.Value);
                    if (serial.Arduino.PackageName != textBoxPackageName.Text && textBoxPackageName.BackColor == Color.White)
                        await serial.Arduino.Communicate(DATACATEGORY.CONFIG, SUBCATEGORY.PACKAGE_NAME, ACTION.WRITEVAR, textBoxPackageName.Text);
                    if ((serial.Arduino.StartDelay == 0) != radioButtonStartDelayNone.Checked && radioButtonStartDelayNone.Checked)
                        await serial.Arduino.Communicate(DATACATEGORY.CONFIG, SUBCATEGORY.START_DELAY, ACTION.WRITEVAR, 0);
                    if ((serial.Arduino.StartDelay == 60) != radioButtonStartDelayOneMin.Checked && radioButtonStartDelayOneMin.Checked)
                        await serial.Arduino.Communicate(DATACATEGORY.CONFIG, SUBCATEGORY.START_DELAY, ACTION.WRITEVAR, 60);
                    if ((serial.Arduino.StartDelay == 60*3) != radioButtonStartDelayThreeMin.Checked && radioButtonStartDelayThreeMin.Checked)
                        await serial.Arduino.Communicate(DATACATEGORY.CONFIG, SUBCATEGORY.START_DELAY, ACTION.WRITEVAR, 180);
                    if (checkBoxSyncTimeDate.Checked == true)
                    {
                        // This isn't implemented yet in ArduinoBoard.cs or in the arduino code
                        if (radioButtonUseSysTime.Checked) // System time
                        {
                            await serial.Arduino.Communicate(DATACATEGORY.OTHER, SUBCATEGORY.TIME_DATE, ACTION.WRITEVAR, DateTime.Now, 600);
                            // If an exception is thrown in the method above, it shouldn't execute the line below 
                            checkBoxSyncTimeDate.Checked = false; // Does this verify that the time was set correctly first? 
                        }
                        else  // Custom time 
                        {
                            MessageBox.Show("Sorry, custom time and date is currently unsupported. ");
                            //Response r = await serial.Arduino.Communicate(DATACATEGORY.OTHER, SUBCATEGORY.TIME_DATE, ACTION.WRITEVAR, dateTimePickerCustomTime.Value);
                            //if (r.validity == ArduinoBoard.COMMERROR.VALID)
                            //  checkBoxSyncTimeDate.Checked = false;
                        }
                    }

                    // Start delay here 
                }

                if (tabControl.TabPages[2].Text.Last() == '*')  // If data page needs synced 
                {
                    this.UseWaitCursor = true;
                    buttonSync.Enabled = false;
                    // Implement later 
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error when syncing. Error message: {0}\nStack Trace: {1}", ex.Message, ex.StackTrace);
            }
            finally
            {
                this.UseWaitCursor = false;
                UpdateTabText();
            }


        }

        private async void arduinoList_SelectedIndexChanged(object sender, EventArgs e)
        {
            // For switching between arduinos. Not tested yet!!

            if (arduinoList.Items.Count > 0 && serial.Arduino != (ArduinoBoard)arduinoList.SelectedItem)
            {
                bool success = false;
                try
                {
                    await ((ArduinoBoard)arduinoList.SelectedItem).RefreshInfo();
                    success = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("RefreshInfo() failed for the newly-selected arduino: {0}", ex.Message);
                }

                if (success)
                {
                    serial.Arduino = (ArduinoBoard)arduinoList.SelectedItem;
                }

            }
        }

        private async void buttonArduinoSync_Click(object sender, EventArgs e)
        {
            // Untested 
            // RefreshInfo for all arduinos already added: 
            foreach (var ard in serial.LCAArduinos)
            {
                await ard.RefreshInfo();
            }

            await serial.ActivateAllArduinos();  // Activates (Ping + RefreshInfo) all arduinos that have not been added yet
            RefreshControlsEnable();
        }

        private async void buttonStatusStartStop_Click(object sender, EventArgs e)
        {

            bool success = false;

            if (serial.Arduino == null || !serial.Arduino.TestStarted)  // Test is currently not running 
            {
                try
                {
                    buttonStatusStartStop.Enabled = false;
                    await serial.Arduino.Communicate(DATACATEGORY.OTHER, SUBCATEGORY.START_TEST, ACTION.SENDCOMMAND);
                    buttonStatusStartStop.Enabled = true;
                    success = true;
                }
                catch
                {
                    success = false;
                }
                finally
                {
                    if (success)
                    {
                        buttonStatusStartStop.Text = getLanguageText("StopTest", "Stop Test");
                    }
                    buttonStatusStartStop.Enabled = true;
                }
            }
            else  // Test is not running or no arduino is connected and selected 
            {
                try
                {
                    buttonStatusStartStop.Enabled = false;
                    await serial.Arduino.Communicate(DATACATEGORY.OTHER, SUBCATEGORY.STOP_TEST, ACTION.WRITEVAR);
                    buttonStatusStartStop.Enabled = true;
                    success = true;
                }
                catch
                {
                    success = false;
                }
                finally
                {
                    if (success)
                    {
                        buttonStatusStartStop.Text = getLanguageText("StartTest", "Start Test");
                    }
                    buttonStatusStartStop.Enabled = true;
                }
            }

        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(getLanguageText("AboutPageText", "Created at Grove City College for 2018-2019 Senior Capstone Project. "));
        }

        private void TempUnitsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (((ToolStripMenuItem)sender).Checked == false)  // If it was not checked before it was clicked 
            {
                ((ToolStripMenuItem)sender).Checked = true;    // Check it 

                // Uncheck other menu items 
                if (cToolStripMenuItem != (ToolStripMenuItem)sender)
                {
                    cToolStripMenuItem.Checked = false;
                }
                if (fToolStripMenuItem != (ToolStripMenuItem)sender)
                {
                    fToolStripMenuItem.Checked = false;
                }
                if (kToolStripMenuItem != (ToolStripMenuItem)sender)
                {
                    kToolStripMenuItem.Checked = false;
                }

                // Update any controls relying on the temperature unit setting here 
            }

        }

        private void DateFormatOptionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (((ToolStripMenuItem)sender).Checked == false)  // If it was not checked before it was clicked 
            {
                ((ToolStripMenuItem)sender).Checked = true;    // Check it 

                // Uncheck other menu items 
                if (mMDDYYYYToolStripMenuItem != (ToolStripMenuItem)sender)
                {
                    mMDDYYYYToolStripMenuItem.Checked = false;
                }
                if (dDMMYYYYToolStripMenuItem != (ToolStripMenuItem)sender)
                {
                    dDMMYYYYToolStripMenuItem.Checked = false;
                }

                // Update any controls relying on the date format setting here 
            }
        }

        private void TimeFormatOptionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (((ToolStripMenuItem)sender).Checked == false)  // If it was not checked before it was clicked 
            {
                ((ToolStripMenuItem)sender).Checked = true;    // Check it 

                // Uncheck other menu items 
                if (TwelveHourToolStripMenuItem != (ToolStripMenuItem)sender)
                {
                    TwelveHourToolStripMenuItem.Checked = false;
                }
                if (TwentyFourHourToolStripMenuItem != (ToolStripMenuItem)sender)
                {
                    TwentyFourHourToolStripMenuItem.Checked = false;
                }

                // Update any controls relying on the time format setting here 
            }
        }

        private void checkBoxSyncTimeDate_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxSyncTimeDate.Checked == false)
            {
                radioButtonUseSysTime.Enabled = false;
                radioButtonUseCustomTime.Enabled = false;
                dateTimePickerCustomTime.Enabled = false;
            }
            else
            {
                radioButtonUseSysTime.Enabled = true;
                radioButtonUseCustomTime.Enabled = true;
                dateTimePickerCustomTime.Enabled = true;
            }
            UpdateTabText();
        }

        private void buttonConfigDiscardChanges_Click(object sender, EventArgs e)
        {
            numericUpDownSamplePeriod.Value = (decimal)(serial.Arduino?.SamplePeriod ?? 1);

            numericUpDownTestDurationHours.Value = (serial.Arduino != null) ? (Math.Floor((decimal)serial.Arduino.TestDuration / 60 / 60)) : 0;
            numericUpDownTestDurationMinutes.Value = (serial.Arduino != null) ? (Math.Floor((decimal)serial.Arduino.TestDuration / 60) % 60) : 0;
            numericUpDownTestDurationSeconds.Value = (serial.Arduino != null) ? ((decimal)serial.Arduino.TestDuration % 60) : 30;

            textBoxPackageName.Text = serial.Arduino?.PackageName ?? "ERROR";

            radioButtonStartDelayNone.Checked = (serial.Arduino != null) ? (serial.Arduino.StartDelay == 0) : true;
            radioButtonStartDelayOneMin.Checked = (serial.Arduino != null) ? (serial.Arduino.StartDelay == 60) : false;
            radioButtonStartDelayThreeMin.Checked = (serial.Arduino != null) ? (serial.Arduino.StartDelay == 60*3) : false;

            checkBoxSyncTimeDate.Checked = (bool)checkBoxSyncTimeDate.Tag;

            UpdateTabText();
        }

        #endregion

        #region Other Event Handlers 

        private void Serial_ArduinoChanged(object sender, ArduinoEventArgs e)
        {
            // The ArduinoChanged event that this handler is subscribed to is triggered when SerialInterface.Arduino is changed. 
            // SerialInterface.Arduino is the currently active ArduinoBoard object - the one that this synchronization program is connected to and interacting with. 

            Console.WriteLine("SerialInterface.Arduino changed.");

            RefreshControlsEnable();  // Refresh all GUI controls that require a sensor package to be connected

            if (serial.Arduino == null)
            {
                // If no arduino is connected and selected, set Status page labels text to their default values
                labelStatusPackageName.Text = getLanguageText("PackageName", "Package Name");
                labelStatusSerialPort.Text = getLanguageText("SerialPort", "Serial Port");
                labelStatusTotalSensors.Text = getLanguageText("TotalSensors", "Total Sensors") + ":";
                labelStatusMode.Text = getLanguageText("Mode", "Mode") + ":";

                labelStatusElapsedTime.Text = getLanguageText("ElapsedTime", "Elapsed Time") + ":";
                labelStatusDataFile.Text = getLanguageText("CurrentDataFile", "Current Data File") + ":";

                buttonStatusStartStop.Text = getLanguageText("StartTest", "Start Test");

                buttonConfigDiscardChanges_Click(this, new EventArgs()); // No ardunio connected, so discard user's config page changes  
            }
            else
            {
                // The serial port name only changes when Arduino changes, so I'll update the serial port text label here instead 
                //      of the serial arduino data changed event handler.  
                labelStatusSerialPort.Text = "(" + (serial.Arduino.Port?.PortName ?? "ERROR") + ")";
            }
        }

        private void UnfocusControls(object sender, MouseEventArgs e)
        {
            // Removes focus for controls if you click away 

            //Console.WriteLine("In UnfocusControls. Still active: {0}, Clicked on: {1}.", ActiveControl?.Name, ((Control)sender)?.Name ); // sender?.GetType()?.Name);

            if (ActiveControl?.Name != ((Control)sender)?.Name)
            {
                this.ActiveControl = null;
            }

        }

        private void serial_ArduinoDataChanged(object sender, ArduinoEventArgs e)
        {
            // Can do more here: 
            // Use e.Reason for the reason the event was called 

            switch ((string)e.Type)
            {
                case "PackageName":
                    Console.WriteLine("PackageName changed, so status page will be updated");
                    textBoxPackageName.Text = serial.Arduino.PackageName;
                    labelStatusPackageName.Text = serial.Arduino.PackageName;
                    break;
                case "StartDelay":
                    if (serial.Arduino.StartDelay == 0)  // No start delay 
                    {
                        radioButtonStartDelayNone.Checked = true;
                        radioButtonStartDelayOneMin.Checked = false;
                        radioButtonStartDelayThreeMin.Checked = false;
                    }
                    else if (serial.Arduino.StartDelay == 60)  // One minute 
                    {
                        radioButtonStartDelayNone.Checked = false;
                        radioButtonStartDelayOneMin.Checked = true;
                        radioButtonStartDelayThreeMin.Checked = false;
                    }
                    else if (serial.Arduino.StartDelay == 60 * 3) // Three minutes
                    {
                        radioButtonStartDelayNone.Checked = false;
                        radioButtonStartDelayOneMin.Checked = false;
                        radioButtonStartDelayThreeMin.Checked = true;
                    }
                    else
                    {
                        Console.WriteLine("Unacceptable value for StartDelay.");
                    }
                    break;
                case "TestDuration":
                    numericUpDownTestDurationHours.Value = Math.Floor((decimal)serial.Arduino.TestDuration / 60 / 60); // Get hours from total seconds 
                    numericUpDownTestDurationMinutes.Value = Math.Floor((decimal)serial.Arduino.TestDuration / 60) % 60; // Get minutes from total seconds 
                    numericUpDownTestDurationSeconds.Value = (decimal)serial.Arduino.TestDuration % 60; // Get seconds from total seconds 

                    //Console.WriteLine("{0}:{1}:{2}", Math.Floor((decimal)serial.Arduino.TestDuration / 60 / 60), Math.Floor((decimal)serial.Arduino.TestDuration / 60) % 60, (decimal)serial.Arduino.TestDuration % 60);
                    break;
                case "SamplePeriod":
                    numericUpDownSamplePeriod.Value = (decimal)serial.Arduino.SamplePeriod;
                    break;
                case "TestStarted":
                    Console.WriteLine("TestStarted changed, so status page will be updated");
                    ButtonStatusStartStopRefresh();
                    if (serial.Arduino.TestStarted)
                    {
                        labelStatusMode.Text = getLanguageText("Mode", "Mode") + ": " + getLanguageText("Running", "Running");
                    }
                    else
                    {
                        labelStatusMode.Text = getLanguageText("Mode", "Mode") + ": " + getLanguageText("Ready", "Ready");
                    }

                    break;
                default:
                    Console.WriteLine("In serial_ArduinoDataChanged, an unrecognized arduino variable was changed: {0}", (string)e.Type);
                    break;
            }


            //// I'm putting these here since they don't have their own member variables in ArduinoBoard yet:
            labelStatusTotalSensors.Text = getLanguageText("TotalSensors", "Total Sensors") + ": __"; // A placeholder
            labelStatusElapsedTime.Text = getLanguageText("ElapsedTime", "Elapsed Time") + ": __/__";  // A placeholder 
            labelStatusDataFile.Text = getLanguageText("CurrentDataFile", "Current Data File") + ": __";  // A placeholder
            ///////

            arduinoListBinding.ResetBindings(false);

            //RefreshStatusPageText();  // Refresh the text on the status page 
            UpdateTabText();
        }

        /// <summary>
        /// LCAArduinos_Changed event is caught in
        /// order to update anything when LCAArduinos
        /// list is changed or values within it are 
        /// changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void serial_LCAArduinos_Changed(object sender, ListChangedEventArgs e)  // Maybe use arduinoList.DataSourceChanged event instead? 
        {

            // I think in general, serial_ArduinoDataChanged or serial_ArduinoChanged should be used instead. 
            // I haven't been consistent with which event gets triggered first: serial_ArduinoChanged or serial_LCAArduinos_Changed.

            Console.WriteLine("LCAArduinos has been changed. ");
        }

        #endregion

        #region Other Methods

        private void RefreshControlsEnable()
        {
            // This function refreshes all GUI controls that require a sensor package to be connected

            //buttonConfigDiscardChanges_Click(this, new EventArgs());  // Clears changes made in the config page

            bool enabled = serial.Arduino != null;
            textBoxPackageName.Enabled = enabled;
            numericUpDownSamplePeriod.Enabled = enabled;
            numericUpDownTestDurationHours.Enabled = enabled;
            numericUpDownTestDurationMinutes.Enabled = enabled;
            numericUpDownTestDurationSeconds.Enabled = enabled;
            radioButtonStartDelayNone.Enabled = enabled;
            radioButtonStartDelayOneMin.Enabled = enabled;
            radioButtonStartDelayThreeMin.Enabled = enabled;
            checkBoxSyncTimeDate.Enabled = enabled;
            radioButtonUseSysTime.Enabled = enabled;
            radioButtonUseCustomTime.Enabled = enabled;
            buttonStatusStartStop.Enabled = enabled;
            //buttonSync.Enabled = enabled;

            ButtonStatusStartStopRefresh();  // Sets the Start/Stop Test button to what it should be 
            UpdateTabText();

        }

        private void UpdateTabText()
        {
            // Config Tab: 
            if (serial.Arduino != null &&
                ((decimal)serial.Arduino.SamplePeriod != numericUpDownSamplePeriod.Value ||
                Math.Floor((decimal)serial.Arduino.TestDuration / 60 / 60) != numericUpDownTestDurationHours.Value ||
                Math.Floor((decimal)serial.Arduino.TestDuration / 60) % 60 != numericUpDownTestDurationMinutes.Value ||
                serial.Arduino.TestDuration % 60 != numericUpDownTestDurationSeconds.Value ||
                serial.Arduino.PackageName != textBoxPackageName.Text ||
                (serial.Arduino.StartDelay == 0) != radioButtonStartDelayNone.Checked ||
                (serial.Arduino.StartDelay == 60) != radioButtonStartDelayOneMin.Checked ||
                (serial.Arduino.StartDelay == 60*3) != radioButtonStartDelayThreeMin.Checked ||
                checkBoxSyncTimeDate.Checked == true))
            {
                // Difference detected
                if (tabControl.TabPages[1].Text.Last() != '*')
                {
                    tabControl.TabPages[1].Text += '*';
                    buttonConfigDiscardChanges.Enabled = true;
                }
            }
            else
            {
                //Console.WriteLine("No difference.");
                tabControl.TabPages[1].Text = tabControl.TabPages[1].Text.TrimEnd('*');
                buttonConfigDiscardChanges.Enabled = false;
            }

            // If changes have been made and those changes are valid and can be applied: 
            if ((tabControl.TabPages[1].Text.Last() == '*' || tabControl.TabPages[2].Text.Last() == '*') &&
                textBoxPackageName.BackColor == Color.White && serial.Arduino != null)
            {
                buttonSync.Enabled = true;
            }
            else
            {
                buttonSync.Enabled = false;
            }

        }

        private void LoadLanguages()
        {
            // The one weird thing about this whole language thing is that if the en.dat file is missing, you cannot have English
            //     unless you delete the other language files or the lang folder, and then all you can have is English. 
            //     Maybe to be less weird, a copy of the English language files can be stored in the resources and if the lang 
            //     directory or English language files don't exist, then it can create them before loading the languages. 
            //     But what if the files fail to save for some reason and there are other languages that exist? Then the weird 
            //     situation would still exist. So I would have to add the English language in a special way that does not involve 
            //     files. It shouldn't be too hard, and it would be the best course of action probably. I'd have to get the 
            //     alphabetical ordering right which would be the hardest part. I'd just use defaultLangIcon in the resources and 
            //     DefaultLanguage and DefaultLanguageLong and RefreshLanguages would handle the rest. So if both the files for the 
            //     UserDefaultLanguage and DefaultLanguage do not exist, there will still be English as an option regardless. 
            //     No disabling of the imageComboLanguage would be needed ever since there will always be at least one option. 

            string langShort;
            string[] keyvalue = new string[2]; // keyvalue[0] = the key; keyvalue[1] = the value 
            ImageComboItem comboItem;

            if (!Directory.Exists(Application.StartupPath + @"\lang\"))  // If lang directory is missing, disable language selection
            {
                Console.WriteLine("Error: The directory lang\\ does not exist.");
                LanguageIcons.Images.Add("error", Properties.Resources.errorLang);
                imageComboLanguage.ImageList = LanguageIcons;
                LanguageText["errorLang"] = "error";
                LanguageText["errorLangLong"] = "Lang. error";
                comboItem = new ImageComboItem("Lang. error", 0);
                comboItem.Tag = "error";
                imageComboLanguage.Items.Add(comboItem);
                imageComboLanguage.SelectedIndex = 0;
                imageComboLanguage.Enabled = false;
                AvailableLanguages.Clear();
                CurrentLanguage = "error";
                return;
            }

            string[] filePaths = Directory.GetFiles(Application.StartupPath + @"\lang\", "*.dat").OrderBy(s => s).ToArray();
            //Array.Sort(filePaths, (x, y) => String.Compare(x, y));

            Console.WriteLine("Number of language files found: {0}", filePaths.Length);

            if (filePaths.Length == 0)  // If no language files exist, disable language selection
            {
                Console.WriteLine("Error: Could not locate any language files.");
                LanguageIcons.Images.Add("error", Properties.Resources.errorLang);
                imageComboLanguage.ImageList = LanguageIcons;
                LanguageText["errorLang"] = "error";
                LanguageText["errorLangLong"] = "Lang. error";
                comboItem = new ImageComboItem("Lang. error", 0);
                comboItem.Tag = "error";
                imageComboLanguage.Items.Add(comboItem);
                imageComboLanguage.SelectedIndex = 0;
                imageComboLanguage.Enabled = false;
                AvailableLanguages.Clear();
                CurrentLanguage = "error";
                return;
            }

            int i = 0;
            foreach (var file in filePaths)
            {
                langShort = file.Split('\\').Last().Split('.').First();

                // Load language text
                foreach (string line in File.ReadAllLines(file, Encoding.Unicode))
                {
                    line.Split('=').CopyTo(keyvalue, 0);

                    if (keyvalue[0] == "") { continue; }
                    if (keyvalue.Length != 2) { Console.WriteLine("Error: A line in the file {0} does not have only one '=' character.", file); return; }

                    if (!LanguageText.ContainsKey(langShort + keyvalue[0]))
                    {
                        LanguageText.Add(langShort + keyvalue[0], keyvalue[1]);
                    }
                }

                // Load language icon 
                if (!LanguageIcons.Images.ContainsKey(langShort))
                {
                    if (LanguageText.ContainsKey(langShort + "Icon"))
                    {
                        if (File.Exists(Application.StartupPath + @"\lang\" + LanguageText[langShort + "Icon"]))
                        {
                            LanguageIcons.Images.Add(langShort, Image.FromFile(Application.StartupPath + @"\lang\" + LanguageText[langShort + "Icon"]));
                            //Console.WriteLine("Added language image file.");
                        }
                        else  // There was an error loading the language icon 
                        {
                            LanguageIcons.Images.Add(langShort, Properties.Resources.errorLang);
                            Console.WriteLine("Error loading language icon for the language: {0}.", langShort);
                        }
                    }
                    else  // No language icon was specified 
                    {
                        LanguageIcons.Images.Add(langShort, Properties.Resources.noLangIcon);
                    }

                    imageComboLanguage.ImageList = LanguageIcons;
                    // Check if it's already in the list first? 
                    comboItem = new ImageComboItem(LanguageText[langShort + "LangLong"], i);
                    comboItem.Tag = langShort;
                    imageComboLanguage.Items.Add(comboItem);
                }

                AvailableLanguages.Add(langShort);
                i++;
            }

            if (CurrentLanguage == "")  // If the CurrentLanguage has not been set, then set it 
            {
                if (AvailableLanguages.Contains(UserDefaultLanguage))  // 1st priority is the user's default language if it exists in the lang folder
                {
                    CurrentLanguage = UserDefaultLanguage;
                }
                else if (AvailableLanguages.Contains(DefaultLanguage)) // 2nd priority is the program's default language if it exists in the lang folder
                {
                    CurrentLanguage = DefaultLanguage;
                }
                else if (AvailableLanguages.Count > 0) // 3rd priority is whatever other language exists in the lang folder 
                {
                    CurrentLanguage = AvailableLanguages.First();
                }
                else // Last is the error state where no languages exist. 
                {
                    // This case should be caught previously by previous error checks and should never occur. 
                    return;
                }

                // Select the current language in the ImageCombo drop-down list: 
                foreach (ImageComboItem item in imageComboLanguage.Items)
                {
                    if (item.ItemText == LanguageText[CurrentLanguage + "LangLong"])
                    {
                        imageComboLanguage.SelectedIndex = item.ImageIndex;
                    }
                }

            }

            RefreshLanguage();
            imageComboLanguage.Refresh();
        }

        private string getLanguageText(string key, string defaultValue, string lang = null)
        {
            lang = lang ?? CurrentLanguage;
            if (LanguageText.ContainsKey(lang + key))
            {
                return LanguageText[lang + key];
            }
            else
            {
                return defaultValue;
            }
        }

        private void RefreshLanguage()
        {
            // This method refreshes the text in GUI controls to match the CurrentLanguage. 
            // All other language changes are made through calling LanguageText on the fly by the controls that need it. 
            // English is the default language and can be loaded even if the English language file does not exist. 

            // Menu Items //////////////////// 
            // The English "File" is default if the language cannot be loaded:  
            menuStrip1.Items[0].Text = getLanguageText("File", "File");
            menuStrip1.Items[1].Text = getLanguageText("Options", "Options");
            menuStrip1.Items[2].Text = getLanguageText("About", "About");

            loadConfigurationToolStripMenuItem.Text = getLanguageText("LoadConfig", "Load Configuration");
            saveConfigurationToolStripMenuItem.Text = getLanguageText("SaveConfig", "Save Configuration");

            temperatureUnitsToolStripMenuItem.Text = getLanguageText("TempUnits", "Temperature Units");
            dateFormatToolStripMenuItem.Text = getLanguageText("DateFormat", "Date Format");
            timeFormatToolStripMenuItem.Text = getLanguageText("TimeFormat", "Time Format");
            mMDDYYYYToolStripMenuItem.Text = getLanguageText("DateFormatMDY", "mm/dd/yyyy");
            dDMMYYYYToolStripMenuItem.Text = getLanguageText("DateFormatDMY", "dd/mm/yyyy");
            TwelveHourToolStripMenuItem.Text = getLanguageText("12Hour", "12-hour");
            TwentyFourHourToolStripMenuItem.Text = getLanguageText("24Hour", "24-hour");

            // Tab Control Items //////////////////// 
            tabControl.TabPages[0].Text = getLanguageText("Status", "Status");
            tabControl.TabPages[1].Text = getLanguageText("Config", "Config");
            tabControl.TabPages[2].Text = getLanguageText("Sensors", "Sensors");
            tabControl.TabPages[3].Text = getLanguageText("Data", "Data");


            // Config Page Items //////////////////// 
            labelPackageName.Text = getLanguageText("PackageName", "Package Name");
            labelSamplePeriod.Text = getLanguageText("SamplePeriod", "Sample Period");
            labelTestDuration.Text = getLanguageText("TestDuration", "Test Duration");
            labelConfigHr.Text = getLanguageText("HourAbbr", "Hr.");
            labelConfigMin.Text = getLanguageText("MinuteAbbr", "Min.");
            labelConfigSec.Text = getLanguageText("SecondAbbr", "Sec.");
            labelConfigSec2.Text = getLanguageText("SecondAbbr", "Sec.");
            checkBoxSyncTimeDate.Text = getLanguageText("SyncTimeDate", "Synchronize Time and Date");
            radioButtonUseSysTime.Text = getLanguageText("UseSysTimeDate", "Use System Time/Date");
            labelStartDelay.Text = getLanguageText("StartDelay", "Start Delay");
            radioButtonStartDelayNone.Text = getLanguageText("None", "None");
            radioButtonStartDelayOneMin.Text = getLanguageText("1Min", "1 Min.");
            radioButtonStartDelayThreeMin.Text = getLanguageText("3Min", "3 Min.");

            buttonConfigDiscardChanges.Text = getLanguageText("DiscardChanges", "Discard Changes");

            // Status Page Items //////////////////// 
            buttonConnectDisconnect.Text = getLanguageText("ConnectDisconnect", "Connect/Disconnect");
            buttonSync.Text = getLanguageText("Sync", "Sync");
            buttonClearWindow.Text = getLanguageText("ClearWindow", "Clear Window");
            RefreshStatusPageText();

            if (serial.Arduino != null && serial.Arduino.TestStarted)
                buttonStatusStartStop.Text = getLanguageText("StopTest", "Stop Test");
            else
                buttonStatusStartStop.Text = getLanguageText("StartTest", "Start Test");

            // Add the rest of the language support in the same way as above or by calling LanguageText when needed. 

            UpdateTabText();
        }

        private void RefreshStatusPageText()
        {
            //RefreshControlsEnable();
            ButtonStatusStartStopRefresh();
            if (serial.Arduino == null)
            {
                labelStatusPackageName.Text = getLanguageText("PackageName", "Package Name");
                labelStatusSerialPort.Text = getLanguageText("SerialPort", "Serial Port");
                labelStatusTotalSensors.Text = getLanguageText("TotalSensors", "Total Sensors") + ": __"; // A placeholder
                labelStatusMode.Text = getLanguageText("Mode", "Mode") + ":";

                labelStatusElapsedTime.Text = getLanguageText("ElapsedTime", "Elapsed Time") + ": __/__";    // A placeholder
                labelStatusDataFile.Text = getLanguageText("CurrentDataFile", "Current Data File") + ": __"; // A placeholder

            }
            else
            {
                labelStatusPackageName.Text = serial.Arduino.PackageName;
                labelStatusSerialPort.Text = "(" + (serial.Arduino.Port?.PortName ?? "ERROR") + ")";
                labelStatusTotalSensors.Text = getLanguageText("TotalSensors", "Total Sensors") + ": __"; // A placeholder
                if (serial.Arduino.TestStarted)
                {
                    labelStatusMode.Text = getLanguageText("Mode", "Mode") + ": " + getLanguageText("Running", "Running");
                }
                else
                {
                    labelStatusMode.Text = getLanguageText("Mode", "Mode") + ": " + getLanguageText("Ready", "Ready");
                }

                labelStatusElapsedTime.Text = getLanguageText("ElapsedTime", "Elapsed Time") + ": __/__";  // A placeholder 
                labelStatusDataFile.Text = getLanguageText("CurrentDataFile", "Current Data File") + ": __";  // A placeholder
            }
        }

        private void ButtonStatusStartStopRefresh()
        {
            if (serial.Arduino != null)
            {
                buttonStatusStartStop.Enabled = true;
                if (serial.Arduino.TestStarted)
                    buttonStatusStartStop.Text = getLanguageText("StopTest", "Stop Test");
                else
                    buttonStatusStartStop.Text = getLanguageText("StartTest", "Start Test");
            }
            else  // No arduino connected, so the Start/Stop Test button should be disabled  
            {
                buttonStatusStartStop.Enabled = false;
                buttonStatusStartStop.Text = getLanguageText("StartTest", "Start Test"); // It should say "Start Test" when disabled
            }
        }

        #endregion

    }

}
