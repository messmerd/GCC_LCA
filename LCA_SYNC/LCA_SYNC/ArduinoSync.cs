
//#define DEBUG_MODE  // Uncomment for Debug mode; Comment for Arduino mode

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Configuration;
using System.Configuration.Assemblies;
using System.Management;
using System.Text.RegularExpressions;
using Windows.Management;

namespace Arduino_Serial_Interface
{

    public enum DATACATEGORY { NULL, PING, CONFIG, SENSORS, SENSORDATA, REALTIME, OTHER };
    // enum for arduino operating states? (Unresponsive, RunningTest, ComputerMode, etc.)

    /// <summary>
    /// Encapsulates the communication from and to
    /// an Arduino Board which sends weather data
    /// it has stored previously
    /// </summary>
    public class SerialInterface
    {
        /// <summary>
        /// Interface for the Serial Port at which an Arduino Board
        /// is connected.
        /// </summary>

        private static SerialInterface singleton;

        public static SerialInterface Create()
        {

            if (singleton == null)
            {
                singleton = new SerialInterface();
            }
            return singleton;

        }

        ~SerialInterface()
        {
            Console.WriteLine();
            if (pnpWatcher != null)
            {
                if (pnpWatcherHandler != null)
                {
                    pnpWatcher.EventArrived -= pnpWatcherHandler;
                }
                pnpWatcher.Stop();    // Will this fix the problem? 
                pnpWatcher.Dispose();
            }
        }

        public ArduinoBoard Arduino
        {
            get;
            set;

            /*  // Causes stack overflow exception for some reason: 
            get {return Arduino; }
            set
            {
                Arduino = value;
                // Notify/update anything (i.e. UI elements, data transfers) depending on Arduino here
            }
            */
        }
        public List<ArduinoBoard> LCAArduinos
        {
            get;
            set;
            /* // Causes stack overflow exception for some reason: 
            get { return LCAArduinos; }
            set
            {
                LCAArduinos = value;
                // Notify/update anything (i.e. UI elements, data transfers) depending on LCAArduinos here
            }
            */
        }
        public ManagementEventWatcher pnpWatcher { get; set; }
        public EventArrivedEventHandler pnpWatcherHandler { get; set; }


        private SerialInterface()  // Default constructor
        {
            //
            Arduino = null;
            LCAArduinos = new List<ArduinoBoard>();
            pnpWatcher = null;
            pnpWatcherHandler = null;

            //startPnPWatcher();

        }

        public void StartPnPWatcher()
        {
            // This code detects when a new PnP (Plug-n-Play) device is added or removed.
            
            String scope = "root\\CIMV2";
            ManagementScope scopeObject = new ManagementScope(scope);
            WqlEventQuery queryObject = 
                new WqlEventQuery("__InstanceOperationEvent", 
                new TimeSpan(0, 0, 1), 
                "TargetInstance isa \"Win32_PnPEntity\"");

            pnpWatcher = new ManagementEventWatcher();
            pnpWatcher.Scope = scopeObject;
            pnpWatcher.Query = queryObject;

            pnpWatcherHandler = new EventArrivedEventHandler(USBPnPDeviceChanged);

            pnpWatcher.EventArrived += pnpWatcherHandler;

            pnpWatcher.Options.Timeout = new TimeSpan(0, 0, 5);
            //new EventArrivedEventHandler(USBDeviceChanged);
            pnpWatcher.Start();

        }

        /// <summary>
        /// Holds a List of <see cref="WeatherDataItem"/> in order
        /// to store weather data received from an Arduino Board.
        /// </summary>
        List<WeatherDataItem> weatherDataItems = new List<WeatherDataItem>();

        /// <summary>
        /// Raised when a USB device is connected
        /// or disconnected from the computer. 
        /// </summary>
        public event EventHandler USBPnPDeviceChanged;

        /// <summary>
        /// Gets a list of <see cref="WeatherDataItem"/> which was
        /// previsously retrieved from an Arduino Board.
        /// </summary>
        internal List<WeatherDataItem> WeatherDataItems
        {
            get { return weatherDataItems; }
        }

        


        public void PrintWow()
        {
            Console.WriteLine("Wow");
        }


        

        public void LocateLCADevices()
        {
            foreach (ManagementBaseObject dev in FindArduinos())
            {
                ActivateArduino(dev);

            }

        }
        

        public List<ManagementBaseObject> FindArduinos()  // Find all LCA Arduinos connected
        {

            //Console.WriteLine(System.IO.Ports.SerialPort.Events);

            // Returns a list of COM ports with an Arduino connected, and their status
            List<ManagementBaseObject> result = new List<ManagementBaseObject>();

            // In the tuple, String stores the port name, and UInt16 stores a status code:
            // Status codes:
            // 0xab
            // a --> 0 = fake Arduino; 1 = genuine Arduino 
            // b --> 0 = Arduino isn't the LCA Arduino or isn't working; 1 = Arduino is the LCA Arduino and is working

            // The 0403 VID was used by older Arduinos which use FTDI
            /* Now, Arduinos use the 2341 VID with the following PIDs:
             *  2341  Arduino SA
	                0001  Uno (CDC ACM)
	                0010  Mega 2560 (CDC ACM)
	                003b  Serial Adapter (CDC ACM)
	                003f  Mega ADK (CDC ACM)
	                0042  Mega 2560 R3 (CDC ACM)
	                0043  Uno R3 (CDC ACM)
	                0044  Mega ADK R3 (CDC ACM)
	                0045  Serial R3 (CDC ACM)
	                8036  Leonardo (CDC ACM, HID)
             */

            // Use WMI to get info
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2","SELECT * FROM Win32_PnPEntity WHERE ClassGuid=\"{4d36e978-e325-11ce-bfc1-08002be10318}\"");

            //Win32_SystemConfigurationChangeEvent
            //System.Management.ManagementClass manClass1 = new ManagementClass("Win32_SystemConfigurationChangeEvent");
            //System.Management.ManagementClass manClass2 = new ManagementClass("Win32_DeviceChangeEvent");
            //Console.WriteLine(manClass1.Methods.ToString()); 
            //ConfigManagerErrorCode

            // Search all serial ports
            foreach (ManagementObject queryObj in searcher.Get())
            {
                //PNPDeviceID = USB\VID_1A86&PID_7523\5&1A63D808&0&2
                if (null != queryObj["PNPDeviceID"])
                {
                    String vid = GetVID(queryObj);
                    String port = GetPortName(queryObj);

                    // Need an isLCAArduino method instead: 
                    if (vid == "2341" || (vid == "0403" && queryObj["Description"].ToString().Contains("Arduino")))  // Genuine arduino 
                    {

                        // Test by sending message and waiting for correct response here. 0x1x  <-- Find the x here

                        Console.WriteLine("Arduino detected at port {0}.", port);
                        //Tuple<String, UInt16> ard = new Tuple<String, UInt16>(port, 0x11);
                         
                        result.Add(queryObj);
                    }
                    else if (vid == "1a86" || queryObj["Description"].ToString().Contains("CH340") || queryObj["Manufacturer"].ToString().Contains("wch.cn") || queryObj["Service"].ToString().Contains("CH341SER_A64")) // Fake Arduino 
                    {

                        // Test by sending message and waiting for correct response here. 0x1x  <-- Find the x here

                        Console.WriteLine("Arduino detected at port {0}. It appears to be a counterfeit! ", port);
                        //Tuple<String, UInt16> ard = new Tuple<String, UInt16>(port, 0x01);
                        result.Add(queryObj);
                    }
                    else
                    {
                        Console.WriteLine("No Arduino at port {0}.", port);
                    }
                    
                }
            }

            Console.WriteLine("There are "+result.Count.ToString() + " arduino devices found by FindArduinos().");

            return result; 
        }


        public void ActivateArduino(ManagementBaseObject device)  // Verifies that an arduino is LCA, and activates it if it is
        {
            String port = GetPortName(device);

            Console.WriteLine("In ActivateArduino, the port is: "+port);  // good

            ArduinoBoard ard = LCAArduinos.Find(a => a.Port.PortName == port);  // Gives ArduinoBoard if one with that port exists, else null

            if (ard == null) // If an LCA arduino with the port specified doesn't exist in the LCAArduinos list, create an ArduinoBoard 
            {
                ard = new ArduinoBoard(device);
                ard.ExpectedResponseType = DATACATEGORY.PING;  // Getting ready to ping it
            }

            if (!ard.Port.IsOpen)  // ?
            {
                ard.OpenConnection();
            }

            //Console.WriteLine("In ActivateArduino, ard.Port.PortName is: " + ard.Port.PortName);

            

            System.Threading.CancellationTokenSource source = new System.Threading.CancellationTokenSource();  // Can/should this be used? 
            
            var t = Task.Run(async delegate
            {
                await Task.Delay(TimeSpan.FromMilliseconds(10000), source.Token);  // If the timespan is too long, it crashes!!!!!!!!!!!1
                if (!ard.lca)  // After 5 ms, if the arduino instance hasn't gotten a ping back telling that it's an LCA board
                {
                    if (LCAArduinos.Exists(a => a.Port.PortName == port))
                    {
                        // One of the LCA arduinos is not responding!
                        // Set some kind of status variable in arduino instance? (Unresponsive, RunningTest, ComputerMode, etc.)
                        //  
                    }
                    else
                    {
                        ard.Port = null;  // "Destroy" its serial port (unecessary?)
                        ard = null;       // "Destroy" the arduino instance, since ping was not received 
                    }
                    
                }
                else
                {
                    // Claim position as the "current", in-use arduino here, if it is untaken. 
                    ard.GetInfo(); // It pinged successfully, so it's a real LCA arduino. It should get more info about itself and add itself to the 
                    // await a delay? Delay for the length of a timeout.
                    // after delay, check that info has been received (syncNeeded == false). If it hasn't, give error or something. 
                    
                    // In arduino instance, add arduino to LCAArduinos, and also to Arduino if that was claimed earlier. (once data received)
                    // In arduino instance, destroy this thread (if possible) to prevent it from sitting here doing nothing until it times out. 
                }
                    
                //return 42;
            });


            //Console.WriteLine(ard.Port.WriteTimeout.ToString());
           // Console.WriteLine(ard.Port.IsOpen.ToString());
            //Console.WriteLine(ard.Port.PortName);

            if (ard.Port.IsOpen)
            {
                try
                {
                    ard.Port.Write("1ping");
                }
                catch (Exception e)
                {
                    Console.WriteLine("In ActivateArduino: "+e.Message);
                }
            }
            else
            {
                Console.WriteLine("Arduino isn't open.");
                throw new InvalidOperationException("Can't send data if the serial port is closed!");
            }

            Console.WriteLine("End of ActivateArduino.");

            /*
            System.Timers.Timer aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = 5000;
            aTimer.Enabled = true;
            */
            


        }



        // Unnecessary since we have getPortName? 
        public List<String> GetLCAArduinoPorts(List<ManagementBaseObject> arduinos)
        {
            // Right now this code just assumes all arduinos are LCA arduinos. 
            // Need to send special message to arduino and receive special response to detemine if it is an LCA Arduino. 

            List<String> lcaArduinoPorts = new List<String>();
            foreach (ManagementBaseObject ard in arduinos)
            {
                lcaArduinoPorts.Add(GetPortName(ard));
            }

            return lcaArduinoPorts; 
        }

        public static String GetPortName(ManagementBaseObject dev)
        {
            return new Regex(@"\((COM\d+)\)").Match(dev["Name"].ToString()).Groups[1].Value;
        }

        public static String GetVID(ManagementBaseObject dev)
        {
            return new Regex(@"(VID_)([0-9a-fA-F]+)").Match(dev["PNPDeviceID"].ToString()).Groups[2].Value.ToLower();
        }

        public static String GetPID(ManagementBaseObject dev)
        {
            return new Regex(@"(PID_)([0-9a-fA-F]+)").Match(dev["PNPDeviceID"].ToString()).Groups[2].Value.ToLower();
        }


        /*
        // Helper function to handle regex search
        public string regex(string pattern, string text)
        {
            Regex re = new Regex(pattern);
            Match m = re.Match(text); 
            if (m.Success)
            {
                return m.Value;
            }
            else
            {
                return null;
            }
        }

        public string AutodetectArduinoPort()
        {
            //ManagementScope connectionScope = new ManagementScope();
            ManagementScope connectionScope = new ManagementScope("root\\CIMV2");

            //SelectQuery serialQuery = new SelectQuery("SELECT * FROM Win32_SerialPort");
            SelectQuery serialQuery = new SelectQuery("SELECT * FROM Win32_PnPEntity WHERE ClassGuid=\"{4d36e978-e325-11ce-bfc1-08002be10318}\"");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(connectionScope, serialQuery);

            try
            {
                foreach (ManagementObject item in searcher.Get())
                {
                    //Console.WriteLine(item["Description"]);
                    //Console.WriteLine(item["DeviceID"]);
                    Console.WriteLine(item.GetText(TextFormat.Mof));
                    string desc = item["Description"].ToString();
                    string deviceId = item["DeviceID"].ToString();
                    

                    if (desc.Contains("Arduino"))
                    {
                        Console.WriteLine("Arduino detected.");
                        Console.WriteLine(deviceId);
                        return deviceId;
                    }
                }
            }
            catch (ManagementException e)
            {
                // Do Nothing 
            }

            return null;
        }
        */
    }

    class WeatherDataItem
    {
        DateTime date;
        public DateTime Date
        {
            get { return date; }
            set { date = value; }
        }

        float temperatureCelsius;
        public float TemperatureCelsius
        {
            get { return temperatureCelsius; }
            set { temperatureCelsius = value; }
        }

        public void FromString(string weatherDataItemString)
        {
            string[] weatherDataItemArray = weatherDataItemString.Split('=');
            DateTime.TryParse(weatherDataItemArray[0], out date);
            float.TryParse(weatherDataItemArray[1].Replace('.', ','), out temperatureCelsius);
        }
    }

    class ConfigDataItem
    {

        String packageName { get; set; } = "Package Name";
        UInt32 testDuration { get; set; } = 1200;  // Variable type? 
        UInt16 startDelay { get; set; } = 0;
        double sampleRate { get; set; } = 1.0;
        char temperatureUnits { get; set; } = 'C';
        DateTime initialDateTime { get; set; } = DateTime.Now; // This is different from arduino code (date and time combined)
        bool resetDateTime { get; set; } = true;

        /*
        package_name = root["pkg_name"] | "Untitled";    // Package name 
  test_duration = root["test_dur"] | 1200;         // 1200 second (20 minute) default test duration
  start_delay = root["start_delay"] | 0;           // 0 second default start delay
  sample_rate = root["smpl_rate"] | 1.0;           // 1 second default sample rate
  temp_units = root["temp_units"] | 'C';           // Celcius is default 
  initial_date = root["init_date"] | "01/01/2000"; // 01/01/2000 initial date (DD/MM/YYYY)
  initial_time = root["init_time"] | "00:00:00";   // 00:00:00 initial time
  reset_date_time = root["reset_date_time"] | 1;   // Time and date are reset by default.  
    */


    }

    public class ArduinoBoard
    {

        //static char eot = '\x03';
        static char eot;

        public ManagementBaseObject mgmtBaseObj { get; set; }
        public SerialPort Port { get; set; }

        public bool genuine { get; set; }
        public bool lca { get; set; }
        String type; // Mega, Uno, Nano, etc. 

        String vid;
        String pid;

        public DATACATEGORY ExpectedResponseType { get; set; } 

        private String _ReceivedData;   // Stores last data received via Serial
        public String ReceivedData
        {
            get { return _ReceivedData; }
        }     

        private String _SendData;
        public String SendData   // Can set SendData to send data  - is this a good design? 
        {
            get { return _SendData; }
            set
            {
                _SendData = value;  // Do this?
                // Send data here
            }

        }

        public String packageName { get; set; }
        public String displayName { get; set; }

        public static String PINGVALUE = "1"+"qlc9KNMKi0mAyT4oKlVky6w7gtHympiyzpdJhE8gj2PPgvO0am5zoSeqkOanME";  // "1" (PING) + 62-character random string from random.org + eot

        private bool _syncNeeded;
        public bool syncNeeded
        {
            get { return _syncNeeded; }
        }

        public ArduinoBoard()
        {
            mgmtBaseObj = null;
            Port = new SerialPort();
            genuine = false;
            lca = false; 
            type = "NULL";
            vid = "NULL";
            pid = "NULL";
            packageName = "NULL";
            displayName = "NULL";
            _ReceivedData = "";
            _SendData = "";
            _syncNeeded = true;
            ExpectedResponseType = DATACATEGORY.PING;  // ??
            eot = '.';
            //OpenConnection(); // ? 
        }

        public ArduinoBoard(String portName)
        {
            mgmtBaseObj = null;
            Port = new SerialPort(portName);
            genuine = false;
            lca = false;
            type = "NULL";
            vid = "NULL";
            pid = "NULL";
            packageName = "NULL";
            displayName = "NULL";
            _ReceivedData = "";
            _SendData = "";
            _syncNeeded = true;
            ExpectedResponseType = DATACATEGORY.PING;  // ??
            eot = '.';
            //OpenConnection(); // ?
        }

        public ArduinoBoard(ManagementBaseObject device)
        {
            lca = false;                            // to be determined...
            mgmtBaseObj = device;
            Port = new SerialPort(SerialInterface.GetPortName(device));

            Console.WriteLine("In ArduinoBoard constructor, Port.PortName = "+Port.PortName);

            vid = SerialInterface.GetVID(device);
            pid = SerialInterface.GetPID(device);
            genuine = false;                        // use a get genuine function 
            type = "NULL";                          // GetArduinoType function 
            packageName = "NULL";                   // to be found
            displayName = "NULL";                   // to be found
            _ReceivedData = "";
            _SendData = "";
            _syncNeeded = true;
            ExpectedResponseType = DATACATEGORY.PING;  // ??
            eot = '.';
            
        }

        ~ArduinoBoard()
        {
            CloseConnection();
        }

        /// <summary>
        /// Raised when new  <see cref="WeatherDataItem"/>s are added
        /// </summary>
        public event EventHandler NewDataReceived;

        public void setArduino(ManagementBaseObject ard)
        {
            mgmtBaseObj = ard;
            Port.PortName = SerialInterface.GetPortName(ard);
            // set other info here using ard
        }

        public void GetInfo()
        {
            // Get all information about Arduino here:
            // Name of lca sensor package
            // Config file
            // Sensors info
            // List of data file filenames
            // ...
        }

        /// <summary>
        /// Opens the connection to an Arduino board
        /// </summary>
        public void OpenConnection()
        {
            // This method is throwing an exception. 1/28/2019.

            Port.ReadTimeout = 1000;  // ???
            Port.WriteTimeout = 1000; // Fixed the problem! yay 

            // SerialPort() defaults: COM1, 9600 baud rate, 8 data bits, 0 parity bits, no parity, 1 stop bit, no handshake
            // Arduino defaults:                            8 data bits, 0 parity bits, no parity, 1 stop bit, no handshake(?). Can be changed. 

            if (!Port.IsOpen)
            {
                //System.Management.Instrumentation.
                //_arduinoPort.

                /*  // This was uncommented previously
                ManagementEventWatcher watcher = new ManagementEventWatcher();
                WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2");
                //watcher.EventArrived += new EventArrivedEventHandler(SerialInterface.USBPnPDeviceChanged);
                watcher.Query = query;
                watcher.Start();
                //ManagementBaseObject a = watcher.WaitForNextEvent();
                */

                Port.DataReceived += arduinoBoard_DataReceived;
                //arduinoBoard.PortName =  ConfigurationSettings.AppSettings["ArduinoPort"];

#if (DEBUG_MODE) 
                Port.PortName = ConfigurationSettings.AppSettings["VirtualPort"]; // com0com virtual serial port
#endif
                Port.Open();
            }
            else
            {
                throw new InvalidOperationException("The Serial Port is already open!");

            }



        }

        /// <summary>
        /// Closes the connection to the Arduino Board.
        /// </summary>
        public void CloseConnection()
        {
            if (Port != null && Port.IsOpen == true)
            {
                Port.Close();
            }
        }

        /// <summary>
        /// Reads weather data from the arduinoBoard serial port
        /// </summary>
        void arduinoBoard_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //string data = ((char)Port.ReadChar()).ToString();//Read 

            if (Port == null)
            {
                Console.WriteLine("The port should only be null here if the ArduinoBoard receives data after the ping times out (in ActivateArduino). {0} {1}", ((SerialPort)sender).ToString(), e.EventType);
                return; 
            }

            Console.WriteLine(Port.ReadBufferSize.ToString());

            String data = null;

            Console.WriteLine("hereeeee");

            try
            {

                //while (Port.BytesToRead >= 0)  // Necessary?
                //{
                    data += Port.ReadExisting();
                //}

                //data = Port.ReadTo(eot.ToString()); //Read until the EOT code   // This crashed it once from timeout
                // Maybe read bytes at a time instead...? 
            }
            catch (Exception exc)
            {
                Console.WriteLine("In arduino's DataReceived handler: " + exc.Message + " " + e.EventType);
                // Other code here????
                // Set state to unresponsive or timeout? 
                return;
            }

            _ReceivedData += data;

            Console.WriteLine("Data received: " + data);

            if (_ReceivedData != "" && _ReceivedData.Last() == eot)
            {
                // The entire data packet has been received. 
                ProcessData(_ReceivedData);  // _ReceivedData should be copied once edited
                _ReceivedData = ""; 
            }

            Console.WriteLine("end hereeee");

            /* // Commented out for now just to test single-character transmissions
            //Split into 'data=temperature' formatted text
            string[] dataArray = data.Split(new string[] {"\x02", "$" }, StringSplitOptions.RemoveEmptyEntries); 
            //Iterate through the splitted data and parse it into weather data items
            //and add them to the list of received weather data.
            foreach (string dataItem in dataArray.ToList())
            {
                WeatherDataItem weatherDataItem = new WeatherDataItem();
                weatherDataItem.FromString(dataItem);
                weatherDataItems.Add(weatherDataItem);
            }
            */



        }

        void ProcessData(String data)
        {
            Console.WriteLine("In ProcessData.");


            if (ExpectedResponseType == DATACATEGORY.PING && _ReceivedData.Equals(PINGVALUE))
            {
                this.lca = true;
                Console.WriteLine("Set lca to true.");
                ExpectedResponseType = DATACATEGORY.NULL; // This assumes only one response type at a time
            }
            else
            {
                // If it can be expecting more than one kind of data at one time, then if it wasn't a ping response, that doesn't imply the ping is never received.
            }


            if (NewDataReceived != null && ExpectedResponseType != DATACATEGORY.PING) // If there is someone waiting for this event to be fired
            {
                NewDataReceived(this, new EventArgs()); //Fire the event, indicating that new WeatherData was added to the list.
            }
        }

        /// <summary>
        /// Sends the command to the Arduino board which triggers the board
        /// to send the weather data it has internally stored.
        /// </summary>
        public void GetWeatherData()
        {
            if (Port.IsOpen)
            {
                try
                {
                    Port.Write("1#");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

            }
            else
            {
                Console.WriteLine("Arduino isn't open.");
                throw new InvalidOperationException("Can't get weather data if the serial Port is closed!");
            }
        }


    }

}
