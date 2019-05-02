using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Management;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace LCA_SYNC
{

    public enum DATACATEGORY : byte { NULL=0x0, PING=0x1, CONFIG=0xF2, OTHER=0xF3, SENSORS=0x4, DATAFILE=0x5, ONEWAY=0x6 };  
    public enum SUBCATEGORY : byte { ALL=0, START_TEST=0, PACKAGE_NAME=1, STOP_TEST=1, TEST_DUR=2, TIME_DATE=2, START_DELAY=3, SAMPLE_PERIOD=4, TEMP_UNITS=5, INIT_DATE=6, INIT_TIME=7, RESET_DT=8, LANGUAGE=9 };
    public enum ACTION : byte { READFILE=0, DELETEFILE=1, READVAR=32, WRITEVAR=96, SENDCOMMAND=96 };
    public enum ONEWAYCATEGORY : byte { TEST_STARTED=0, TEST_ENDED=1, ELAPSED_SAMPLES=2, ERROR_OCCURRED=3 }; // Left-shifted 5 bits and OR'd with DATACATEGORY.ONEWAY 
    // public enum ARDUINOTYPE { UNO, MEGA, SERIAL_ADAPTER, ... };
    // Make an enum for arduino operating states? (Unresponsive, Running, Ready, etc.)

/// <summary>
/// Encapsulates the communication to and from 
/// serial ports, with support for arduino 
/// devices.
/// </summary>
    public class SerialInterface
    {
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
            set
            {
                int oldArduinoValue = _Arduino;
                if (value == null)
                {
                    // Is the order of these two lines correct? 
                    _Arduino = -1;
                    if (oldArduinoValue != -1)
                    {
                        LCAArduinos.RemoveAt(oldArduinoValue);
                    }   
                }
                else
                {
                    _Arduino = LCAArduinos.IndexOf(value); // This should use the overrided Equals function of ArduinoBoard 
                    if (_Arduino == -1)
                    {
                        _Arduino = LCAArduinos.Count;
                        LCAArduinos.Add(value);
                    }
                }
                if (oldArduinoValue != _Arduino)  // The value of _Arduino changed 
                {
                    ArduinoChanged.Invoke(this, new ArduinoEventArgs(oldArduinoValue, "ArduinoChanged"));
                }
                
            }

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

        private BindingList<ArduinoBoard> _LCAArduinos;
        public BindingList<ArduinoBoard> LCAArduinos
        {
            get { return _LCAArduinos; }
            set
            {
                _LCAArduinos = value;
                // Note: BindingList has events that are invoked when things happen to it, and they can be useful for this program's GUI  
            }
            
        }
        public ManagementEventWatcher pnpWatcher { get; set; }
        public EventArrivedEventHandler pnpWatcherHandler { get; set; }
        

        private SerialInterface()  // Default constructor
        {
            LCAArduinos = new BindingList<ArduinoBoard>();
            _Arduino = -1;
            Arduino = null;
            
            pnpWatcher = null;
            pnpWatcherHandler = null;
            USBPnPDeviceChanged += SerialInterface_USBPnPDeviceChanged;

            _ActivateArduinoLock = new SpinLock();
        }

        /// <summary>
        /// USBDeviceChanged event is caught in
        /// order to prevent send/receive errors
        /// and allow the program to connect to
        /// the Arduino automatically.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SerialInterface_USBPnPDeviceChanged(object sender, EventArrivedEventArgs e)
        {
            Console.WriteLine("USB PnP device changed");

            //Console.WriteLine(((ManagementBaseObject)(((EventArrivedEventArgs)e).Context))["TargetInstance"].ToString());

            ManagementBaseObject device = (ManagementBaseObject)e.NewEvent["TargetInstance"];  // Is this right?
            string wclass = e.NewEvent.SystemProperties["__Class"].Value.ToString(); 
            string port = GetPortName(device);

            //Console.WriteLine(wclass);
            //Console.WriteLine(port);
            //Console.WriteLine(Arduino?.Port?.PortName == port);

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
            
            if (Arduino?.Port?.PortName == port)  // If the added/removed device is the LCA Arduino in use 
            {
                Console.WriteLine("The LCA arduino device on port {0} that you were using was {1}.", port, wop);

                if (wop == "created")
                {
                    throw new InvalidOperationException("Cannot create an arduino device that is already created and in use. ");
                }
                else if (wop == "deleted")
                {
                    // Handle errors here. Code to stop from writing to port or start writing, or whatever?
                    // Or are there no errors to handle?  
                    Arduino = null; // This also updates the LCA Arduino list and invokes the event ArduinoChanged 
                } 
            }
            else if (LCAArduinos.ToList().Exists(a => a?.Port?.PortName == port))  // If the added/removed device was an LCA Arduino not in use
            {
                Console.WriteLine("An LCA arduino device on port {0} that you were not using was {1}.", port, wop);

                if (wop == "deleted")
                {
                    // This part is untested 
                    // Want to remove the other ArduinoBoard object from the list without disturbing the currently in-use Arduino
                    int remove_index = LCAArduinos.ToList().FindIndex(a => a?.Port?.PortName == port);
                    int old_arduino_index = _Arduino;
                    // If this condition is true, Arduino's position in the LCAArduinos list will change after removal: 
                    if (remove_index < old_arduino_index && _Arduino != -1)  
                    {
                        LCAArduinos.RemoveAt(remove_index);
                        _Arduino--; 
                    }     
                }
            }
            else  // The added/removed device was not an LCA Arduino (an LCA Arduino device previously known by the program)
            {
                Console.WriteLine("A non-LCA-Arduino device on port {0} was {1}.", port, wop); 

                if (wop == "created" && IsGenuineArduino(device))  // A new arduino device was added!
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(2000));  // Give it a little time before trying to connect. Want the Arduino to go through its setup routine first.  
                    ActivateArduino(device); // Checks if it is an LCA arduino device in the LCAArduinos list (which it shouldn't be), and adds it to LCAArduinos if it is not 
                }
                
            }

            Console.WriteLine("Done with USB PnP device change.");
        }

        public void StartPnPWatcher()
        {
            // This code detects when a new PnP (Plug-n-Play) device is added or removed.
            
            string scope = "root\\CIMV2";
            ManagementScope scopeObject = new ManagementScope(scope);
            WqlEventQuery queryObject = 
                new WqlEventQuery("__InstanceOperationEvent", 
                new TimeSpan(0, 0, 0, 0, 500), 
                "TargetInstance isa \"Win32_PnPEntity\"");

            pnpWatcher = new ManagementEventWatcher();
            pnpWatcher.Scope = scopeObject;
            pnpWatcher.Query = queryObject;
            // The ManagementEventWatcher pnpWatcher is now set up to look for changes to USB PnP devices and invoke the EventArrived event within 500 ms. 

            pnpWatcher.EventArrived += USBPnPDeviceChanged; // Subscribe to the event where a USB PnP device is added/removed/modified 
            pnpWatcher.Options.Timeout = new TimeSpan(0, 0, 5);

            pnpWatcher.Start();  // Start watching for USB PnP device changes 
            // pnpWatcher is stopped when the program closes. 
        }

        /// <summary>
        /// Raised when a USB PnP device is added/removed/modified.  
        /// </summary>
        public event EventArrivedEventHandler USBPnPDeviceChanged;
        
        public event ArduinoEventHandler ArduinoDataChanged;

        public event ArduinoEventHandler ArduinoChanged;
        
        public async Task ActivateAllArduinos()
        {
            // This method activates (Ping + Adding to LCAArduino + RefreshInfo) all arduinos that have not been added yet
            foreach (ManagementBaseObject dev in FindArduinos())
            {
                await ActivateArduino(dev);  // ActivateArduino does nothing for arduinos already added

            }
        }
        
        public List<ManagementBaseObject> FindArduinos()  // Find all arduinos connected
        {
            // This method returns a list of Arduino device management objects for all Arduinos connected to the computer
            List<ManagementBaseObject> result = new List<ManagementBaseObject>();

            // Use WMI to get info
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity WHERE ClassGuid=\"{4d36e978-e325-11ce-bfc1-08002be10318}\"");

            // Search all USB PnP (Plug-n-Play) devices for Arduinos
            foreach (ManagementObject queryObj in searcher.Get())
            {
                // Device IDs look like this: PNPDeviceID = USB\VID_1A86&PID_7523\5&1A63D808&0&2
                if (null != queryObj["PNPDeviceID"])
                {
                    string port = GetPortName(queryObj);  // "COM3", "COM4", etc.  

                    if (IsGenuineArduino(queryObj))  // If it's a genuine arduino, not a fake arduino or other USB PnP device  
                    {
                        string ardType = GetArduinoType(GetVID(queryObj), GetPID(queryObj));  // Mega, Due, Uno, etc. 
                        result.Add(queryObj);
                        Console.WriteLine("Arduino detected at port {0}. Arduino type: {1}.", port, ardType);
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
            string port = GetPortName(device);  // "COM3", "COM4", etc.   
            bool ardExists = LCAArduinos.ToList().Exists(a => a.Port.PortName == port);  // Gives ArduinoBoard if one with that port exists, else null

            if (ardExists) // If an LCA arduino with the port specified doesn't exist in the LCAArduinos list, create an ArduinoBoard 
            {
                return; 
            }

            Console.WriteLine("Currently verifying arduino on port {0}...", port);
            
            try
            {
                // CancellationTokenSource source = new CancellationTokenSource();  // Can/should this be used? 
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

                    ard.ArduinoDataChanged += delegate (object sender, ArduinoEventArgs e) { ArduinoDataChanged.Invoke(sender, e); };  // Pass event from arduinos' event handlers to SerialInterface's ArduinoDataChanged event handler 
                    if (!ard.Port.IsOpen)  // 
                    {
                        ard.OpenConnection();
                    }

                    bool success = false; 
                    try
                    {
                        Console.WriteLine("Now pinging arduino...");
                        await ard.Ping(5000);  // The timeout is 5 seconds for this ping. 
                        // Sending a ping let's us verify that the Arduino is from one of our sensor packages and that this program can communicate with it. 
                        success = true; // If there's no response to the ping or there's another error, ard.Ping() will throw and exception and success will never be set to true.
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Back in ActivateArduino: " + ex.Message + " Inner exception: " + ex.InnerException);
                        success = false; 
                    }

                    //Console.WriteLine("Lock status after ping: _ActivateArduinoLock.IsHeld = {0}, _ActivateArduinoLock.IsHeldByCurrentThread = {1} ", _ActivateArduinoLock.IsHeld, _ActivateArduinoLock.IsHeldByCurrentThread);

                    if (!success)  // If there's some sort of error and a correct ping response is not received before the timeout 
                    {
                        if (LCAArduinos.ToList().Exists(a => a.Port.PortName == port))  // If we've previously connected to this arduino, but now it isn't responding 
                        {
                            // Note: This case should never happen to the ping that happens in this method because this method only executes fully if the arduino is not already in LCAArduinos 
                            // One of the LCA arduinos is not responding!
                            // Set some kind of status variable in arduino instance? (Unresponsive, Running, Ready, etc.)
                            Console.WriteLine("Ping response not received, but the device on this port is thought to be a verified LCA arduino.");
                        }
                        else // The unresponsive arduino is not in LCAArduinos
                        {
                            // Get rid of the ArduinoBoard object ard. 
                            // The arduino is not responding to the ping, so it could be either unresponsive or not an arduino from one of our sensor packages.
                            // In either case, it is not an arduino we want to add to LCAArduinos if we cannot communicate with it. 
                            ard.Port.Close();
                            ard.Port = null;  // "Destroy" its serial port (unecessary?)
                            ard = null;       // "Destroy" the arduino instance, since ping was not received 
                            Console.WriteLine("Ping response not received. The ArduinoBoard instance was set to null.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("\n\nYES!!!! IT PINGED SUCCESSFULLY!!!!!\n\n");

                        if (Arduino == null)  // If there is no LCA arduino currently in use
                        {
                            // Adds the arduino to LCAArduinos if it is not there already, and sets the arduino as the one currently in use. 
                            Arduino = ard; 
                        }
                        else
                        {
                            // Adds the arduino to the list of available LCA arduino devices. It's just not the one we're currently using. 
                            LCAArduinos.Add(ard);  
                        }
                        

                        success = false; 
                        try
                        {
                            // It already pinged successfully, so it's a real LCA arduino. Now we want more information about it. 
                            // So RefreshInfo gets more info about the arduino in order to have something to show in the GUI 
                            await ard.RefreshInfo();
                            // If there's no response or there's another error, ard.RefreshInfo() will throw and exception and success will never be set to true.
                            success = true; 
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message + " Inner exception: " + ex.InnerException);
                            success = false; 
                        }
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
        public List<string> GetLCAArduinoPorts(List<ManagementBaseObject> arduinos)
        {
            // Right now this code just assumes all arduinos are LCA arduinos. 
            // Need to send special message to arduino and receive special response to detemine if it is an LCA Arduino. 

            List<string> lcaArduinoPorts = new List<string>();
            foreach (ManagementBaseObject ard in arduinos)
            {
                lcaArduinoPorts.Add(GetPortName(ard));
            }

            return lcaArduinoPorts; 
        }

        public static string GetPortName(ManagementBaseObject dev)
        {
            // Gets the port name ("COM1", "COM2", etc.) from the ManagementBaseObject for the USB PnP device 
            return new Regex(@"\((COM\d+)\)").Match(dev["Name"].ToString()).Groups[1].Value;
        }

        public static string GetVID(ManagementBaseObject dev)
        {
            // Gets the USB VID (vendor ID) from the ManagementBaseObject for the USB PnP device 
            return new Regex(@"(VID_)([0-9a-fA-F]+)").Match(dev["PNPDeviceID"].ToString()).Groups[2].Value.ToLower();
        }

        public static string GetPID(ManagementBaseObject dev)
        {
            // Gets the USB PID (product ID) from the ManagementBaseObject for the USB PnP device 
            return new Regex(@"(PID_)([0-9a-fA-F]+)").Match(dev["PNPDeviceID"].ToString()).Groups[2].Value.ToLower();
        }

        public static string GetArduinoType(string vid, string pid)
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
                    case "003d":
                        return "DUE";
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
            else  // Does not have the official Arduino USB PID. (Could be an old Arduino, fake Arduino, or not an Arduino.)
            {
                return "OTHER/NON-ARDUINO";
            }

        }

        public static bool IsGenuineArduino(ManagementBaseObject dev)
        {
            // The 0403 VID was used by older Arduinos which use FTDI 
            // Now, Arduinos use the 2341 VID. 
            string vid = GetVID(dev);
            return vid == "2341" || vid == "1b4f" || (vid == "0403" && dev["Description"].ToString().Contains("Arduino"));
        }

    }

}
