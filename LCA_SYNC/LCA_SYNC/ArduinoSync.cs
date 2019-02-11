
// Not used anymore: 
//#define DEBUG_MODE  // Uncomment for Debug mode; Comment for Arduino mode    !!!!

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
using System.ComponentModel;
using Windows.Management;

namespace Arduino_Serial_Interface
{

    public enum DATACATEGORY : byte { NULL, PING, CONFIG, OTHER, SENSORS, DATAFILE };  
    public enum CONFIGCATEGORY : byte { ALL, PACKAGE_NAME, TEST_DUR, START_DELAY, SAMPLE_RATE, TEMP_UNITS, INIT_DATE, INIT_TIME, RESET_DT, LANGUAGE };
    public enum ACTION : byte { READFILE=0, DELETEFILE=1, READVAR=32, WRITEVAR=96 };
    
    //public enum ARDUINOTYPE { UNO, MEGA, SERIAL_ADAPTER, };

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

        //public EventHandler LCAArduinos_Changed { get; set; }
        private BindingList<ArduinoBoard> _LCAArduinos;
        
        public BindingList<ArduinoBoard> LCAArduinos
        {
            get { return _LCAArduinos; }
            set
            {
                
                _LCAArduinos = value;
                // Notify/update anything (i.e. UI elements, data transfers) depending on LCAArduinos here:
                //LCAArduinos_Changed?.Invoke(this, new EventArgs());
            }
            
        }
        public ManagementEventWatcher pnpWatcher { get; set; }
        public EventArrivedEventHandler pnpWatcherHandler { get; set; }


        private SerialInterface()  // Default constructor
        {
            //
            Arduino = null;
            LCAArduinos = new BindingList<ArduinoBoard>();
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
        

        public List<ManagementBaseObject> FindArduinos()  // Find all arduinos connected
        {
            // This method returns a list of arduino device management objects for all arduinos connected to the computer
            List<ManagementBaseObject> result = new List<ManagementBaseObject>();

            // Use WMI to get info
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity WHERE ClassGuid=\"{4d36e978-e325-11ce-bfc1-08002be10318}\"");

            // Search all USB PnP (Plug-n-Play) devices for arduinos
            foreach (ManagementObject queryObj in searcher.Get())
            {
                //PNPDeviceID = USB\VID_1A86&PID_7523\5&1A63D808&0&2
                if (null != queryObj["PNPDeviceID"])
                {
                    String port = GetPortName(queryObj);

                    if (IsGenuineArduino(queryObj))  // Genuine arduino 
                    {
                        String ardType = GetArduinoType(GetVID(queryObj), GetPID(queryObj));
                        Console.WriteLine("Arduino detected at port {0}. Arduino type: {1}.", port, ardType);

                        result.Add(queryObj);
                    }
                    else
                    {
                        Console.WriteLine("No arduino at port {0}.", port);
                    }
                    
                }
            }

            if (result.Count == 1)
            {
                Console.WriteLine("There is 1 arduino device found by FindArduinos().\n");
            }
            else
            {
                Console.WriteLine("There are " + result.Count.ToString() + " arduino devices found by FindArduinos().\n");
            }

            return result; 
        }


        public void ActivateArduino(ManagementBaseObject device)  // Verifies that an arduino is LCA, and activates it if it is
        {
            String port = GetPortName(device);

            Console.WriteLine("Currently verifying arduino on port " + port + "..."); 
            
            ArduinoBoard ard = LCAArduinos.ToList().Find(a => a.Port.PortName == port);  // Gives ArduinoBoard if one with that port exists, else null

            if (ard == null) // If an LCA arduino with the port specified doesn't exist in the LCAArduinos list, create an ArduinoBoard 
            {
                ard = new ArduinoBoard(device);
                //ard.ExpectedResponseType = DATACATEGORY.PING;  // Getting ready to ping it  // Set ExpectedResponseType to DATACATEGORY.PING in constructor.
            }

            if (!ard.Port.IsOpen)  // ?
            {
                ard.OpenConnection();
            }

            System.Threading.CancellationTokenSource source = new System.Threading.CancellationTokenSource();  // Can/should this be used? 

            var t = Task.Run(async delegate
            {
                ard.LCAChanged += delegate { Console.WriteLine("Canceling delay."); try { source?.Cancel(); } catch (Exception ee) {Console.WriteLine(ee.Message); } };

                try
                {
                    Console.WriteLine("Now pinging arduino...");
                    ard.SendPing();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                if (!ard.lca)
                {
                    //Console.WriteLine("Waiting...");
                    try
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(100), source.Token)/*.ConfigureAwait(false)*/;  // If the timespan is too long, it crashes!!!!!!!!!!!1

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Task.Delay's exception: " + ex.GetType().Name + ": " + ex.Message);
                    }
                }
                //Console.WriteLine("After delay.");

                if (!ard.lca)  // After 5 ms, if the arduino instance hasn't gotten a ping back telling that it's an LCA board
                {
                    if (LCAArduinos.ToList().Exists(a => a.Port.PortName == port))
                    {
                        // One of the LCA arduinos is not responding!
                        // Set some kind of status variable in arduino instance? (Unresponsive, RunningTest, ComputerMode, etc.)
                        //  
                        Console.WriteLine("Ping response not received, but the device on this port is thought to be a verified LCA arduino.");
                    }
                    else
                    {
                        //ard.Port.Close();
                        //ard.Port = null;  // "Destroy" its serial port (unecessary?)
                        //ard = null;       // "Destroy" the arduino instance, since ping was not received 
                        Console.WriteLine("Ping response not received. The ArduinoBoard instance should be set to null.");
                    }
                    
                }
                else
                {

                    LCAArduinos.Add(ard); // Usually, this would be done only after GetInfo()  (???), but I'm doing it now to test the UI since GetInfo is not implemented yet. 2/9/2019.

                    // Claim position as the "current", in-use arduino here, if it is untaken. 
                    ArduinoBoard.COMMERROR result = await ard.RefreshInfo(); // It pinged successfully, so it's a real LCA arduino. It should get more info about itself and add itself to the 
                    // await a delay? Delay for the length of a timeout.
                    // after delay, check that info has been received (syncNeeded == false). If it hasn't, give error or something. 
                    
                    // In arduino instance, add arduino to LCAArduinos, and also to Arduino if that was claimed earlier. (once data received)
                    // In arduino instance, destroy this thread (if possible) to prevent it from sitting here doing nothing until it times out. 
                }
                
                //return 42;
            });

            //Console.WriteLine("End of ActivateArduino.\n\n");

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

        public static String GetArduinoType(String vid, String pid)
        {
            if (vid.ToLower() == "2341")  // Has the official Arduino USB PID
            {
                switch (pid.ToLower())
                {
                    case "0001":
                        return "UNO";
                    case "0010":
                        return "MEGA";
                    case "003b":
                        return "SERIAL ADAPTER";
                    case "003f":
                        return "MEGA ADK";
                    case "0042":
                        return "MEGA 2560 R3";
                    case "0043":
                        return "UNO R3";
                    case "0044":
                        return "MEGA ADK R3";
                    case "0045":
                        return "SERIAL R3";
                    case "8036":
                        return "LEONARDO";
                    default:
                        return "UNKNOWN";
                }

            }
            else  // Does not have the official Arduino ESB PID. (Could be an old Arduino, fake Arduino, or not an Arduino.)
            {
                return "OTHER/NON-ARDUINO";
            }

        }

        public static bool IsGenuineArduino(ManagementBaseObject dev)
        {
            // The 0403 VID was used by older Arduinos which use FTDI
            // Now, Arduinos use the 2341 VID. 
            String vid = GetVID(dev);
            return vid == "2341" || (vid == "0403" && dev["Description"].ToString().Contains("Arduino"));
        }

        /* // I'm going to remove support for fake arduinos since we won't be using them anyway
        public static bool IsFakeArduino(ManagementBaseObject dev)
        {
            return GetVID(dev) == "1a86" || dev["Description"].ToString().Contains("CH340") || dev["Manufacturer"].ToString().Contains("wch.cn") || dev["Service"].ToString().Contains("CH341SER_A64"); 
        }
        */


    }

    /*
    public class ListWithEvents<T> : List<T>
    {

        public event EventHandler OnAdd;
        public event EventHandler OnRemove;

        public void Add(T item)
        {
            base.Add(item);
            OnAdd?.Invoke(this, null);
        }

        public bool Remove(T item)
        {
            bool result = base.Remove(item);
            OnRemove?.Invoke(this, null);
            return result; 
        }


    }

    public class SyncList<T> : System.ComponentModel.BindingList<T>
    {

        private System.ComponentModel.ISynchronizeInvoke _SyncObject;
        private System.Action<System.ComponentModel.ListChangedEventArgs> _FireEventAction;

        public SyncList() : this(null)
        {
        }

        public SyncList(System.ComponentModel.ISynchronizeInvoke syncObject)
        {

            _SyncObject = syncObject;
            _FireEventAction = FireEvent;
        }

        protected override void OnListChanged(System.ComponentModel.ListChangedEventArgs args)
        {
            if (_SyncObject == null)
            {
                FireEvent(args);
            }
            else
            {
                _SyncObject.Invoke(_FireEventAction, new object[] { args });
            }
        }

        private void FireEvent(System.ComponentModel.ListChangedEventArgs args)
        {
            base.OnListChanged(args);
        }
    }
    */ 


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

        static char eot; // End of text
        static char sot; // Start of text 

        public ManagementBaseObject mgmtBaseObj { get; set; }
        public SerialPort Port { get; set; }

        public EventHandler LCAChanged { get; set; }
        private bool _lca;  // Whether the device is a legit LCA arduino (true) or not (false)
        public bool lca
        {
            get { return _lca; }
            set
            {
                if (LCAChanged != null && _lca != value)
                {
                    _lca = value;
                    LCAChanged?.Invoke(this, new EventArgs());
                }
            }
        }
        String type; // Mega, Uno, Nano, etc. 

        String vid;
        String pid;

        public ArduinoBoard Self  // Is there a better way to do this in Form1? DataSource
        {
            get { return this; }
        }

        private DATACATEGORY _ExpectedResponseType { get; set; }
        private System.Threading.CancellationTokenSource _ExpectedResponseCancellation { get; set; }

        private String _ReceivedData;   // Stores last data received via Serial
        public String ReceivedData
        {
            get { return _ReceivedData; }
        }

        /*  is this a good design? 
        private String _SendData;
        public String SendData   // Can set SendData to send data  - is this a good design? 
        {
            get { return _SendData; }
            set
            {
                _SendData = value;  // Do this?
                // Send data:
                if (Port != null && Port.IsOpen)
                {
                    try
                    {
                        Port.Write(sot + _SendData + eot);
                        _SendData = "";
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.GetType().Name + ": " + e.Message);
                    }
                }
                else
                {
                    Console.WriteLine("Arduino isn't open.");
                    throw new InvalidOperationException("Can't send data if the serial port is closed!");
                }

            }

        }
        */

        public String packageName { get; set; }
        public String displayName { get; set; }
        private UInt32 testDuration { get; set; }
        private byte startDelay { get; set; }
        private byte sampleRate { get; set; }

        //public static String PINGVALUE = "10" + "qlc9KNMKi0mAyT4oKlVky6w7gtHympiyzpdJhE8gj2PPgvO0am5zoSeqkOanME";  // "1" (PING) + 62-character random string from random.org + eot
        public static String PINGVALUE = '\x01' + '\x00' + "qlc9KNMKi0mAyT4o";  // "10" (PING) + 62-character random string from random.org + eot
        // !10qlc9KNMKi0mAyT4o.

        private bool _syncNeeded;
        public bool syncNeeded
        {
            get { return _syncNeeded; }
        }

        private bool _Busy;

        public enum COMMERROR { NULL, VALID, INVALID, UNVALIDATED, TIMEOUT, PORTBUSY, INVALIDINPUT, PORTERROR, OTHER };
        private COMMERROR _ResponseValidity; 

        public ArduinoBoard()
        {
            System.Threading.CancellationTokenSource _ExpectedResponseCancellation = new System.Threading.CancellationTokenSource();
            mgmtBaseObj = null;
            Port = new SerialPort();
            Port.ErrorReceived += delegate { _ResponseValidity = COMMERROR.PORTERROR; _ExpectedResponseCancellation.Cancel(); };
            lca = false; 
            type = "NULL";
            vid = "NULL";
            pid = "NULL";
            packageName = "NULL";
            displayName = "NULL";
            _ReceivedData = "";
            //_SendData = "";
            _syncNeeded = true;
            _ExpectedResponseType = DATACATEGORY.PING;  // Pinging the arduino is the 1st step

            eot = '\x03';
            sot = '\x02';
            _ResponseValidity = COMMERROR.NULL;
            _Busy = true;   // Assuming ping happens at start 
            //OpenConnection(); // ? 

            testDuration = 0;
            startDelay = 0;
            sampleRate = 0; 
        }

        public ArduinoBoard(String portName)
        {
            System.Threading.CancellationTokenSource _ExpectedResponseCancellation = new System.Threading.CancellationTokenSource();
            mgmtBaseObj = null;
            Port = new SerialPort(portName);
            Port.ErrorReceived += delegate { _ResponseValidity = COMMERROR.PORTERROR; _ExpectedResponseCancellation.Cancel(); };
            lca = false;
            type = "NULL";
            vid = "NULL";
            pid = "NULL";
            packageName = "NULL";
            displayName = "NULL";
            _ReceivedData = "";
            //_SendData = "";
            _syncNeeded = true;
            _ExpectedResponseType = DATACATEGORY.PING;  // Pinging the arduino is the 1st step
            eot = '\x03';
            sot = '\x02';
            //OpenConnection(); // ?
            _ResponseValidity = COMMERROR.NULL;
            _Busy = true;   // Assuming ping happens at start 
            testDuration = 0;
            startDelay = 0;
            sampleRate = 0;
        }

        public ArduinoBoard(ManagementBaseObject device)
        {
            System.Threading.CancellationTokenSource _ExpectedResponseCancellation = new System.Threading.CancellationTokenSource();
            lca = false;                            // to be determined...
            mgmtBaseObj = device;
            Port = new SerialPort(SerialInterface.GetPortName(device));
            Port.ErrorReceived += delegate { _ResponseValidity = COMMERROR.PORTERROR; _ExpectedResponseCancellation.Cancel(); };  // Untested - Could occur after data received event which could cause issues.
            // See  https://docs.microsoft.com/en-us/dotnet/api/system.io.ports.serialport.errorreceived?view=netframework-4.7.2 

            //Console.WriteLine("In ArduinoBoard constructor, Port.PortName = "+Port.PortName);

            vid = SerialInterface.GetVID(device);
            pid = SerialInterface.GetPID(device);
            type = SerialInterface.GetArduinoType(vid, pid);
            packageName = "NULL";                   // to be found
            displayName = "NULL";                   // to be found
            _ReceivedData = "";
            //_SendData = "";
            _syncNeeded = true;
            _ExpectedResponseType = DATACATEGORY.PING;  // Pinging the arduino is the 1st step
            eot = '\x03'; //'.'; //'\x03';
            sot = '\x02'; //'!'; // '\x02' 
            _ResponseValidity = COMMERROR.NULL;
            _Busy = true;   // Assuming ping happens at start 
            testDuration = 0;
            startDelay = 0;
            sampleRate = 0;
        }

        ~ArduinoBoard()
        {
            CloseConnection();
        }

        private void SendData(String data)
        {
            if (Port != null && Port.IsOpen && !_Busy)
            {
                try
                {
                    _Busy = true;
                    Port.Write(sot + data + eot);
                }
                catch (Exception e)
                {
                    //Console.WriteLine(e.GetType().Name + ": " + e.Message);
                    throw e;
                }
            }
            else
            {
                //Console.WriteLine("Arduino isn't open.");
                throw new InvalidOperationException("Can't send data if the serial port is closed or busy!");
            }
        }

        private void SendData(byte[] data)
        {
            data.Prepend((byte)sot).Append((byte)eot); // Add sot and eot to byte array

            if (Port != null && Port.IsOpen && !_Busy)
            {
                try
                {
                    _Busy = true;
                    Port.Write(data, 0, data.Length);
                    Console.WriteLine("Just wrote: " + BitConverter.ToString(data));
                }
                catch (Exception e)
                {
                    //Console.WriteLine(e.GetType().Name + ": " + e.Message);
                    throw e;
                }
            }
            else
            {
                //Console.WriteLine("Arduino isn't open.");
                throw new InvalidOperationException("Can't send data if the serial port is closed or busy!");
            }
        }

        public void SendData(DATACATEGORY cat, byte subcat, ACTION action, String data=null)
        {
            // Need to check for invalid input in this method!!!! 
            // Return a COMMERROR ?  An maybe change everything over to Exceptions (b/c some errors already begin as exceptions, and they are more detailed in description and don't require a special Response class)?

            switch (cat)
            {
                case DATACATEGORY.NULL:
                    SendData(new byte[] { 0x00, 0x00 });  // cat = 0
                    break;
                case DATACATEGORY.PING:
                    SendData(new byte[] { 0x01, 0x00 });  // cat = 1
                    break;
                case DATACATEGORY.CONFIG:
                    // cat = 2. The first hex is F just so that the byte isn't 0x02, which is sot. 
                    SendData(new byte[] { 0xF2, (byte)((byte)action | subcat) });  // cat = 2
                    break;
                case DATACATEGORY.OTHER:
                    // Not implemented yet...
                    // Real-time toggle
                    // Stop/stop tests 
                    // Sensor code? 
                    // Rick-roll
                    // etc.
                    // Don't forget the 0x02 and 0x03 bytes can't be used
                    break;
                case DATACATEGORY.SENSORS:
                    // Not implemented yet...
                    break;
                case DATACATEGORY.DATAFILE:
                    // The two bytes are structured as: (data file # - upper 5b)(cat), (data file # - lower 6b)0(action)
                    SendData(new byte[] { (byte)(((subcat & 0x7C0) >> 3) | (byte)cat), (byte)(((subcat & 0x3F) << 2) | (byte)action) });
                    break;
                default:
                    Console.WriteLine("Error. Invalid transmission category.");
                    break;
            }

        }

        public void SendData(DATACATEGORY cat, CONFIGCATEGORY subcat, ACTION action, String data = null)
        {
            SendData(cat, (byte)subcat, action, data);
        }

        public void SendPing()
        {
            // Use ExpectedResponseType = DATACATEGORY.PING;  here???? 
            try
            {
                SendData("10");  // 10 is for ping. SendData does the rest. 
            }
            catch (Exception e)
            {
                Console.WriteLine(e.GetType().Name + ": " + e.Message);
            }
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

        public async Task<COMMERROR> RefreshInfo()
        {
            // Get all information about Arduino here:
            // Name of lca sensor package
            // Config file
            // Sensors info
            // List of data file filenames (if lastdatafile# - total#offiles > 0.5*total#offiles, then it would be less data needed to specify which files are present rather than missing (?))
            // ...

            Console.WriteLine("In RefreshInfo.");

            Response results = await Communicate(DATACATEGORY.CONFIG, CONFIGCATEGORY.ALL, ACTION.READVAR);

            if (results.validity == COMMERROR.VALID)  // Only update values if successful
            {
                List<object> results2;
                results2 = (List<object>)results.data;

                packageName = results2[0].ToString();
                displayName = packageName + " (" + Port.PortName + ")";
                testDuration = (uint)results2[1];
                startDelay = (byte)results2[2];
                sampleRate = (byte)results2[3];
            }

            Console.WriteLine("At end of RefreshInfo.");

            return results.validity;

        }


        double _GetTimeoutLength(DATACATEGORY cat, CONFIGCATEGORY subcat, ACTION action, String data = null)
        {
            return 100.0; // Implement this later. 
        }

        private async Task<Response> CommunicateRaw(DATACATEGORY cat, CONFIGCATEGORY subcat, ACTION action, String data = null)
        {
            // Returns the raw response without validating it. 
            // It can still return some errors (in Response.validity), but it can't find fault with the content of the data received, so Response.validity should never be INVALID. 

            if (_Busy)
            {
                // new Exception()
                return new Response("", COMMERROR.PORTBUSY);
            }

            if (Port == null || !Port.IsOpen)
            {
                return new Response("", COMMERROR.OTHER);
            }

            double timeoutLength = _GetTimeoutLength(cat, subcat, action, data);

            _ExpectedResponseCancellation = new System.Threading.CancellationTokenSource();  // needed
            _ResponseValidity = COMMERROR.UNVALIDATED;  // If nothing happens to this, it will remain as UNVALIDATED after the response has been received.
            SendData(cat, subcat, action, data);
            await Task.Delay(TimeSpan.FromMilliseconds(timeoutLength), _ExpectedResponseCancellation.Token);
            return new Response(_ReceivedData, _ResponseValidity);  // Should it be COMMERROR.UNVALIDATED rather than _ResponseValidity ?  Probably not 
        }

        public async Task<Response> Communicate(DATACATEGORY cat, CONFIGCATEGORY subcat, ACTION action, String data = null)
        {
            Response resp;
            try
            {
                resp = await CommunicateRaw(cat, subcat, action, data);
            }
            catch (Exception)
            {
                throw;
            }

            if (resp.validity != COMMERROR.UNVALIDATED && resp.validity != COMMERROR.VALID)   // Invalid response ()
            {
                return resp;
            }
            else if (resp.validity == COMMERROR.VALID)  // Should not be valid at this point
            {
                return new Response(resp.data, COMMERROR.OTHER);
                // Or throw some unknown error exception
            }
            else  // Unvalidated response (the only good response here), now need to parse it and validate it
            {
                switch (cat)
                {
                    case DATACATEGORY.NULL:  // I'm not sure what sorts of repsonses would be in this category, if any, since I don't know anything that would be sent in this category

                        //throw new Exception("Unknown error");
                        return new Response(resp.data, COMMERROR.OTHER);

                    case DATACATEGORY.PING:
                        if (resp.data.ToString().Equals(sot+PINGVALUE+eot))
                        {
                            _ExpectedResponseType = DATACATEGORY.NULL; // This assumes only one response type at a time
                            lca = true;
                            return new Response("Successful ping", COMMERROR.VALID);
                        }
                        else
                        {
                            return new Response("Ping failed", COMMERROR.INVALID);
                        }

                    case DATACATEGORY.CONFIG:
                        if (subcat == CONFIGCATEGORY.ALL)
                        {
                            // Check for sot and eot ? When should that be done ?
                            if (_ReceivedData.Length > 42 || _ReceivedData.Length < 10) { return new Response("The response string was of the wrong length.", COMMERROR.INVALID); }
                            if (_ReceivedData[1] != '\xF2' && _ReceivedData[2] != (byte)((byte)action | (byte)subcat)) { return new Response("The response string contained the wrong request code.", COMMERROR.INVALID); }

                            List<object> results = new List<object>();

                            int nullterm = _ReceivedData.IndexOf('\x00');
                            if (nullterm == -1)
                            {
                                return new Response("Could not parse the Package Name from the response string.", COMMERROR.INVALID);
                            }
                            else
                            {
                                results.Add(_ReceivedData.Substring(3,nullterm-4));  // Don't include the null term. in the package name 
                            }
                            if (_ReceivedData.Length != nullterm + 6) { return new Response("The response string was of the wrong length.", COMMERROR.INVALID); }

                            results.Add(_ReceivedData.Substring(nullterm + 1, 3)); // The test duration 
                            results.Add(_ReceivedData.Substring(nullterm + 4, 1)); // The start delay  
                            results.Add(_ReceivedData.Substring(nullterm + 5, 1)); // The sample rate  
                            return new Response(results, COMMERROR.VALID);
                        }
                        else
                        {
                            // Not implemented yet 
                            return new Response("Not implemented yet...", COMMERROR.OTHER);
                        }
                    
                    default:
                        return new Response(resp.data, COMMERROR.OTHER);
                }

            }

        }

        /// <summary>
        /// Opens the connection to an Arduino board
        /// </summary>
        public void OpenConnection()
        {
            // This method is throwing an exception. 1/28/2019.

            Port.ReadTimeout = 1500;  // ???
            Port.WriteTimeout = 99; // Fixed the problem! yay 

            Port.NewLine = eot.ToString(); // ? 

            // SerialPort() defaults: COM1, 9600 baud rate, 8 data bits, 0 parity bits, no parity, 1 stop bit, no handshake
            // Arduino defaults:                            8 data bits, 0 parity bits, no parity, 1 stop bit, no handshake(?). Can be changed. 

            if (!Port.IsOpen)
            {
                Port.DataReceived += arduinoBoard_DataReceived;
                //arduinoBoard.PortName =  ConfigurationSettings.AppSettings["ArduinoPort"];

                // For debugging: Port.PortName = ConfigurationSettings.AppSettings["VirtualPort"]; // com0com virtual serial port

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

            SerialPort p = (SerialPort)sender;

            if (Port == null)  // p is not null when Port is???
            {
                Console.WriteLine("The port should only be null here if the ArduinoBoard receives data after the ping times out (in ActivateArduino). {0} {1}", ((SerialPort)sender).ToString(), e.EventType);
                return; 
            }

            //Console.WriteLine(p.ReadBufferSize.ToString());

            String data = null;

            //Console.WriteLine("hereeeee");

            try
            {

                /*
                // This is not really working well. 
                while (p.BytesToRead > 0)
                {
                    data += p.ReadExisting();
                    //data += p.ReadLine();
                }*/
                  
                data = Port.ReadTo(eot.ToString()); // Read until the EOT char. This is working well as of 2/9/2019.
                data += eot;
                // Maybe read bytes at a time instead...? 
            }
            catch (Exception exc)
            {
                // For some reason, this keeps going on after the main loop finishes in the arduino program: (it eventually stops though)
                Console.WriteLine("In arduino's DataReceived handler: " + exc.Message + " " + e.EventType);
                // Other code here????
                // Set state to unresponsive or timeout? 
                return;
            }

            _ReceivedData += data;

            if (data.First() != '#')  // Just so that there isn't a bunch of spam
            {
                Console.WriteLine("Data received:   " + data);
            }
            

            if (_ReceivedData != "" && _ReceivedData.Last() == eot && _ReceivedData.First() == sot)
            {
                // Cancel
                _ExpectedResponseCancellation.Cancel();  // Data has been received, so cancel the delay in any thread waiting for this event
                // The entire data packet has been received. 
                ProcessData(_ReceivedData);  // _ReceivedData should be copied once edited
                _ReceivedData = ""; 
                
            }

            //Console.WriteLine("end hereeee");

        }

        void ProcessData(String data)
        {
            Console.WriteLine("In ProcessData.");

            data = data.TrimStart(sot).TrimEnd(eot);

            if (_ExpectedResponseType == DATACATEGORY.PING && data.Equals(PINGVALUE))
            {
                Console.WriteLine("Setting lca to true.");
                _ExpectedResponseType = DATACATEGORY.NULL; // This assumes only one response type at a time
                lca = true;
                
            }
            else
            {
                // If it can be expecting more than one kind of data at one time, then if it wasn't a ping response, that doesn't imply the ping is never received.
            }


            if (NewDataReceived != null && _ExpectedResponseType != DATACATEGORY.PING) // If there is someone waiting for this event to be fired
            {
                NewDataReceived(this, new EventArgs()); //Fire the event, indicating that new WeatherData was added to the list.
            }
        }

        /// <summary>
        /// Sends the command to the Arduino board which triggers the board
        /// to send the weather data it has internally stored.
        /// </summary>
        


    }

    public struct Response
    {
        public object data { get; }
        public ArduinoBoard.COMMERROR validity { get; }
        public Response(object data, ArduinoBoard.COMMERROR validity)
        {
            this.data = data;
            this.validity = validity;
        }
    }

}
