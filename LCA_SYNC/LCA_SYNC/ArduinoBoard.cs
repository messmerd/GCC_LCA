﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Management;

namespace LCA_SYNC
{

    public class ArduinoBoard
    {
        // Is there a better way to do this in Form1? DataSource
        public ArduinoBoard Self  
        {
            get { return this; }
        }

        // Stores the Windows Management base object for the USB PnP device (the sensor package's arduino) 
        public ManagementBaseObject mgmtBaseObj { get; set; }
        // For sending and receiving data on the serial port 
        public SerialPort Port { get; set; }

        // Start-of-text byte, a magic number used at the start of every serial data transmission
        static readonly byte sotb = 0x02;
        // End-of-text byte, a magic number used at the end of every serial data transmission
        static readonly byte eotb = 0x03;

        // Used to limit program to only one two-way communication at a time
        private SpinLock commLock = new SpinLock();   

        // This event is raised whenever important arduino variables in this class are changed. ArduinoEventArgs provides 
        public event ArduinoEventHandler ArduinoDataChanged;

        string vid;  // The arduino's USB VID 
        string pid;  // The arduino's USB PID 
        string type; // The arduino type: Mega, Uno, Nano, etc. 


        private CancellationTokenSource _ExpectedResponseCancellation { get; set; }

        private List<byte> _ReceivedTwoWayData;   // Stores last data received via Serial

        private List<byte> _ReceivedBytes;

        private string _PackageName; 
        public string PackageName
        {
            get { return _PackageName; }
            private set
            {
                string oldPackageName = _PackageName; 
                _PackageName = value;
                if (oldPackageName != _PackageName) ArduinoDataChanged.Invoke(this, new ArduinoEventArgs(oldPackageName, "PackageName"));
            }
        }       
        public string DisplayName
        {
            get
            {
                if (Port != null && Port.PortName != "")
                {
                    return PackageName + " (" + Port.PortName + ")";
                }
                else
                {
                    return PackageName + " (ERROR)";
                }
            }
        }
        private byte _StartDelay;
        public byte StartDelay
        {
            get { return _StartDelay; }
            private set
            {
                byte oldStartDelay = _StartDelay; 
                _StartDelay = value;
                if (oldStartDelay != _StartDelay) ArduinoDataChanged.Invoke(this, new ArduinoEventArgs(oldStartDelay, "StartDelay"));
            }
        }
        private uint _TestDuration; 
        public uint TestDuration
        {
            get { return _TestDuration; }
            private set
            {
                uint oldTestDuration = _TestDuration;
                _TestDuration = value;
                if (oldTestDuration != _TestDuration) ArduinoDataChanged.Invoke(this, new ArduinoEventArgs(oldTestDuration, "TestDuration"));
            }
        }
        private float _SamplePeriod; 
        public float SamplePeriod
        {
            get { return _SamplePeriod; }
            private set
            {
                float oldSamplePeriod = _SamplePeriod; 
                _SamplePeriod = value;
                if (oldSamplePeriod != _SamplePeriod) ArduinoDataChanged.Invoke(this, new ArduinoEventArgs(oldSamplePeriod, "SamplePeriod"));
            }
        }
        private bool _TestStarted; 
        public bool TestStarted
        {
            get { return _TestStarted; }
            private set
            {
                bool oldTestStarted = _TestStarted;
                _TestStarted = value;
                if (oldTestStarted != _TestStarted) ArduinoDataChanged.Invoke(this, new ArduinoEventArgs(oldTestStarted, "TestStarted"));
            }
        }

        // Maybe this should be used in ArduinoCommunicationException to describe the error, in addition to the text descriptions: 
        // public enum COMMERROR { VALID = 0, NULL, INVALID, UNVALIDATED, TIMEOUT, PORTBUSY, INVALIDINPUT, PORTERROR, OTHER };

        // The constructor: 
        public ArduinoBoard(ManagementBaseObject device)
        {
            Console.WriteLine("Creating a new arduino device (in constructor now)");
            _ExpectedResponseCancellation = new CancellationTokenSource();
            mgmtBaseObj = device;
            Port = new SerialPort(SerialInterface.GetPortName(device));
            Port.DataReceived += arduinoBoard_DataReceived;

            vid = SerialInterface.GetVID(device);
            pid = SerialInterface.GetPID(device);
            type = SerialInterface.GetArduinoType(vid, pid);

            _PackageName = "NULL";                   // to be found
            _ReceivedTwoWayData = new List<byte>();

            _TestDuration = 0;
            _StartDelay = 0;
            _SamplePeriod = 0;

            _TestStarted = false; 

            _ReceivedBytes = new List<byte>();
            
            Port.Encoding = Encoding.GetEncoding(28591);

        }

        ~ArduinoBoard()
        {
            CloseConnection();
        }


        private void SendData(byte[] data)
        {
            if (Port != null && Port.IsOpen)
            {
                try
                {
                    List<byte> byte_list = new List<byte>();
                    byte_list.Add(sotb);
                    byte_list.AddRange(data);
                    byte_list.Add(eotb);

                    Port.Write(byte_list.ToArray(), 0, byte_list.Count);

                    Console.WriteLine("Just wrote: {0}", BitConverter.ToString(byte_list.ToArray()));
                }
                catch (Exception e)
                {
                    Console.WriteLine("In SendData(byte[]).... Yeeting exception.");
                    throw new ArduinoCommunicationException("Exception in SerialPort.Write.", e);
                }
            }
            else
            {
                Console.WriteLine("In SendData(byte[])... Arduino isn't open. Yeet.");
                throw new ArduinoCommunicationException("The serial port is null or not open.");
            }
        }

        public void SendData(DATACATEGORY cat, byte subcat, ACTION action, object data = null)
        {
            // Need to check for invalid input in this method!!!! 
            // Return a COMMERROR ?  An maybe change everything over to Exceptions (b/c some errors already begin as exceptions, and they are more detailed in description and don't require a special Response class)?

            switch (cat)
            {
                case DATACATEGORY.NULL:  // Should never be used. 0x00 might cause issues when sending?
                    // cat = 0 
                    SendData(new byte[] { (byte)cat, 0x00 });  
                    break;
                case DATACATEGORY.PING:
                    // cat = 1 
                    SendData(new byte[] { (byte)cat, 0xF0 });  
                    break;
                case DATACATEGORY.CONFIG:
                    // cat = 2. The first hex is F just so that the byte isn't 0x02, which is sot (start of text). 
                    if (subcat == (byte)SUBCATEGORY.SAMPLE_PERIOD && data != null)  // Write sample period 
                    {
                        SendData(new byte[] { (byte)cat, (byte)((byte)action | subcat), (byte)(((float)data) * 8.0 + 4.0) });  
                    }
                    else if (subcat == (byte)SUBCATEGORY.TEST_DUR && data != null)  // Write test duration 
                    {
                        char[] hex = BitConverter.ToString(BitConverter.GetBytes((uint)data).Reverse().ToArray()).Replace("-","").ToCharArray(); // Converts test duration to ascii-encoded char array
                        SendData(new byte[] { (byte)cat, (byte)((byte)action | subcat), (byte)hex[2], (byte)hex[3], (byte)hex[4], (byte)hex[5], (byte)hex[6], (byte)hex[7] });  // cat = 2
                    }
                    else if (subcat == (byte)SUBCATEGORY.PACKAGE_NAME && data != null)  // Write package name 
                    {
                        List<byte> bytelist = new List<byte>();
                        bytelist.Add((byte)cat);
                        bytelist.Add((byte)((byte)action | subcat));
                        bytelist.AddRange(StringToBytes((string)data)); 
                        SendData(bytelist.ToArray()); 
                    }
                    else if (subcat == (byte)SUBCATEGORY.START_DELAY && data != null)  // Write start delay 
                    {
                        // Add 4 to the start delay before sending to avoid sending 0x02 or 0x03 which are the special sotb and eotb bytes: 
                        SendData(new byte[] { (byte)cat, (byte)((byte)action | subcat), Convert.ToByte((int)data + 4) }); 
                    }
                    else  // Else use the default case. Used for reading and whatever else 
                    {
                        SendData(new byte[] { (byte)cat, (byte)((byte)action | subcat) }); 
                    }

                    break;
                case DATACATEGORY.OTHER:
                    // cat = 3. The first hex is F just so that the byte isn't 0x03, which is eot (end of text). 
                    if (subcat == (byte)SUBCATEGORY.START_TEST || subcat == (byte)SUBCATEGORY.STOP_TEST)  // Start test or stop test
                    {
                        SendData(new byte[] { (byte)cat, (byte)((byte)ACTION.SENDCOMMAND | subcat) });
                    }
                    else if (subcat == (byte)SUBCATEGORY.TIME_DATE)  // Set or read time/date 
                    {
                        if ((action == ACTION.SENDCOMMAND || action == ACTION.WRITEVAR) && data != null)
                        {
                            // Set the time and date on the arduino's RTC (real-time clock)
                            DateTime dt = (DateTime)data;
                            List<byte> bytelist = new List<byte>();
                            bytelist.AddRange(new byte[] { (byte)cat, (byte)((byte)action | subcat), (byte)(dt.Hour + 4), (byte)(dt.Minute + 4), (byte)(dt.Second + 4), (byte)(dt.Month + 4), (byte)(dt.Day + 4) });
                            bytelist.AddRange(Encoding.ASCII.GetBytes(dt.Year.ToString())); // array of ascii-encoded bytes representing the year 
                            SendData(bytelist.ToArray());
                        }
                        else if (action == ACTION.READVAR)
                        {
                            SendData(new byte[] { (byte)cat, (byte)((byte)action | subcat) });   
                        }

                    }

                    // Features not implemented yet:
                    // Real-time data toggle
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
            }

        }

        public void SendData(DATACATEGORY cat, SUBCATEGORY subcat, ACTION action, object data = null)
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

        public async Task Ping(double timeoutLength = -1)
        {
            try
            {
                // Ping the arduino and await its response 
                await Communicate(DATACATEGORY.PING, 0, 0, null, timeoutLength);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task RefreshInfo()
        {
            // Get essential information about Arduino here
            // List of data file filenames (if lastdatafile# - total#offiles > 0.5*total#offiles, then it would be less data needed to specify which files are present rather than missing (?))
            // ...

            bool success = false; 
            try
            {
                await Communicate(DATACATEGORY.CONFIG, SUBCATEGORY.ALL, ACTION.READVAR);
                success = true; 
            }
            catch (Exception e)
            {
                Console.WriteLine("Communicate error in RefreshInfo: " + e.Message);
                throw e; 
            }
            finally
            {
                //if (success)
                //    ArduinoDataChanged.Invoke(this, new ArduinoEventArgs("RefreshInfo"));
            }

        }

        private double _GetTimeoutLength(DATACATEGORY cat, SUBCATEGORY subcat, ACTION action, object data = null)
        {
            // Different kinds of communication take different amounts of time, so maybe you'd want to adjust the timeout length based on that. 
            // For now, this method is a placeholder 
            return 600.0;   // 600 ms (placeholder)
        }

        private async Task<List<byte>> CommunicateRaw(DATACATEGORY cat, SUBCATEGORY subcat, ACTION action, object data = null, double timeoutLength = -1.0)
        {
            // Returns the raw response without validating it. 
            bool gotLock = false;  // Wait.... this should be a member variable of the class, not a local variable? 
            try
            {
                // Spin for up to 500 ms trying to enter into communication 
                commLock.TryEnter(500, ref gotLock);  
                if (!gotLock)  // Don't enter the critical section if the lock wasn't acquired. 
                {
                    Console.WriteLine("Could not enter critical section. Lock is held by another thread. ");
                    throw new ArduinoCommunicationException("Serial port is busy. Could not enter critical section.");
                } 

                if (Port == null || !Port.IsOpen)
                {
                    if (gotLock) { commLock.Exit(false); }
                    throw new ArduinoCommunicationException("The serial port is null or not open.");
                }

                // The timeoutLength is the total time in milliseconds between just before the data is sent and right after the response is received. 
                if (timeoutLength < 0.0) // If the user didn't specify a timeout length 
                {
                    // Use the recommended timeout length for the type of communication you're using 
                    timeoutLength = _GetTimeoutLength(cat, subcat, action, data);  // Returns a placeholder value for now
                }

                bool noResponse = true;
                
                // New line:
                if (_ExpectedResponseCancellation.IsCancellationRequested) { _ExpectedResponseCancellation = new CancellationTokenSource(); Console.WriteLine("Made new cancellation token."); }

                // try using the token.register thing?

                _ReceivedBytes.Clear();  // This should be ok b/c we have the commLock. What about One-Way comm???????
                _ReceivedTwoWayData.Clear();

                SendData(cat, subcat, action, data);
                try
                {
                    Task.Delay(TimeSpan.FromMilliseconds(timeoutLength), _ExpectedResponseCancellation.Token).Wait();
                    Console.WriteLine("---No longer waiting for a response!!!");
                }
                catch (TaskCanceledException)
                {
                    noResponse = false;
                    Console.WriteLine("Canceled the delay for the response.");
                }
                catch (AggregateException ex)  // Hmmm. There could be a legit bad exception that it lets through
                {
                    AggregateException aggregateException = ex.Flatten();
                    foreach (var inner_ex in ex.InnerExceptions)
                    {
                        if (inner_ex is TaskCanceledException)
                        {
                            noResponse = false;
                        }
                    }
                    if (noResponse)
                    {
                        Console.WriteLine("In CommunicateRaw.... Yeeting AggregateException..");
                        throw ex;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("In CommunicateRaw.... Yeeting unknown exception.");
                    throw ex; 
                }

                // These were both uncommented before:  (2/24/2019)
                //_ExpectedResponseCancellation.Dispose();
                //_ExpectedResponseCancellation = new CancellationTokenSource(); // Create new one for next time

                if (noResponse)
                {
                    throw new ArduinoCommunicationException("No response.");
                }

                List<byte> resp_data_bytes = new List<byte>();

                resp_data_bytes.AddRange(_ReceivedTwoWayData);
                _ReceivedTwoWayData.Clear(); 

                Console.WriteLine("################# CommunicateRaw. Received: {0}\nOR ALSO THE SAME AS............ : {1}", BytesToString(resp_data_bytes), BitConverter.ToString(resp_data_bytes.ToArray()));

                if (gotLock) { commLock.Exit(false); }
                return resp_data_bytes; 

            }
            catch (Exception e)
            {
                Console.WriteLine("In CommunicateRaw: Exception caught: "+e.Message);
                
                // Only give up the lock if you actually acquired it
                if (gotLock)
                {
                    commLock.Exit(false);
                    throw;
                }
                else
                {
                    throw new ArduinoCommunicationException("Exception in CommunicateRaw. ", e);
                }
            }
            finally
            {
                // Only give up the lock if you actually acquired it
                //if (gotLock) commLock.Exit();
                
                //throw new ArduinoCommunicationException("In CommunicateRaw in finally. You should never see this exception! ");
                //Console.WriteLine("In CommunicateRaw in finally. You should never see this! ");
            }
        }

        public async Task Communicate(DATACATEGORY cat, SUBCATEGORY subcat, ACTION action, object data = null, double timeoutLength = -1.0)
        {
            // Throws an ArduinoCommunicationException if unsuccessful 
            Console.WriteLine("In Communicate. The current thread is {0}", Thread.CurrentThread.Name);

            List<byte> resp = new List<byte>();
            try
            {
                resp = await CommunicateRaw(cat, subcat, action, data, timeoutLength);
            }
            catch (Exception)
            {
                Console.WriteLine("In Communicate (at beginning).....Yeeting an exception.  ");
                throw;
            }


            switch (cat)
            {
                case DATACATEGORY.NULL:
                    // I'm not sure what sorts of repsonses would be in this category, if any, since I don't know anything that would be sent in this category
                    throw new ArduinoCommunicationException("The response was of the NULL category.");
                case DATACATEGORY.PING:
                    Console.WriteLine("In Communicate, in PING category, resp.data=" + BytesToString(resp));
                    if (resp.Count == 5 && resp[0] == sotb && resp[1] == 0x01 && resp[2] == 0xF0 && resp[4] == eotb)
                    {
                        if (resp[3] == 0x00 || resp[3] == 0x01)
                        {
                            Console.WriteLine("Setting TestStarted.");
                            TestStarted = Convert.ToBoolean(resp[3]);
                            return; 
                        }
                        else
                        {
                            throw new ArduinoCommunicationException("Ping failed: Response contained an invalid value.");
                        }
                        
                    }
                    else
                    {
                        throw new ArduinoCommunicationException("Ping failed: Invalid response.");
                    }

                case DATACATEGORY.CONFIG:
                    if (action == ACTION.READVAR)
                    { 
                        if (subcat == SUBCATEGORY.ALL)
                        {
                            //List<byte> resp_data = (List<byte>)resp.data;
                            // Check for sot and eot ? When should that be done ?
                            if (resp.Count > 46 || resp.Count < 9) { throw new ArduinoCommunicationException("The response string is of the wrong length."); }
                            if (resp[1] != (byte)DATACATEGORY.CONFIG && resp[2] != (byte)((byte)action | (byte)subcat)) { throw new ArduinoCommunicationException("The response string contains the wrong request code."); }

                            int nullterm = resp.IndexOf(0x00, 3);
                            if (nullterm == -1)
                            {
                                throw new ArduinoCommunicationException("Could not parse the Package Name from the response string.");
                            }
                            else
                            {
                                string str = "";
                                for (int i = 3; i < nullterm; i++)  // Don't include the null term. in the package name 
                                {
                                    str += (char)resp[i];
                                }
                                PackageName = str;
                            }

                            int nullterm2 = resp.IndexOf(0x00, nullterm + 1);
                            if (nullterm2 == -1)
                            {
                                throw new ArduinoCommunicationException("Could not parse the Test Duration from the response string.");
                            }
                            else
                            {
                                // DON't USE DEFAULT ENCODING - CHANGES DEPENDING ON COMPUTER
                                TestDuration = uint.Parse(Encoding.Default.GetString(resp.GetRange(nullterm + 1, nullterm2 - nullterm - 1).ToArray()), System.Globalization.NumberStyles.HexNumber); 
                            }

                            if (resp.Count < nullterm2 + 4) { throw new ArduinoCommunicationException("The response string is of the wrong length."); }

                            // The start delay in seconds as a byte (the 0x4 is to correct for avoiding sending sot or eot over serial. Arduino side adds 0x4 before sending.)
                            StartDelay = (byte)(resp[nullterm2 + 1] - 0x4);
                            // The sample period in seconds as a float
                            SamplePeriod = ((byte)(resp[nullterm2 + 2] - 0x4)) / 8.0f;  

                            Console.WriteLine("Done parsing the config response.");

                            return; 
                        }
                        else
                        {
                            // Not implemented yet 
                            throw new ArduinoCommunicationException("Error, not implemented yet...");
                        }
                    }
                    else if (action == ACTION.WRITEVAR || action == ACTION.SENDCOMMAND)
                    {
                        switch (subcat)
                        {
                            case SUBCATEGORY.PACKAGE_NAME:
                                if (resp.Count == 4 && resp[1] == (byte)cat && resp[2] == (byte)((byte)action | (byte)subcat))
                                {
                                    PackageName = (string)data; // Success. Can update this now. 
                                    return;
                                }  
                                else
                                    throw new ArduinoCommunicationException("The response didn't match the expected value.");
                            case SUBCATEGORY.TEST_DUR:
                                if (resp.Count == 4 && resp[1] == (byte)cat && resp[2] == (byte)((byte)action | (byte)subcat))
                                {
                                    TestDuration = (uint)data; // Success. Can update this now. 
                                    return;
                                }
                                else
                                    throw new ArduinoCommunicationException("The response didn't match the expected value.");
                            case SUBCATEGORY.SAMPLE_PERIOD:
                                if (resp.Count == 4 && resp[1] == (byte)cat && resp[2] == (byte)((byte)action | (byte)subcat))
                                {
                                    SamplePeriod = (float)data; // Success. Can update this now. 
                                    return;
                                }
                                else
                                    throw new ArduinoCommunicationException("The response didn't match the expected value.");
                            case SUBCATEGORY.START_DELAY:
                                if (resp.Count == 4 && resp[1] == (byte)cat && resp[2] == (byte)((byte)action | (byte)subcat))
                                {
                                    StartDelay = Convert.ToByte((int)data); // Success. Can update this now. 
                                    return;
                                }
                                else
                                {
                                    Console.WriteLine("In Communicate------Yeeting an exception.");
                                    throw new ArduinoCommunicationException("The response didn't match the expected value.");
                                }
                                    
                            // Implement the rest of the cases later 
                            default:
                                throw new ArduinoCommunicationException("Error, not implemented yet...");
                        }
                    }
                    else
                    {
                        // invalid action type 
                        throw new ArduinoCommunicationException("Invalid communication action type.");
                    }
                case DATACATEGORY.OTHER:
                    if (action == ACTION.SENDCOMMAND || action == ACTION.WRITEVAR)
                    {
                        switch (subcat)
                        {
                            case SUBCATEGORY.START_TEST:
                            case SUBCATEGORY.STOP_TEST:
                                if (resp.Count == 5 && resp[1] == (byte)cat && resp[2] == (byte)((byte)action | (byte)subcat))
                                {
                                    if (resp[3] == 0x00 || resp[3] == 0x01) 
                                    {
                                        TestStarted = Convert.ToBoolean(resp[3] & 0x01);
                                        return;
                                    }
                                    else
                                        throw new ArduinoCommunicationException("The response didn't match the expected value.");
                                }
                                else
                                    throw new ArduinoCommunicationException("The response didn't match the expected value.");

                                
                            case SUBCATEGORY.TIME_DATE:
                                if (resp.Count == 4 && resp[1] == (byte)cat && resp[2] == (byte)((byte)action | (byte)subcat))
                                {
                                    return;
                                }
                                else
                                    throw new ArduinoCommunicationException("The response didn't match the expected value.");
                            default:
                                throw new ArduinoCommunicationException("Error, not implemented yet...");
                        }
                    }
                    else if (action == ACTION.READVAR)
                    {
                        if (subcat == SUBCATEGORY.TIME_DATE)
                        {
                            if (resp.Count == 11 && resp[1] == (byte)cat && resp[2] == (byte)((byte)action | (byte)subcat))
                            {
                                // Set local copy of Arduino's RTC time here 
                                throw new ArduinoCommunicationException("Error, not implemented yet...");
                            }
                            else
                                throw new ArduinoCommunicationException("The response didn't match the expected value.");
                        }
                        else
                        {
                            throw new ArduinoCommunicationException("Error, not implemented yet...");
                        }
                    }
                    else
                    {
                        // invalid action type 
                        throw new ArduinoCommunicationException("Invalid communication action type.");
                    }
                default:
                    throw new ArduinoCommunicationException("The response is of an invalid category.");
            }

        }

        /// <summary>
        /// Opens the connection to an Arduino board
        /// </summary>
        public void OpenConnection()
        {
            Port.ReadTimeout = 2000;  // ???
            Port.WriteTimeout = 1000; // Fixed the problem! yay 

            // SerialPort() defaults: COM1, 9600 baud rate, 8 data bits, 0 parity bits, no parity, 1 stop bit, no handshake
            // Arduino defaults:                            8 data bits, 0 parity bits, no parity, 1 stop bit, no handshake(?). Can be changed. 

            if (!Port.IsOpen)
            {
                Port.Open();
                Port.DiscardInBuffer(); // ? 
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
        /// Reads data from the arduinoBoard serial port
        /// </summary>
        void arduinoBoard_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            
            //Console.WriteLine("In arduinoBoard_DataReceived. The current thread is " + Thread.CurrentThread.ManagedThreadId.ToString());

            SerialPort p = (SerialPort)sender;

            if (Port == null)  // p is not null when Port is???
            {
                Console.WriteLine("The port should only be null here if the ArduinoBoard receives data after the ping times out (in ActivateArduino). {0} {1}", ((SerialPort)sender).ToString(), e.EventType);
                return;
            }

            string data = null;
            List<byte> data2 = new List<byte>();

            try
            {
                // This is not really working well. 
                while (p.BytesToRead > 0)
                {
                    data2.Add((byte)p.ReadByte());


                    if (data2.Last() == eotb)
                    {
                        break;
                    }
                    //data += p.ReadLine();
                }

                //data = Port.ReadTo(eot.ToString()); // Read until the EOT char. This is working well as of 2/9/2019.
                //data += eot;
            }
            catch (Exception exc)
            {
                // For some reason, this keeps going on after the main loop finishes in the arduino program: (it eventually stops though)
                Console.WriteLine("In arduino's DataReceived handler: " + exc.Message + " " + e.EventType + " _Busy=false");
                // Other code here????
                // Set state to unresponsive or timeout? 
                return;
            }
            finally
            {
                _ReceivedBytes.AddRange(data2);
            }

            Console.WriteLine("In arduinoBoard_DataReceived  -----> Just received: {0}, and _ReceivedBytes={1} ", BitConverter.ToString(data2.ToArray()), BitConverter.ToString(_ReceivedBytes.ToArray()));

            if (_ReceivedBytes.Count != 0 && _ReceivedBytes.Last() == eotb && _ReceivedBytes.First() == sotb)
            {
                // There are two main type of communication between PC and arduino: 2-way and 1-way. 
                // As of 4/26/2019, only two-way communication has been implemented.
                // One-way communication (arduino to PC) would be used for a real-time data feed, 
                //      notifying the PC that a test has been started/stopped, error messages, etc.
                // This Windows program would not ask for the information beforehand so it needs to be handled differently. 
                // One-way communication should be implemented to have its own DATACATEGORY entry. 

                if (_ReceivedBytes.Count > 2 && (_ReceivedBytes[1] & 0x07) == (byte)DATACATEGORY.ONEWAY)  // if (one-way communication is used) 
                {
                    // Process one-way messages here
                    List<byte> oneway_bytes = new List<byte>();
                    oneway_bytes.AddRange(_ReceivedBytes);
                    _ReceivedBytes.Clear();

                    ProcessOneWayData(oneway_bytes);
                }
                else  // Two-way data received 
                {
                    _ReceivedTwoWayData.AddRange(_ReceivedBytes);
                    _ReceivedBytes.Clear();
                    if (_ExpectedResponseCancellation != null)  // In the future, also check that this is two-way communication 
                    {
                        _ExpectedResponseCancellation?.Cancel();  // Data has been received, so cancel the delay in any thread waiting for this event (ActivateArduino)
                                                                  // The entire data packet has been received. 
                                                                  // _ReceivedBytes should be copied
                    }
                }

            }

        }

        void ProcessOneWayData(List<byte> data)
        {
            Console.WriteLine("In ProcessOneWayData.");

            if (data.Count >= 3)
            {
                Console.WriteLine("OneWay category: {0}", (byte)(data[1] >> 3));
                switch ((byte)(data[1] >> 3)) // Only look at upper 5 bits
                {
                    case (byte)ONEWAYCATEGORY.TEST_ENDED:       // Test has ended 
                        TestStarted = false;
                        Console.WriteLine("Just changed TestStarted to false");
                        break;
                    case (byte)ONEWAYCATEGORY.TEST_STARTED:     // Test has started 
                        TestStarted = true;
                        Console.WriteLine("Just changed TestStarted to true");
                        break;
                    default:
                        Console.WriteLine("Unknown OneWay category.");
                        break;
                }
            }
            else
            {
                Console.WriteLine("Wrong data length for OneWay data packet. ");
                // Handle error
            }

        }

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

        // override object.Equals
        public override bool Equals(object obj)
        {
            //       
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            if (Port == null || ((ArduinoBoard)obj).Port == null || Port.PortName != ((ArduinoBoard)obj).Port.PortName)
            {
                return false; 
            }
            else
            {
                return true;
                // ArduinoBoard objects with the same serial port name are treated as equal. 
            }
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            // Using the serial port name
            if (Port == null)
                return 0; 
            return Port.PortName.GetHashCode(); 
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

    public class ArduinoEventArgs : EventArgs
    {
        public ArduinoEventArgs()
        {
            Reason = "";
            Type = "";
        }
        public ArduinoEventArgs(object reason)
        {
            Reason = reason;
            Type = "";
        }
        public ArduinoEventArgs(object reason, object type)
        {
            Reason = reason;
            Type = type;
        }

        public object Reason { get; set; }
        public object Type { get; set; }

    }

    public delegate void ArduinoEventHandler(object sender, ArduinoEventArgs e);

}
