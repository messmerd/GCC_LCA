using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

//using System.Runtime;
//using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;
using System.Threading;
using System.Threading.Tasks;

//using System.Runtime.CompilerServices;

using Windows;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Devices.Enumeration.Pnp;
using Windows.Foundation;
using System.Management;

//using Arduino_Serial_Interface;

namespace LCA_SYNC
{

    public partial class Main : Form
    {
        SerialInterface serial;
        object[] deviceList;
        private BindingSource arduinoListBinding;
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

            //serial.Arduino.NewDataReceived += arduinoBoard_NewDataReceived;  // !!!!!!!!!!!!!!!!!!!!!!!!!!!  Will need to implement this somehow. (From the Arduino side?)
            //serial.USBPnPDeviceChanged += serial_USBPnPDeviceChanged;
            //serial.LCAArduinos_Changed += serial_LCAArduinos_Changed;
            //serial.LCAArduinos.OnAdd += serial_LCAArduinos_Changed;
            //serial.LCAArduinos.OnRemove += serial_LCAArduinos_Changed;

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


            //deviceList = (serial.LCAArduinos.Select(a => a.displayName).Cast<object>().ToArray());

            // Status Page setup:
            buttonStatusStartStop.Tag = false;

            // Config Page setup: 
            numericUpDownSampleRate.Tag = (decimal)1;           // The tag will store the arduino's current value
            numericUpDownTestDurationHours.Tag = (decimal)0;    // The tag will store the arduino's current value
            numericUpDownTestDurationMinutes.Tag = (decimal)0;  // The tag will store the arduino's current value
            numericUpDownTestDurationSeconds.Tag = (decimal)30; // The tag will store the arduino's current value
            radioButtonStartDelayNone.Tag = true;               // Checked by default 
            radioButtonStartDelayOneMin.Tag = false;            // Unchecked by default 
            radioButtonStartDelayThreeMin.Tag = false;          // Unchecked by default 
            checkBoxSyncTimeDate.Tag = false;                   // Unchecked by default 
            textBoxPackageName.Tag = "";
            textBoxPackageName.TextChanged += TextBoxPackageName_TextChanged;
            numericUpDownSampleRate.ValueChanged += NumericUpDownSampleRate_ValueChanged;
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
            //RefreshLanguage(); 

            serial.ArduinoDataChanged += serial_ArduinoDataChanged;
            Console.WriteLine("\n\n\n");

            // Arduino List setup: 
            deviceList = new object[] {"<No Device>"};
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

        }

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
                textBoxPackageName.BackColor = Color.White; 
            }
            else
            {
                textBoxPackageName.BackColor = Color.Red;
            }

            UpdateTabText(); 
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

        private void NumericUpDownSampleRate_ValueChanged(object sender, EventArgs e)
        {
            //Console.WriteLine("Value: {0}, Tag: {1}", numericUpDownSampleRate.Value, (decimal?)numericUpDownSampleRate?.Tag);
            
            if (Math.Floor(numericUpDownSampleRate.Value * 8) != numericUpDownSampleRate.Value * 8) // If the sample rate is not a multiple of 0.125 
            {
                //Console.WriteLine("numericUpDownSampleRate: Not a multiple of 0.125");
                numericUpDownSampleRate.Value = Math.Round(numericUpDownSampleRate.Value * 8) / 8; // Set it to the nearest multiple of 0.125 
            }
            else  // Valid input
            {
                //Console.WriteLine("numericUpDownSampleRate: Just right");
            }

            UpdateTabText(); 
        }

        private void UpdateTabText()
        {
            // Config Tab: 
            if ((decimal?)numericUpDownSampleRate.Tag != numericUpDownSampleRate.Value ||
                (decimal?)numericUpDownTestDurationHours.Tag != numericUpDownTestDurationHours.Value ||
                (decimal?)numericUpDownTestDurationMinutes.Tag != numericUpDownTestDurationMinutes.Value ||
                (decimal?)numericUpDownTestDurationSeconds.Tag != numericUpDownTestDurationSeconds.Value ||
                (string)textBoxPackageName.Tag != textBoxPackageName.Text || 
                (bool)radioButtonStartDelayNone.Tag != radioButtonStartDelayNone.Checked ||
                (bool)radioButtonStartDelayOneMin.Tag != radioButtonStartDelayOneMin.Checked ||
                (bool)radioButtonStartDelayThreeMin.Tag != radioButtonStartDelayThreeMin.Checked ||
                checkBoxSyncTimeDate.Checked == true)
            {

                //Console.WriteLine("Difference detected.");
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

            // If changes have been made and those changes are valid: 
            if ((tabControl.TabPages[1].Text.Last() == '*' || tabControl.TabPages[2].Text.Last() == '*') && 
                textBoxPackageName.BackColor == Color.White)
            {
                buttonSync.Enabled = true;
            }
            else
            {
                buttonSync.Enabled = false;
            }

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
            buttonConfigDiscardChanges.Text = getLanguageText("DiscardChanges", "Discard Changes");

            // Add the rest of the language support in the same way as above or by calling LanguageText when needed. 

            UpdateTabText(); 
        }


        /// <summary>
        /// OnWeatherDataReceived event is catched in
        /// order to update the weather data display on the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void arduinoBoard_NewDataReceived(object sender, EventArgs e)
        {
            // Hasn't been tested: 
            Console.WriteLine("Message received from {0}.",((ArduinoBoard)sender).DisplayName /*, ((ArduinoBoard)sender).ReceivedData*/);
            

            /*
            Dispatcher.BeginInvoke(new ThreadStart(DrawChart));
            Dispatcher.BeginInvoke(new ThreadStart(() =>
                weatherDataGrid.ItemsSource = weatherData.WeatherDataItems));
            */
        }

        void serial_ArduinoDataChanged(object sender, ArduinoEventArgs e)
        {
            // Can do more here: 
            // Use e.Reason for the reason the event was called 

            switch ((string)e.Type)
            {
                case "PackageName":
                    textBoxPackageName.Text = serial.Arduino.PackageName;
                    textBoxPackageName.Tag = textBoxPackageName.Text;
                    break;
                case "StartDelay":
                    if (serial.Arduino.StartDelay == 0)  // No start delay 
                    {
                        radioButtonStartDelayNone.Checked = true;
                        radioButtonStartDelayNone.Tag = true;
                        radioButtonStartDelayOneMin.Checked = false;
                        radioButtonStartDelayOneMin.Tag = false;
                        radioButtonStartDelayThreeMin.Checked = false;
                        radioButtonStartDelayThreeMin.Tag = false;
                    }
                    else if (serial.Arduino.StartDelay == 60)  // One minute 
                    {
                        radioButtonStartDelayNone.Checked = false;
                        radioButtonStartDelayNone.Tag = false;
                        radioButtonStartDelayOneMin.Checked = true;
                        radioButtonStartDelayOneMin.Tag = true;
                        radioButtonStartDelayThreeMin.Checked = false;
                        radioButtonStartDelayThreeMin.Tag = false;
                    }
                    else if (serial.Arduino.StartDelay == 60 * 3) // Three minutes
                    {
                        radioButtonStartDelayNone.Checked = false;
                        radioButtonStartDelayNone.Tag = false;
                        radioButtonStartDelayOneMin.Checked = false;
                        radioButtonStartDelayOneMin.Tag = false;
                        radioButtonStartDelayThreeMin.Checked = true;
                        radioButtonStartDelayThreeMin.Tag = true;
                    }
                    else
                    {
                        Console.WriteLine("Unacceptable value for StartDelay.");
                    }
                    break;
                case "TestDuration":
                    numericUpDownTestDurationHours.Value = Math.Floor((decimal)serial.Arduino.TestDuration/60/60); // Get hours from total seconds 
                    numericUpDownTestDurationHours.Tag = numericUpDownTestDurationHours.Value;   // Tag is the value the arduino is set to 

                    numericUpDownTestDurationMinutes.Value = Math.Floor((decimal)serial.Arduino.TestDuration / 60) % 60; // Get minutes from total seconds 
                    numericUpDownTestDurationMinutes.Tag = numericUpDownTestDurationMinutes.Value;   // Tag is the value the arduino is set to 

                    numericUpDownTestDurationSeconds.Value = (decimal)serial.Arduino.TestDuration % 60; // Get seconds from total seconds 
                    numericUpDownTestDurationSeconds.Tag = numericUpDownTestDurationSeconds.Value;   // Tag is the value the arduino is set to 

                    //Console.WriteLine("{0}:{1}:{2}", Math.Floor((decimal)serial.Arduino.TestDuration / 60 / 60), Math.Floor((decimal)serial.Arduino.TestDuration / 60) % 60, (decimal)serial.Arduino.TestDuration % 60);
                    break;
                case "SamplePeriod":
                    numericUpDownSampleRate.Value = (decimal)serial.Arduino.SamplePeriod; 
                    numericUpDownSampleRate.Tag = (decimal)serial.Arduino.SamplePeriod;   // Tag is the value the arduino is set to 
                    break;

                default:
                    Console.WriteLine("In serial_ArduinoDataChanged, an unrecognized arduino variable was changed. ");
                    break;
            }

            arduinoListBinding.ResetBindings(false);

            UpdateTabText();
        }

        /// <summary>
        /// USBDeviceChanged event is caught in
        /// order to prevent send/receive errors
        /// and allow the program to connect to
        /// the Arduino automatically.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /*
        void serial_USBPnPDeviceChanged(object sender, EventArgs e)
        {

            // Need to find way to distinguish between adding and removing, b/c this event is triggered by both

            //Console.WriteLine("weatherData_USBPnPDeviceChanged event");

            //Console.WriteLine("You have added or removed a USB device. weatherData_USBDeviceChanged.\nEventArgs={0}\nsender={1}",e.ToString(),sender.ToString());
            //Console.WriteLine(((ManagementBaseObject)(((EventArrivedEventArgs)e).Context))["TargetInstance"].ToString());

            
            string wclass = ((EventArrivedEventArgs)e).NewEvent.SystemProperties["__Class"].Value.ToString();
            string wop = string.Empty;
            switch (wclass)
            {
                case "__InstanceModificationEvent":
                    wop = "modified";
                    break;
                case "__InstanceCreationEvent":
                    wop = "created";
                    break;
                case "__InstanceDeletionEvent":
                    wop = "deleted";
                    break;
            }

            ManagementBaseObject device = (ManagementBaseObject)(((EventArrivedEventArgs)e).NewEvent)["TargetInstance"]; 

            if (serial.Arduino != null && device.Equals(serial.Arduino.mgmtBaseObj)) // If the added/removed device is the LCA Arduino in use
            {
                Console.WriteLine("The LCA arduino device you were using was {0}.", wop);
                //Console.WriteLine(serial.Arduino.Port.PortName);
                MessageBox.Show("The LCA arduino device you were using was " + wop + ".");

                // Update LCA Arduino List here
                // Do code to stop from writing to port or start writing, or whatever
            }
            else if (serial.LCAArduinos.ToList().Exists(a => a.Port.PortName == SerialInterface.GetPortName(device))) // If the added/removed device was an LCA Arduino not in use
            {
                Console.WriteLine("An LCA arduino device you were not using was {0}.", wop);
                //Console.WriteLine(serial.Arduino.Port.PortName);
                MessageBox.Show("An LCA arduino device you were not using was " + wop + ".");

                // Update LCA Arduino List here
            }
            else  // The added/removed device was not an LCA Arduino (an LCA Arduino device previously known by the program)
            {
                Console.WriteLine("A non-LCA-Arduino device was {0}.", wop);
                //Console.WriteLine(serial.Arduino.Port.PortName);
                MessageBox.Show("A non-LCA-Arduino device was " + wop + ".");

                // Nothing else needs done since non-LCA-Arduino devices are irrelevant
            }
            //((ManagementBaseObject)e["TargetInstance"])["Name"]
            //((ManagementEventWatcher)sender).

        }
        */ 


        /// <summary>
        /// LCAArduinos_Changed event is caught in
        /// order to update anything when LCAArduinos
        /// list is changed or values within it are 
        /// changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void serial_LCAArduinos_Changed(object sender, ListChangedEventArgs e)  // Maybe use arduinoList.DataSourceChanged event instead? 
        {
            Console.Write("LCAArduinos has been changed: ");
            Console.Write("ListChanagedType = {0}, ", e.ListChangedType.ToString());
            Console.Write("OldIndex = {0}, ", e.OldIndex.ToString());
            Console.WriteLine("NewIndex = {0}. ", e.NewIndex.ToString());
            //Console.WriteLine("PropertyDescriptor = {0}.", e.PropertyDescriptor.ToString());  // This isn't displaying for some reason.. An exception?

        }


        // Click 'Sync'
        private async void buttonSync_Click(object sender, EventArgs e)
        {
            // This method currently doesn't check to see if it syncs successfully... Might be a problem
            UpdateTabText();
            try
            {
                if (tabControl.TabPages[1].Text.Last() == '*')  // If config page needs synced 
                {
                    this.UseWaitCursor = true; // This isn't working for some reason 
                    buttonSync.Enabled = false;
                    if ((decimal?)numericUpDownSampleRate.Tag != numericUpDownSampleRate.Value)
                        await serial.Arduino.Communicate(DATACATEGORY.CONFIG, SUBCATEGORY.SAMPLE_PERIOD, ACTION.WRITEVAR, (float)numericUpDownSampleRate.Value);
                    if ((decimal?)numericUpDownTestDurationHours.Tag != numericUpDownTestDurationHours.Value || (decimal?)numericUpDownTestDurationMinutes.Tag != numericUpDownTestDurationMinutes.Value || (decimal?)numericUpDownTestDurationSeconds.Tag != numericUpDownTestDurationSeconds.Value)
                        await serial.Arduino.Communicate(DATACATEGORY.CONFIG, SUBCATEGORY.TEST_DUR, ACTION.WRITEVAR, (uint)numericUpDownTestDurationHours.Value * 60 * 60 + (uint)numericUpDownTestDurationMinutes.Value * 60 + (uint)numericUpDownTestDurationSeconds.Value);
                    if ((string)textBoxPackageName.Tag != textBoxPackageName.Text && textBoxPackageName.BackColor == Color.White)
                        await serial.Arduino.Communicate(DATACATEGORY.CONFIG, SUBCATEGORY.PACKAGE_NAME, ACTION.WRITEVAR, textBoxPackageName.Text);
                    if (checkBoxSyncTimeDate.Checked == true)
                    {
                        // This isn't implemented yet in ArduinoBoard.cs or in the arduino code
                        if (radioButtonUseSysTime.Checked) // System time
                        {
                            await serial.Arduino.Communicate(DATACATEGORY.CONFIG, SUBCATEGORY.SET_TIME_DATE, ACTION.WRITEVAR, DateTime.Now);
                            checkBoxSyncTimeDate.Checked = false; // Should this verify that the time was set correctly first? 
                        }
                        else  // Custom time 
                        {
                            await serial.Arduino.Communicate(DATACATEGORY.CONFIG, SUBCATEGORY.SET_TIME_DATE, ACTION.WRITEVAR, dateTimePickerCustomTime.Value);
                            checkBoxSyncTimeDate.Checked = false;
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
                Console.WriteLine("Error when syncing. Error message: ", ex.Message); 
            }
            finally
            {
                this.UseWaitCursor = false; 
                UpdateTabText();
            }
            

        }

        private void button1_Click_1(object sender, EventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        /*
        public static TResult Await<TResult>(this IAsyncOperation<TResult> operation)
        {
            try
            {
                return operation.GetResults();
            }
            finally
            {
                operation.Close();
            }
        }
        */
        

        private async void button1_Click_2(object sender, EventArgs e)
        {
            Console.WriteLine("start");
            var Uuid = GattServiceUuids.HeartRate;

            Console.WriteLine(GattServiceUuids.HeartRate);

            //GattDeviceService.GetDeviceSelectorFromUuid(
            //IAsyncOperation<DeviceInformationCollection>
            string selector = GattDeviceService.GetDeviceSelectorFromUuid(Uuid);

            //var results = Await<DeviceInformationCollection>(DeviceInformation.FindAllAsync(selector, new string[] { "System.Devices.ContainerId" }));
            Console.WriteLine("here0");

            
            var op = DeviceInformation.FindAllAsync(selector);  // Needs await? But await needs System.Runtime.WindowsRuntime which apparently isn't supported currently on .NET
            Console.WriteLine("here1");

            var tcs = new TaskCompletionSource<DeviceInformationCollection>();
            op.Completed = delegate
            {
                if (op.Status == Windows.Foundation.AsyncStatus.Completed) { tcs.SetResult(op.GetResults()); }
                else if (op.Status == Windows.Foundation.AsyncStatus.Error) { tcs.SetException(op.ErrorCode); }
                else { tcs.SetCanceled(); }
            };
            var result = await tcs.Task;


            Console.WriteLine("here2");

            Console.WriteLine(result.Count);
            
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void toolStripContainer1_TopToolStripPanel_Click(object sender, EventArgs e)
        {

        }

        private void buttonRoundSync_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        // Untested: 
        private async void buttonArduinoSync_Click(object sender, EventArgs e)
        {
         
            // RefreshInfo for all arduinos already added: 
            foreach (var ard in serial.LCAArduinos)
            {
                await ard.RefreshInfo();
            }

            await serial.ActivateAllArduinos();  // Activates (Ping + RefreshInfo) all arduinos that have not been added yet

        }


        private async void buttonStatusStartStop_Click(object sender, EventArgs e)
        {
            // Doesn't work yet. Not implemented in ArduinoBoard.cs or in the arduino code  

            if ((bool)buttonStatusStartStop.Tag == false)  // Test is currently not running 
            {
                Response resp = new Response("", ArduinoBoard.COMMERROR.NULL);
                try
                {
                    buttonStatusStartStop.Enabled = false;
                    resp = await serial.Arduino.Communicate(DATACATEGORY.OTHER, SUBCATEGORY.START_TEST, ACTION.WRITEVAR);
                    buttonStatusStartStop.Enabled = true;
                }
                catch
                {

                }
                finally
                {
                    if (resp.validity == ArduinoBoard.COMMERROR.VALID)
                    {
                        buttonStatusStartStop.Tag = true;  // Test is running
                        buttonStatusStartStop.Text = getLanguageText("StopTest", "Stop Test");
                    }
                    buttonStatusStartStop.Enabled = true;
                }
            }
            else
            {
                Response resp = new Response("", ArduinoBoard.COMMERROR.NULL);
                try
                {
                    buttonStatusStartStop.Enabled = false;
                    resp = await serial.Arduino.Communicate(DATACATEGORY.OTHER, SUBCATEGORY.STOP_TEST, ACTION.WRITEVAR);
                    buttonStatusStartStop.Enabled = true;
                }
                catch
                {

                }
                finally
                {
                    if (resp.validity == ArduinoBoard.COMMERROR.VALID)
                    {
                        buttonStatusStartStop.Tag = false;  // Test is stopped
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


        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        private void buttonConfigDiscardChanges_Click(object sender, EventArgs e)
        {
            numericUpDownSampleRate.Value = (decimal)numericUpDownSampleRate.Tag; 

            numericUpDownTestDurationHours.Value = (decimal)numericUpDownTestDurationHours.Tag;
            numericUpDownTestDurationMinutes.Value = (decimal)numericUpDownTestDurationMinutes.Tag;
            numericUpDownTestDurationSeconds.Value = (decimal)numericUpDownTestDurationSeconds.Tag;

            textBoxPackageName.Text = (string)textBoxPackageName.Tag;

            radioButtonStartDelayNone.Checked = (bool)radioButtonStartDelayNone.Tag;
            radioButtonStartDelayOneMin.Checked = (bool)radioButtonStartDelayOneMin.Tag;
            radioButtonStartDelayThreeMin.Checked = (bool)radioButtonStartDelayThreeMin.Tag;

            checkBoxSyncTimeDate.Checked = (bool)checkBoxSyncTimeDate.Tag; 

            UpdateTabText();
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
        

    }

    



}
