﻿// Not used anymore: 
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
//using Arduino_Serial_Interface;

namespace LCA_SYNC
{


    public class ArduinoBoard
    {
        //private readonly object commLock = new object();
        private SpinLock commLock = new SpinLock();

        static char eot; // End of text
        static char sot; // Start of text 

        static byte eotb;
        static byte sotb;

        public ManagementBaseObject mgmtBaseObj { get; set; }
        public SerialPort Port { get; set; }

        public EventHandler LCAChanged { get; set; }
        private bool _lca;  // Whether the device is a legit LCA arduino (true) or not (false)
        public bool lca
        {
            get { return _lca; }
            set
            {
                if (_lca != value)
                {
                    _lca = value;
                    //LCAChanged?.Invoke(this, new EventArgs());  // Trying without this 
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

        private List<byte> _ReceivedBytes;

        public String packageName { get; set; }
        public String displayName { get; set; }
        private UInt32 testDuration { get; set; }
        private byte startDelay { get; set; }
        private byte sampleRate { get; set; }

        //public static String PINGVALUE = "10" + "qlc9KNMKi0mAyT4oKlVky6w7gtHympiyzpdJhE8gj2PPgvO0am5zoSeqkOanME";  // "1" (PING) + 62-character random string from random.org + eot
        public static String PINGVALUE = "\x02\x01\xF0qlc9KNMKi0mAyT4o\x03";  // Includes sot and eot.
        //('\x01').ToString() + ('\xF0').ToString() + "qlc9KNMKi0mAyT4o";  // "10" (PING) + 62-character random string from random.org + eot
                                                                            // !10qlc9KNMKi0mAyT4o.

        private bool _syncNeeded;
        public bool syncNeeded
        {
            get { return _syncNeeded; }
        }

        private bool _Busy;

        public enum COMMERROR { VALID = 0, NULL, INVALID, UNVALIDATED, TIMEOUT, PORTBUSY, INVALIDINPUT, PORTERROR, OTHER };
        private COMMERROR _ResponseValidity;

        public ArduinoBoard(ManagementBaseObject device)
        {
            Console.WriteLine("Creating a new arduino device (in constructor now)");
            _ExpectedResponseCancellation = new CancellationTokenSource();
            lca = false;                            // to be determined...
            mgmtBaseObj = device;
            Port = new SerialPort(SerialInterface.GetPortName(device));
            Port.DataReceived += arduinoBoard_DataReceived;
            
            //Port.ErrorReceived += delegate { _ResponseValidity = COMMERROR.PORTERROR; _ExpectedResponseCancellation.Cancel(); };  // Untested - Could occur after data received event which could cause issues.
            
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
            eotb = 0x03;
            sotb = 0x02;

            _ResponseValidity = COMMERROR.NULL;
            //_Busy = true;   // Assuming ping happens at start 
            testDuration = 0;
            startDelay = 0;
            sampleRate = 0;

            _ReceivedBytes = new List<byte>();
            
            Port.Encoding = Encoding.GetEncoding(28591);
            Console.WriteLine(Port.Encoding.ToString());
            //Console.WriteLine(Port.NewLine);

        }

        ~ArduinoBoard()
        {
            CloseConnection();
        }

        private void SendData(String data)
        {
            if (Port != null && Port.IsOpen) //&& !_Busy)
            {
                try
                {
                    //_Busy = true;
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
                throw new ArduinoCommunicationException("The serial port is null or not open.");
            }
        }

        private void SendData(byte[] data)
        {
            //data.Prepend((byte)sot).Append((byte)eot); // Add sot and eot to byte array

            if (Port != null && Port.IsOpen)
            {
                try
                {
                    List<byte> byte_list = new List<byte>();
                    byte_list.Add((byte)sot);
                    byte_list.AddRange(data);
                    byte_list.Add((byte)eot);

                    Port.Write(byte_list.ToArray(), 0, byte_list.Count);

                    Console.WriteLine("Just wrote: {0}", BitConverter.ToString(byte_list.ToArray()));

                    //_Busy = true;
                }
                catch (Exception e)
                {
                    throw new ArduinoCommunicationException("Exception in SerialPort.Write.", e);
                }
            }
            else
            {
                //Console.WriteLine("Arduino isn't open.");
                throw new ArduinoCommunicationException("The serial port is null or not open.");
            }
        }

        public void SendData(DATACATEGORY cat, byte subcat, ACTION action, String data = null)
        {
            // Need to check for invalid input in this method!!!! 
            // Return a COMMERROR ?  An maybe change everything over to Exceptions (b/c some errors already begin as exceptions, and they are more detailed in description and don't require a special Response class)?

            switch (cat)
            {
                case DATACATEGORY.NULL:
                    SendData(new byte[] { 0x00, 0x00 });  // cat = 0
                    break;
                case DATACATEGORY.PING:
                    Console.WriteLine("here");
                    SendData(new byte[] { 0x01, 0xF0 });  // cat = 1
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
                    throw new ArduinoCommunicationException("Invalid transmission category.");
                    //Console.WriteLine("Error. Invalid transmission category.");
                    //break;
            }

        }

        public void SendData(DATACATEGORY cat, CONFIGCATEGORY subcat, ACTION action, String data = null)
        {
            try
            {
                SendData(cat, (byte)subcat, action, data);
            }
            catch
            {
                throw;
            }
        }

        public async Task<Response> Ping(double timeoutLength = -1)
        {
            // Use ExpectedResponseType = DATACATEGORY.PING;  here???? 
            try
            {
                return await Communicate(DATACATEGORY.PING, 0, 0, null, timeoutLength);
                //SendData("10");  // 10 is for ping. SendData does the rest. 
            }
            catch
            {
                throw;
                //Console.WriteLine(e.GetType().Name + ": " + e.Message);
            }
        }


        /// <summary>
        /// Raised when new  <see cref="WeatherDataItem"/>s are added
        /// </summary>
        public event EventHandler NewDataReceived;

        public async Task<COMMERROR> RefreshInfo()
        {
            // Get all information about Arduino here:
            // Name of lca sensor package
            // Config file
            // Sensors info
            // List of data file filenames (if lastdatafile# - total#offiles > 0.5*total#offiles, then it would be less data needed to specify which files are present rather than missing (?))
            // ...

            Console.WriteLine("In RefreshInfo.");

            Response results = new Response(null, COMMERROR.INVALID);
            try
            {
                results = await Communicate(DATACATEGORY.CONFIG, CONFIGCATEGORY.ALL, ACTION.READVAR);
            }
            catch (Exception e)
            {
                Console.WriteLine("Communicate error in RefreshInfo: " + e.Message);
            }

            Console.WriteLine("After Communicate in RefreshInfo: results.data=" + results.data + ", results.validity=" + results.validity.ToString());

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

        private async Task<List<byte>> CommunicateRaw(DATACATEGORY cat, CONFIGCATEGORY subcat, ACTION action, String data = null, double timeoutLength = -1.0)
        {
            // Returns the raw response without validating it. 
            // It can still return some errors (in Response.validity), but it can't find fault with the content of the data received, so Response.validity should never be INVALID. 
            bool gotLock = false;
            try
            {
                ////////////commLock.TryEnter(500, ref gotLock);  // Spin for up to 500 ms trying to enter into communication

                if (Port == null || !Port.IsOpen)
                {
                    if (gotLock) { commLock.Exit(); }
                    throw new ArduinoCommunicationException("The serial port is null or not open.");
                }

                // The timeoutLength is the total time in milliseconds between just before the data is sent and right after the response is received. 
                if (timeoutLength < 0.0) // If the user didn't specify a timeout length 
                {
                    timeoutLength = _GetTimeoutLength(cat, subcat, action, data);  // Returns a placeholder value for now
                }

                bool noResponse = true;
                _ResponseValidity = COMMERROR.UNVALIDATED;  // If nothing happens to this, it will remain as UNVALIDATED after the response has been received.
                SendData(cat, subcat, action, data);
                try
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(timeoutLength), _ExpectedResponseCancellation.Token);
                    Console.WriteLine("---No longer waiting for a response!!!");
                }
                catch (TaskCanceledException)
                {
                    noResponse = false;
                }

                _ExpectedResponseCancellation.Dispose();
                _ExpectedResponseCancellation = new CancellationTokenSource(); // Create new one for next time

                if (noResponse)
                {
                    throw new ArduinoCommunicationException("No response.");
                }

                List<byte> resp_data_bytes = new List<byte>();

                resp_data_bytes.AddRange(_ReceivedBytes);

                /*
                foreach (var x in _ReceivedBytes)
                {
                    resp_data_bytes.Add(x);
                }*/

                Console.WriteLine("################# CommunicateRaw. Received: {0}\nOR ALSO THE SAME AS............ : {1}", BytesToString(resp_data_bytes), BitConverter.ToString(resp_data_bytes.ToArray()));

                //object resp_data = resp_data_bytes;
                COMMERROR resp_valid = _ResponseValidity;  // Create local copy before giving up the lock
                if (gotLock) { commLock.Exit(); }
                return resp_data_bytes;  // Should it be COMMERROR.UNVALIDATED rather than _ResponseValidity ?  Probably not 

            }
            catch (Exception e)
            {
                Console.WriteLine("In CommunicateRaw: Exception caught.");
                // Only give up the lock if you actually acquired it
                if (gotLock)
                {
                    commLock.Exit();
                    throw;
                }
                else
                {
                    throw new ArduinoCommunicationException("Exception in CommunicateRaw. ", e);
                    //return new Response("", COMMERROR.PORTBUSY);
                }
            }
            finally
            {
                // Only give up the lock if you actually acquired it
                if (gotLock) commLock.Exit();
                //throw new ArduinoCommunicationException("In CommunicateRaw in finally. You should never see this exception! ");
                //Console.WriteLine("In CommunicateRaw in finally. You should never see this! ");
            }
        }

        public async Task<Response> Communicate(DATACATEGORY cat, CONFIGCATEGORY subcat, ACTION action, String data = null, double timeoutLength = -1.0)
        {
            Console.WriteLine("In Communicate. The current thread is {0}", Thread.CurrentThread.Name);

            List<byte> resp = new List<byte>();
            try
            {
                resp = await CommunicateRaw(cat, subcat, action, data, timeoutLength);
            }
            catch (Exception)
            {
                throw;
            }


            switch (cat)
            {
                case DATACATEGORY.NULL:  // I'm not sure what sorts of repsonses would be in this category, if any, since I don't know anything that would be sent in this category

                    //throw new Exception("Unknown error");
                    //return new Response(resp.data, COMMERROR.OTHER);
                    throw new ArduinoCommunicationException("The response was of the NULL category.");

                case DATACATEGORY.PING:
                    Console.WriteLine("In Communicate, in PING category, resp.data=" + BytesToString(resp) + ". And the correct value is {0}",PINGVALUE);
                    Console.WriteLine("The comparison made to determine correctness: resp.data=" + BitConverter.ToString(resp.ToArray()) + " and {0}", BitConverter.ToString(StringToBytes(PINGVALUE).ToArray()));
                    if (BytesToString(resp).Equals(PINGVALUE))
                    {
                        _ExpectedResponseType = DATACATEGORY.NULL; // This assumes only one response type at a time
                        lca = true;
                        return new Response(null, COMMERROR.VALID);
                    }
                    else
                    {
                        throw new ArduinoCommunicationException("Ping failed.");
                        //return new Response("Ping failed", COMMERROR.INVALID);
                    }

                case DATACATEGORY.CONFIG:
                    if (subcat == CONFIGCATEGORY.ALL)
                    {
                        //List<byte> resp_data = (List<byte>)resp.data;
                        // Check for sot and eot ? When should that be done ?
                        if (resp.Count > 42 || resp.Count < 10) { throw new ArduinoCommunicationException("The response string is of the wrong length."); }
                        if ((byte)resp[1] != 0xF2 && (byte)resp[2] != (byte)((byte)action | (byte)subcat)) { throw new ArduinoCommunicationException("The response string contains the wrong request code."); }

                        List<object> results = new List<object>();

                        int nullterm = resp.IndexOf(0x00);
                        if (nullterm == -1)
                        {
                            throw new ArduinoCommunicationException("Could not parse the Package Name from the response string.");
                        }
                        else
                        {
                            results.Add(resp.GetRange(3, nullterm - 4));  // Don't include the null term. in the package name 
                        }
                        if (_ReceivedData.Length != nullterm + 6) { throw new ArduinoCommunicationException("The response string is of the wrong length."); }

                        results.Add(resp.GetRange(nullterm + 1, 3)); // The test duration 
                        results.Add(resp.GetRange(nullterm + 4, 1)); // The start delay  
                        results.Add(resp.GetRange(nullterm + 5, 1)); // The sample rate  
                        return new Response(results, COMMERROR.VALID);
                    }
                    else
                    {
                        // Not implemented yet 
                        throw new ArduinoCommunicationException("Error, not implemented yet...");
                    }

                default:
                    throw new ArduinoCommunicationException("The response is of an invalid category.");
                    //return new Response(resp.data, COMMERROR.OTHER);
            }

            





        }

        /// <summary>
        /// Opens the connection to an Arduino board
        /// </summary>
        public void OpenConnection()
        {
            // This method is throwing an exception. 1/28/2019.

            Port.ReadTimeout = 2000;  // ???
            Port.WriteTimeout = 1000; // Fixed the problem! yay 

            //Port.NewLine = eot.ToString(); // ? 

            // SerialPort() defaults: COM1, 9600 baud rate, 8 data bits, 0 parity bits, no parity, 1 stop bit, no handshake
            // Arduino defaults:                            8 data bits, 0 parity bits, no parity, 1 stop bit, no handshake(?). Can be changed. 

            if (!Port.IsOpen)
            {
                
                //arduinoBoard.PortName =  ConfigurationSettings.AppSettings["ArduinoPort"];

                // For debugging: Port.PortName = ConfigurationSettings.AppSettings["VirtualPort"]; // com0com virtual serial port

                Port.Open();
                Port.DiscardInBuffer();
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
            
            Console.WriteLine("In arduinoBoard_DataReceived. The current thread is " + Thread.CurrentThread.ManagedThreadId.ToString());

            //string data = ((char)Port.ReadChar()).ToString();//Read 

            SerialPort p = (SerialPort)sender;

            if (Port == null)  // p is not null when Port is???
            {
                Console.WriteLine("The port should only be null here if the ArduinoBoard receives data after the ping times out (in ActivateArduino). {0} {1}", ((SerialPort)sender).ToString(), e.EventType);
                return;
            }

            //Console.WriteLine(p.ReadBufferSize.ToString());

            String data = null;
            List<byte> data2 = new List<byte>();

            //Console.WriteLine("hereeeee");

            try
            {
                //Port.StopBits = 0; 
                Port.Encoding = Encoding.Default;
                Console.WriteLine("Just before read, the encoding is: " + Port.Encoding.ToString() + " Stop bits: " + Port.StopBits);

                // This is not really working well. 
                while (p.BytesToRead > 0)
                {
                    data2.Add((byte)p.ReadByte());

                    //data += p.ReadLine();
                }

                //data = Port.ReadTo(eot.ToString()); // Read until the EOT char. This is working well as of 2/9/2019.
                //data += eot;

                //data.Append(eot);
                // Maybe read bytes at a time instead...? 
            }
            catch (Exception exc)
            {
                // For some reason, this keeps going on after the main loop finishes in the arduino program: (it eventually stops though)
                Console.WriteLine("In arduino's DataReceived handler: " + exc.Message + " " + e.EventType + " _Busy=false");
                // Other code here????
                // Set state to unresponsive or timeout? 
                _Busy = false;  // 
                return;
            }

            _ReceivedData += data;
            _ReceivedBytes.AddRange(data2);


            Console.WriteLine("In arduinoBoard_DataReceived  -----> Just received: {0}, and _ReceivedBytes={1} ", BitConverter.ToString(data2.ToArray()), BitConverter.ToString(_ReceivedBytes.ToArray()));
            
            /*
            if (data != null && data.First() != '#')  // Just so that there isn't a bunch of spam
            {
                Console.WriteLine("Data received:   " + data);
            }
            */

            if (_ReceivedBytes.Count != 0 && _ReceivedBytes.Last() == eotb && _ReceivedBytes.First() == sotb)
            {
                //_ReceivedBytes.Clear();
                //List<byte> received_bytes = new List<byte>(); 
                //foreach (char c in _ReceivedData)
                //{
                //    _ReceivedBytes.Add((byte)c);
                //}




                // Cancel
                // Before using the condition, this gave an error saying that the object didn't exist:
                if (_ExpectedResponseCancellation != null)
                {
                    _ExpectedResponseCancellation?.Cancel();  // Data has been received, so cancel the delay in any thread waiting for this event
                                                              // The entire data packet has been received. 
                                                              //ProcessData(_ReceivedData);  // _ReceivedData should be copied once edited
                                                              //_ReceivedData = ""; 
                }




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

        public string BytesToString(List<byte> bytes)
        {
            return Encoding.Default.GetString(bytes.ToArray());
        }

        public string BytesToString(object bytes)
        {
            return Encoding.GetEncoding(28591).GetString(((List<byte>)bytes).ToArray());
        }

        public string BytesToString(byte[] bytes)
        {
            return Encoding.GetEncoding(28591).GetString(bytes);
        }

        public List<byte> StringToBytes(string str)
        {
            List<byte> bytes = new List<byte>();
            foreach (char ch in str)
            {
                bytes.Add(Convert.ToByte(ch));
            }
            return bytes;
        }


    }



}
