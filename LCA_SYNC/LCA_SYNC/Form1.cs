using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
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

using Arduino_Serial_Interface;

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

        public Main()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(Main_UnhandledException);

            serial = SerialInterface.Create();

            //serial.Arduino.NewDataReceived += arduinoBoard_NewDataReceived;  // !!!!!!!!!!!!!!!!!!!!!!!!!!!  Will need to implement this somehow. (From the Arduino side?)
            serial.USBPnPDeviceChanged += serial_USBPnPDeviceChanged;

            this.FormClosing += Main_FormClosing;  // Trying w/o this. B/c PnP watcher stopped working again for some reason

            InitializeComponent();
            //deviceList = (serial.LCAArduinos.Select(a => a.displayName).Cast<object>().ToArray());

            deviceList = new object[] {"<No Device>"};

            //comboBox1.DataSource = serial.LCAArduinos;
            //comboBox1.DisplayMember = "displayName";
            //comboBox1.ValueMember = "Port.PortName";
            

            //comboBox1.Items.AddRange(serial.LCAArduinos.Select(a => a.displayName).Cast<object>().ToArray());
            comboBox1.Items.AddRange(deviceList);
            comboBox1.SelectedIndex = 0;

            serial.StartPnPWatcher();  // Start watching for USB PnP devices to be added/removed/modified. 


            // Find LCA arduinos

            serial.LocateLCADevices();  // PnP watcher doesn't work whether this goes or not...


            /*
            // Do this AFTER finding the arduinos: 
            try
            {
                serial.Arduino.OpenConnection();
                
            }
            catch (Exception e)
            {
                MessageBox.Show("Error: Can not connect to the Arduino Board - Configure the COM Port in the app.config file and check whether an Arduino Board is connected to your computer.");
                MessageBox.Show(e.Message);
            }
            */

            // Probably should do all this differently: 
            //List<Tuple<String, UInt16>> result = serial.ArduinosConnected();

            /*
            if (result.Count > 1)
            {
                MessageBox.Show("There appears to be more than one Arduino connected.");

                // Deal with this issue (Have drop-down menu of arduinos to pick from on main screen? )
                // Also deal with fake arduinos 
            }
            else if (result.Count == 1)
            {
                switch (result.First().Item2)
                {
                    case 0x00:
                        MessageBox.Show("You've connected a fake arduino on port " + result.First().Item1 + ", but it is not responding correctly.");
                        
                        break;
                    case 0x01:
                        MessageBox.Show("You've connected a fake arduino on port " + result.First().Item1 + ".");
                        break;
                    case 0x10:
                        MessageBox.Show("You've connected an arduino on port " + result.First().Item1 + ", but it is not responding correctly.");
                        break;
                    case 0x11:
                        MessageBox.Show("You've connected an arduino on port " + result.First().Item1 + ".");
                        break;
                    default:
                        MessageBox.Show("Error.");
                        break;
                }
            }
            else
            {
                MessageBox.Show("No Arduino detected.");
            }
            */

            

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

        /// <summary>
        /// OnWeatherDataReceived event is catched in
        /// order to update the weather data display on the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void arduinoBoard_NewDataReceived(object sender, EventArgs e)
        {
            // Hasn't been tested: 
            Console.WriteLine("Message from {0}:\n{1}",((ArduinoBoard)sender).displayName, ((ArduinoBoard)sender).ReceivedData);
            
            

            /*
            Dispatcher.BeginInvoke(new ThreadStart(DrawChart));
            Dispatcher.BeginInvoke(new ThreadStart(() =>
                weatherDataGrid.ItemsSource = weatherData.WeatherDataItems));
            */
        }


        /// <summary>
        /// USBDeviceChanged event is catched in
        /// order to prevent send/receive errors
        /// and allow the program to connect to
        /// the Arduino automatically.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
            else if (serial.LCAArduinos.Exists(a => a.Port.PortName == SerialInterface.GetPortName(device))) // If the added/removed device was an LCA Arduino not in use
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
    }

    



}
