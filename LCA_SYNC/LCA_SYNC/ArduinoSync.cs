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
    // Provides methods for locating, organizing, and connecting to sensor arrays (LCA Arduinos)
    public class SerialInterface
    {
        // This class is implemented using the singleton design pattern 
        private static SerialInterface singleton;

        // For creating or accessing the single instance of this class 
        public static SerialInterface Create()
        {
            if (singleton == null)
            {
                singleton = new SerialInterface();
            }
            return singleton;
        }

        // Constructor
        private SerialInterface()  
        {
            LCAArduinos = new BindingList<ArduinoBoard>();  // The list of sensor array Arduinos connected to the computer  
            _Arduino = -1;    // No Arduino currently in-use 
            Arduino = null;   // No Arduino currently in-use 

            pnpWatcher = null;
            pnpWatcherHandler = null;
            USBPnPDeviceChanged += SerialInterface_USBPnPDeviceChanged;

            _ActivateArduinoLock = new SpinLock();
        }

        // Destructor 
        ~SerialInterface()
        {
            // This is supposed to stop the PnP watcher, because if it isn't stopped, it can continue to run after the program is closed. 
            if (pnpWatcher != null)
            {
                if (pnpWatcherHandler != null)
                {
                    pnpWatcher.EventArrived -= pnpWatcherHandler;
                }
                pnpWatcher.Stop();    
                pnpWatcher.Dispose();
            }
        }

        // The index in LCAArduinos which stores the currently in-use Arduino  
        private int _Arduino = -1;
        // Get or set the currently in-use Arduino, and invokes an event when it changes  
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
                    _Arduino = LCAArduinos.IndexOf(value); // This should use the overridden Equals function of ArduinoBoard 
                    if (_Arduino == -1)
                    {
                        _Arduino = LCAArduinos.Count;
                        LCAArduinos.Add(value);
                    }
                }
                if (oldArduinoValue != _Arduino)  // The value of _Arduino changed 
                {
                    // The Main class (the GUI) has an event handler to respond to this 
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

        // Used in ActivateArduino so that only one Arduino can be activated at a time 
        private static SpinLock _ActivateArduinoLock;

        // A list of all the LCA Arduinos (sensor array Arduinos) connected to the computer 
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

        // Used when automatically detecting USB PnP (Plug and Play) devices that were plugged in, unplugged, or modified 
        public ManagementEventWatcher pnpWatcher { get; set; }
        public EventArrivedEventHandler pnpWatcherHandler { get; set; }
        
        // Responds to USB PnP (Plug and Play) devices when they are plugged into the computer, unplugged from the computer, or modified 
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

        // Starts the WMI management event watcher for detecting when a new USB PnP (Plug and Play) device is added, removed, or modified.
        public void StartPnPWatcher()
        {
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

        // Raised when a USB PnP device is added/removed/modified 
        public event EventArrivedEventHandler USBPnPDeviceChanged;
        
        // Raised when an ArduinoBoard's copy of the data stored on its Arduino is changed  
        public event ArduinoEventHandler ArduinoDataChanged;

        // Raised when the currently in-use Arduino (the Arduino variable) changes 
        public event ArduinoEventHandler ArduinoChanged;
        
        // For all sensor arrays connected to the computer that aren't in LCAArduinos, this method pings them, adds them to LCAArduinos, and reads their config information 
        public async Task ActivateAllArduinos()
        {
            // This method activates (Ping + Adding to LCAArduino + ReadConfig) all Arduinos that have not been added yet
            foreach (ManagementBaseObject dev in FindArduinos())
            {
                if (!LCAArduinos.ToList().Exists(a => a.Port.PortName == GetPortName(dev)))  // Do nothing for Arduinos already added
                {
                    await ActivateArduino(dev);  
                }
            }
        }

        // Finds all Arduinos connected to the computer. They aren't necessarily our LCA Arduinos (sensor array Arduinos). 
        public List<ManagementBaseObject> FindArduinos() 
        {
            // This method returns a list of Arduino device management objects for all Arduinos connected to the computer
            List<ManagementBaseObject> result = new List<ManagementBaseObject>();

            // Use WMI to get info
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity WHERE ClassGuid=\"{4d36e978-e325-11ce-bfc1-08002be10318}\"");

            // Search all USB PnP (Plug and Play) devices for Arduinos
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

        // Pings an Arduino (to verify it is one of our LCA Arduinos), and if it is, it adds them to LCAArduinos and reads their config information 
        public async Task ActivateArduino(ManagementBaseObject device) 
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
                    //Console.WriteLine("Lock status before TryEnter: _ActivateArduinoLock.IsHeld = {0}, _ActivateArduinoLock.IsHeldByCurrentThread = {1} ", _ActivateArduinoLock.IsHeld, _ActivateArduinoLock.IsHeldByCurrentThread);
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

                    //Console.WriteLine("Lock status after TryEnter: _ActivateArduinoLock.IsHeld = {0}, _ActivateArduinoLock.IsHeldByCurrentThread = {1} ", _ActivateArduinoLock.IsHeld, _ActivateArduinoLock.IsHeldByCurrentThread); 

                    // Pass event from Arduino's event handler to SerialInterface's ArduinoDataChanged event handler:  
                    ard.ArduinoDataChanged += delegate (object sender, ArduinoEventArgs e) { ArduinoDataChanged.Invoke(sender, e); };  

                    if (!ard.Port.IsOpen)  
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
                            // ard will be destroyed when garbage collection occurs. 
                            Console.WriteLine("Ping response not received. The ArduinoBoard instance was set to null.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("\n\nThe ping was successful! \n\n");

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
                            // It already pinged successfully, so it's a real LCA Arduino. Now we want more information about it. 
                            // So ReadConfig gets more info about the Arduino in order to have something to show in the GUI 
                            await ard.ReadConfig();
                            // If there's no response or there's another error, ard.ReadConfig() will throw an exception and success will never be set to true.
                            success = true; 
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message + " Inner exception: " + ex.InnerException);
                            success = false; 
                        }
                    }
                    //Console.WriteLine("About to exit lock. _ActivateArduinoLock.IsHeld = {0}, _ActivateArduinoLock.IsHeldByCurrentThread = {1} ", _ActivateArduinoLock.IsHeld, _ActivateArduinoLock.IsHeldByCurrentThread);
                    if (_GotActivateArduinoLock) { _ActivateArduinoLock.Exit(false); }
                    Console.WriteLine("End of ActivateArduino's task.\n\n");
                    return;
                });
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

        // Gets the port name ("COM1", "COM2", etc.) from the ManagementBaseObject object for the USB PnP device
        public static string GetPortName(ManagementBaseObject dev)
        { 
            return new Regex(@"\((COM\d+)\)").Match(dev["Name"].ToString()).Groups[1].Value;
        }

        // Gets the USB VID (vendor ID) from the ManagementBaseObject object for the USB PnP device 
        public static string GetVID(ManagementBaseObject dev)
        {
            return new Regex(@"(VID_)([0-9a-fA-F]+)").Match(dev["PNPDeviceID"].ToString()).Groups[2].Value.ToLower();
        }

        // Gets the USB PID (product ID) from the ManagementBaseObject object for the USB PnP device 
        public static string GetPID(ManagementBaseObject dev)
        {
            return new Regex(@"(PID_)([0-9a-fA-F]+)").Match(dev["PNPDeviceID"].ToString()).Groups[2].Value.ToLower();
        }

        // Gets what type of Arduino a device with the given USB VID and USB PID is 
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

        // Gets whether or not a device is a genuine Arduino. Sketchy bootleg Arduinos and non-Arduinos return false. 
        public static bool IsGenuineArduino(ManagementBaseObject dev)
        {
            // The 0403 VID was used by older Arduinos which used FTDI 
            // Now, Arduinos use the 2341 VID. 
            string vid = GetVID(dev);
            return vid == "2341" || vid == "1b4f" || (vid == "0403" && dev["Description"].ToString().Contains("Arduino"));
        }

    }

}
