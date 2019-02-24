
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
/// Encapsulates the communication to and from 
/// serial ports, with support for arduino 
/// devices.
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

        private int _Arduino = -1;
        public ArduinoBoard Arduino
        {
            get
            {
                if (_Arduino >= 0)
                {
                    return LCAArduinos?[_Arduino];
                }
                else
                {
                    return null;
                }

            }
            set {; }

            /*  // Causes stack overflow exception for some reason: 
            get {return Arduino; }
            set
            {
                Arduino = value;
                // Notify/update anything (i.e. UI elements, data transfers) depending on Arduino here
            }
            */
        }

        private static SpinLock _ActivateArduinoLock;
        //private static object ;

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
            USBPnPDeviceChanged += SerialInterface_USBPnPDeviceChanged;
            _Arduino = -1;
            _ActivateArduinoLock = new SpinLock();
            //_GotActivateArduinoLock = false; 
            //startPnPWatcher();

        }

        private void SerialInterface_USBPnPDeviceChanged(object sender, EventArrivedEventArgs e)
        {
            Console.WriteLine("USB PnP device changed");

            // Need to find way to distinguish between adding and removing, b/c this event is triggered by both

            //Console.WriteLine("weatherData_USBPnPDeviceChanged event");

            //Console.WriteLine("You have added or removed a USB device. weatherData_USBDeviceChanged.\nEventArgs={0}\nsender={1}",e.ToString(),sender.ToString());
            //Console.WriteLine(((ManagementBaseObject)(((EventArrivedEventArgs)e).Context))["TargetInstance"].ToString());

            ManagementBaseObject device = (ManagementBaseObject)e.NewEvent["TargetInstance"];  // Is this right?
            string wclass = e.NewEvent.SystemProperties["__Class"].Value.ToString(); 
            string port = GetPortName(device);

            Console.WriteLine(wclass);
            Console.WriteLine(port);
            Console.WriteLine(Arduino?.Port?.PortName == port);

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
                default:
                    wop = "error";
                    break;
            }

            if (Arduino?.Port?.PortName == port) //(Arduino != null && device.Equals(Arduino.mgmtBaseObj)) // If the added/removed device is the LCA Arduino in use
            {
                Console.WriteLine("The LCA arduino device on port {0} that you were using was {1}.", port, wop);

                if (wop == "created")
                {
                    throw new InvalidOperationException("Cannot create an arduino device that is already created and in use. ");
                }
                else if (wop == "deleted")
                {
                    // Handle errors here.
                    // Remove device from LCAArduinos and Arduino? Or have some disabled or error state that it places it in? 
                }
                

                // Update LCA Arduino List 
                // Do code to stop from writing to port or start writing, or whatever
            }

            Console.WriteLine("here");
            if (LCAArduinos.ToList().Exists(a => a?.Port?.PortName == port)) //(LCAArduinos.ToList().Exists(a => a.Port.PortName == SerialInterface.GetPortName(device))) // If the added/removed device was an LCA Arduino not in use
            {
                Console.WriteLine("An LCA arduino device on port {0} that you were not using was {1}.", port, wop);
                //Console.WriteLine(serial.Arduino.Port.PortName);
                //MessageBox.Show("An LCA arduino device you were not using was " + wop + ".");

                // Update LCA Arduino List here
            }
            else  // The added/removed device was not an LCA Arduino (an LCA Arduino device previously known by the program)
            {
                Console.WriteLine("A non-LCA-Arduino device on port {0} was {1}.", port, wop); 

                if (wop == "created" && IsGenuineArduino(device))  // A new arduino device was added!
                {
                    
                    Thread.Sleep(TimeSpan.FromMilliseconds(5000));
                    ActivateArduino(device); // Checks if it is an LCA arduino device, and adds it to LCAArduinos if it is
                }
                
            }
            //((ManagementBaseObject)e["TargetInstance"])["Name"]
            //((ManagementEventWatcher)sender).

            Console.WriteLine("done with USB PnP device change.");

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

            //pnpWatcherHandler = new EventArrivedEventHandler(USBPnPDeviceChanged);

            pnpWatcher.EventArrived += USBPnPDeviceChanged; // pnpWatcherHandler;

            pnpWatcher.Options.Timeout = new TimeSpan(0, 0, 5);
            //new EventArrivedEventHandler(USBDeviceChanged);
            pnpWatcher.Start();

        }

        /// <summary>
        /// Raised when a USB device is connected
        /// or disconnected from the computer. 
        /// </summary>
        public event EventArrivedEventHandler USBPnPDeviceChanged;
        
        public event ArduinoEventHandler ArduinoDataChanged;
        
        /// <summary>
        /// Gets a list of <see cref="WeatherDataItem"/> which was
        /// previsously retrieved from an Arduino Board.
        /// </summary>
        /*
        internal List<WeatherDataItem> WeatherDataItems
        {
            get { return weatherDataItems; }
        }*/




        public void PrintWow()
        {
            Console.WriteLine("Wow");
        }


        

        public async Task LocateLCADevices()
        {
            foreach (ManagementBaseObject dev in FindArduinos())
            {
                await ActivateArduino(dev);

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


        public async Task ActivateArduino(ManagementBaseObject device)  // Verifies that an arduino is LCA, and activates it if it is
        {
            String port = GetPortName(device);
            bool ardExists = LCAArduinos.ToList().Exists(a => a.Port.PortName == port);  // Gives ArduinoBoard if one with that port exists, else null

            if (ardExists) // If an LCA arduino with the port specified doesn't exist in the LCAArduinos list, create an ArduinoBoard 
            {
                return; 
            }

            Console.WriteLine("Currently verifying arduino on port " + port + "...");
            
            try
            {
                

                //CancellationTokenSource source = new CancellationTokenSource();  // Can/should this be used? 

                // Now awaiting this:  (2/24/2019)
                await Task.Run(async delegate
                {
                    Console.WriteLine("Lock status before TryEnter: _ActivateArduinoLock.IsHeld = {0}, _ActivateArduinoLock.IsHeldByCurrentThread = {1} ", _ActivateArduinoLock.IsHeld, _ActivateArduinoLock.IsHeldByCurrentThread);
                    bool _GotActivateArduinoLock = false;
                    try
                    {
                        _ActivateArduinoLock.TryEnter(1, ref _GotActivateArduinoLock);  // Only want one at a time. Else more than one ArduinoBoard instance could exist and be communicating with the same arduino during the activation stage. 
                    }
                    catch (Exception ex)
                    {
                        if (_GotActivateArduinoLock) { _ActivateArduinoLock.Exit(false); }
                        Console.WriteLine("ActivateArduino TryEnter exception: " + ex.Message + " Inner exception: " + ex.InnerException);
                        return;
                    }

                    if (!_GotActivateArduinoLock) { Console.WriteLine("Could not enter critical section. Lock is held by another thread. "); return; } // Don't enter the critical section if the lock wasn't acquired. 

                    ArduinoBoard ard = new ArduinoBoard(device);
                    
                    Console.WriteLine("Created a new arduino device. _GotActivateArduinoLock = {0}", _GotActivateArduinoLock);
                    Console.WriteLine("Lock status after TryEnter: _ActivateArduinoLock.IsHeld = {0}, _ActivateArduinoLock.IsHeldByCurrentThread = {1} ", _ActivateArduinoLock.IsHeld, _ActivateArduinoLock.IsHeldByCurrentThread); 
                    //ArduinoBoard ard = new ArduinoBoard(device);
                    //ard.ExpectedResponseType = DATACATEGORY.PING;  // Getting ready to ping it  // Set ExpectedResponseType to DATACATEGORY.PING in constructor.
                    ard.ArduinoDataChanged += delegate (object sender, ArduinoEventArgs e) { ArduinoDataChanged.Invoke(sender, e); };  // Pass event from arduinos' event handlers to SerialInterface's ArduinoDataChanged event handler 
                    if (!ard.Port.IsOpen)  // ?
                    {
                        ard.OpenConnection();
                    }
                    //ard.LCAChanged += delegate { Console.WriteLine("Canceling delay."); try { source?.Cancel(); } catch (Exception ee) { Console.WriteLine(ee.Message); } };
                    Response resp = new Response(null, ArduinoBoard.COMMERROR.NULL);

                    try
                    {
                        Console.WriteLine("Now pinging arduino...");
                        //ard.SendPing(); // Old code. Commenting out to test new code:
                        resp = await ard.Ping(5000);
                        
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Back in ActivateArduino: "+ex.Message + " Inner exception: " + ex.InnerException);
                    }

                    //Console.WriteLine("After Communicate. resp.data=" + resp.data + ", resp.validity=" + resp.validity.ToString());
                    //Console.WriteLine("Lock status after ping: _ActivateArduinoLock.IsHeld = {0}, _ActivateArduinoLock.IsHeldByCurrentThread = {1} ", _ActivateArduinoLock.IsHeld, _ActivateArduinoLock.IsHeldByCurrentThread);

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
                            ard.Port.Close();
                            ard.Port = null;  // "Destroy" its serial port (unecessary?)
                            ard = null;       // "Destroy" the arduino instance, since ping was not received 
                            Console.WriteLine("Ping response not received. The ArduinoBoard instance was set to null.");
                        }

                    }
                    else
                    {

                        Console.WriteLine("\n\nYES!!!! IT PINGED SUCCESSFULLY!!!!!\n\n");


                        LCAArduinos.Add(ard); // Usually, this would be done only after GetInfo()  (???), but I'm doing it now to test the UI since GetInfo is not implemented yet. 2/9/2019.

                        if (_Arduino == -1)  // If no arduino is in use
                        {
                            _Arduino = LCAArduinos.Count - 1; // Add this arduino as the one in use
                        }

                        // Claim position as the "current", in-use arduino here, if it is untaken. 

                        //Console.WriteLine("");

                        try
                        {
                            ArduinoBoard.COMMERROR result = await ard.RefreshInfo(); // It pinged successfully, so it's a real LCA arduino. It should get more info about itself and add itself to the 
                                                                                        // await a delay? Delay for the length of a timeout.
                                                                                        // after delay, check that info has been received (syncNeeded == false). If it hasn't, give error or something. 
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message + " Inner exception: " + ex.InnerException);
                        }


                        // In arduino instance, add arduino to LCAArduinos, and also to Arduino if that was claimed earlier. (once data received)
                        // In arduino instance, destroy this thread (if possible) to prevent it from sitting here doing nothing until it times out. 
                    }
                    Console.WriteLine("About to exit lock. _ActivateArduinoLock.IsHeld = {0}, _ActivateArduinoLock.IsHeldByCurrentThread = {1} ", _ActivateArduinoLock.IsHeld, _ActivateArduinoLock.IsHeldByCurrentThread);
                    if (_GotActivateArduinoLock) { _ActivateArduinoLock.Exit(false); }
                    Console.WriteLine("End of Task.\n\n\n");
                    return;
                });


                


                //Console.WriteLine("End of ActivateArduino.\n\n");


            }
            catch (Exception e)
            {
                Console.WriteLine("In ActivateArduino, caught: {0}", e.Message);
                
            }
            finally
            {
                //if (_GotActivateArduinoLock) { _ActivateArduinoLock.Exit(); }
            }


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
            // Maybe return an enum instead? And then make another funtion for getting the string from the enum?
            if (vid.ToLower() == "2341" || vid.ToLower() == "1b4f")  // Has the official Arduino LLC USB PID or Sparkfun USB PID (?)
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
            return vid == "2341" || vid == "1b4f" || (vid == "0403" && dev["Description"].ToString().Contains("Arduino"));
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

    
    

}
