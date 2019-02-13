
// Not used anymore: 
//#define DEBUG_MODE  // Uncomment for Debug mode; Comment for Arduino mode    !!!!

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Configuration;
using System.Configuration.Assemblies;
using System.Management;
using System.Text.RegularExpressions;
using System.ComponentModel;
using Windows.Management;

namespace LCA_SYNC
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
                Response resp = new Response(null,ArduinoBoard.COMMERROR.INVALID);
                
                try
                {
                    Console.WriteLine("Now pinging arduino...");
                    //ard.SendPing(); // Old code. Commenting out to test new code:
                    resp = await ard.Ping(5000); 
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + " Inner exception: " + ex.InnerException?.Message); 
                }

                Console.WriteLine("After Communicate. resp.data="+resp.data+", resp.validity="+resp.validity.ToString());


                /* // Old code. Commenting out to test new code
                if (!ard.lca)
                {
                    //Console.WriteLine("Waiting...");
                    try
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(100), source.Token); //.ConfigureAwait(false);  // If the timespan is too long, it crashes!!!!!!!!!!!1

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Task.Delay's exception: " + ex.GetType().Name + ": " + ex.Message);
                    }
                }
                //Console.WriteLine("After delay.");
                */

                if (resp.validity != ArduinoBoard.COMMERROR.VALID)  //(!ard.lca)  // After 5 ms, if the arduino instance hasn't gotten a ping back telling that it's an LCA board
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
                
                return;
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

    public class ArduinoCommunicationException : Exception
    {
        public ArduinoCommunicationException()
        {
        }

        public ArduinoCommunicationException(string message)
            : base(message)
        {
        }

        public ArduinoCommunicationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    

}
