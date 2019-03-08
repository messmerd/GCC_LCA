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
        
        /// <summary>
        /// Contains a collection of <see cref="WeatherDataItem"/>
        /// and control methods to send commands to the Arduino Board
        /// </summary>
        SerialInterface serial;
        object[] deviceList;
        private BindingSource arduinoListBinding;
        private Dictionary<string, string> LanguageText;
        //private Dictionary<string, Image> LanguageIcons;
        private ImageList LanguageIcons; 
        private SortedSet<string> AvailableLanguages; 
        private string CurrentLanguage = "";
        private readonly string DefaultLanguage = "en";
        
        
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
            //deviceList = (serial.LCAArduinos.Select(a => a.displayName).Cast<object>().ToArray());

            imageComboLanguage.DropDownClosed += imageComboLanguage_DropDownClosed;  // To unhighlight the selection 
            imageComboLanguage.KeyDown += imageComboLanguage_KeyDown;
            imageComboLanguage.SelectedIndexChanged += imageComboLanguage_SelectedIndexChanged;

            LanguageText = new Dictionary<string, string>();
            AvailableLanguages = new SortedSet<string>();
            LanguageIcons = new ImageList();
            CurrentLanguage = ""; 
            //imageComboLanguage.
            LoadLanguages();

            //RefreshLanguage(); 


            Console.WriteLine("\n\n\n");
            deviceList = new object[] {"<No Device>"};

            //comboBox1.DataSource = serial.LCAArduinos;
            //comboBox1.DisplayMember = "displayName";
            //comboBox1.ValueMember = "Port.PortName";


            //comboBox1.Items.AddRange(serial.LCAArduinos.Select(a => a.displayName).Cast<object>().ToArray());

            //arduinoList.Items.AddRange(deviceList);
            arduinoListBinding = new BindingSource();
            arduinoListBinding.DataSource = serial.LCAArduinos;
            arduinoListBinding.ListChanged += serial_LCAArduinos_Changed;
            serial.ArduinoDataChanged += serial_ArduinoDataChanged;

            arduinoList.DataSource = arduinoListBinding;
            arduinoList.DisplayMember = "displayName";
            //arduinoList.ValueMember = "Self";   // What is the default value of this? Is it self? 
            
            //arduinoList.SelectedIndex = 0;

            serial.StartPnPWatcher();  // Start watching for USB PnP devices to be added/removed/modified. 

            // Find LCA arduinos
            serial.LocateLCADevices();


            // Probably should do all this differently: 
            //List<Tuple<String, UInt16>> result = serial.ArduinosConnected();

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
            string langShort;
            string[] keyvalue = new string[2]; // keyvalue[0] = the key; keyvalue[1] = the value 

            if (!Directory.Exists(Application.StartupPath + @"\lang\"))
            {
                Console.WriteLine("Error: The directory lang\\ does not exist.");
                return; 
            }

            string[] filePaths = Directory.GetFiles(Application.StartupPath + @"\lang\", "*.dat").OrderBy(s => s).ToArray();
            //Array.Sort(filePaths, (x, y) => String.Compare(x, y));

            Console.WriteLine("Number of language files found: {0}", filePaths.Length);

            ImageComboItem comboItem; 
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
                            LanguageIcons.Images.Add(Properties.Resources.errorLang);
                            Console.WriteLine("Error loading language icon for the language: {0}.", langShort);
                        }
                    }
                    else  // No language icon was specified 
                    {
                        LanguageIcons.Images.Add(Properties.Resources.noLangIcon);
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

            if (CurrentLanguage == "" && AvailableLanguages.Contains(DefaultLanguage))
            {
                CurrentLanguage = DefaultLanguage; 
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

        private string getLanguageText(string lang, string key)
        {
            // I think this method is unused 
            if (LanguageText.ContainsKey(lang + key))
            {
                return LanguageText[lang + key]; 
            }
            else
            {
                return "Error: Text not found for language " + lang + " and key " + key + ".";
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
            menuStrip1.Items[0].Text = LanguageText?[CurrentLanguage + "File"];
            menuStrip1.Items[1].Text = LanguageText?[CurrentLanguage + "Options"];
            menuStrip1.Items[2].Text = LanguageText?[CurrentLanguage + "About"];
            // Change inner menustrip items here 
            //menuStrip1.Refresh();

            tabControl.TabPages[0].Text = LanguageText?[CurrentLanguage + "Status"];
            tabControl.TabPages[1].Text = LanguageText?[CurrentLanguage + "Config"];
            tabControl.TabPages[2].Text = LanguageText?[CurrentLanguage + "Sensors"];
            tabControl.TabPages[3].Text = LanguageText?[CurrentLanguage + "Data"];
            // !!!! The tab pages are too big when in French!!!!!

            // Add the rest of the language support in the same way as above or by calling LanguageText when needed. 
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
            arduinoListBinding.ResetBindings(false);
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
        


        private void Form1_Load(object sender, EventArgs e)
        {

        }

        // Click 'Sync'
        private void button1_Click(object sender, EventArgs e)
        {
            /*
            try
            {
                serial.Arduino.GetWeatherData();
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }
            */

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
            foreach (var ard in serial.LCAArduinos)
            {
                await ard.RefreshInfo();
            }
            serial.LocateLCADevices(); 
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }



    }

    



}
