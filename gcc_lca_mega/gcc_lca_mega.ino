// This is the main file for the Arduino Mega prototype. 
// This file contains the setup and loop routines as well as some global variable/macro definitions 

#include <SPI.h>
#include <SD.h>
#include "fileIO.h"
#include "serialSync.h"
#include "timing.h"

#include "Adafruit_MAX31855.h"  // From https://github.com/adafruit/Adafruit-MAX31855-library 
#include "RTClib.h"             // From https://github.com/messmerd/RTClib 
#include "TimerOne.h"           // From https://github.com/PaulStoffregen/TimerOne 

extern Config conf;     // A singleton-ish object for working with the config.txt file
RTC_DS3231 rtc;         // Real-time clock (RTC) object - Uses I2C (Wire, not Wire1)

extern const char* CONFIG_FILE("/config.txt");
extern const char* SENSORS_FILE("/sensors.txt");
extern const char* DEBUG_FILE("/debug.txt");
extern const char* DATALOG_FILE_ROOT("/data");

// Digital thermocouple chip select and data output pins:
#define MAXDO_0   45
#define MAXCS_0   44
#define MAXDO_1   43
#define MAXCS_1   42
#define MAXDO_2   41
#define MAXCS_2   40
#define MAXDO_3   39
#define MAXCS_3   38
#define MAXDO_4   37
#define MAXCS_4   36
#define MAXDO_5   35
#define MAXCS_5   34
#define MAXDO_6   33
#define MAXCS_6   32
#define MAXDO_7   31
#define MAXCS_7   30

#define MAXCLK    28              // Shared clock for all digital thermocouples

extern const int LED_PIN(22);                // Sample period LED
extern const int LED_PIN2(23);               // Multipurpose LED 
extern const int PUSHBUTTON_PIN(2);          // Start/stop test pushbutton
extern const int CD_PIN(13);                 // Card detect pin 
extern const int RTC_INT_PIN(47);            // Real-time clock (RTC) interrupt pin 
// The SD card module uses hardware SPI. It uses the hardware SPI chip select which is pin 53. 

unsigned long samples_elapsed = 0;    // Stores how many samples have been taken since the start of a test 
unsigned long last_sample = 0;        // The last sample before the test ends. Depends on conf.test_duration and conf.sample_period. 
unsigned int data_file_number = 0;    // The current data file number. The current data file is "data#.txt" where "#" is data_file_number. 
char dataFileName[16] = "";   // There's a max length to this! "data9999.txt" is the last file because the SD library uses short 8.3 names for files. 10,000 total data files.   

bool dataReceived = false;    // Is true if the main loop needs to process serial data that has been received 
byte dataIn[150];             // Stores serial data received from the LCA Sync application
int dataInPos;                // The current position within dataIn 
String dataString;            // The string that is written to the data file each sample period 

extern const byte sot(0x02);  // The ASCII start-of-text character. Has special significance when doing serial communication with the LCA Sync application. 
extern const byte eot(0x03);  // The ASCII end-of-text character. Has special significance when doing serial communication with the LCA Sync application.

volatile bool testStarted;          // The state of the Arduino: 0=Ready, 1=Running 
volatile bool samplePeriodReached;  // If true and a test is running, the main loop will take a sample 
volatile bool missedClock = 0;      // Isn't really used for much right now. But in the future it could be useful. 
volatile bool inSerialSafeRegion;   // If this is true, the program will not communicate with the LCA Sync application. This is used to prevent the program being in the middle of communicating when it should be taking a sample. 

boolean led_value = 0;

// Initialize the digital thermocouples: 
Adafruit_MAX31855 digital_thermo_0(MAXCLK, MAXCS_0, MAXDO_0);
Adafruit_MAX31855 digital_thermo_1(MAXCLK, MAXCS_1, MAXDO_1);
Adafruit_MAX31855 digital_thermo_2(MAXCLK, MAXCS_2, MAXDO_2);
Adafruit_MAX31855 digital_thermo_3(MAXCLK, MAXCS_3, MAXDO_3);
Adafruit_MAX31855 digital_thermo_4(MAXCLK, MAXCS_4, MAXDO_4);
Adafruit_MAX31855 digital_thermo_5(MAXCLK, MAXCS_5, MAXDO_5);
Adafruit_MAX31855 digital_thermo_6(MAXCLK, MAXCS_6, MAXDO_6);
Adafruit_MAX31855 digital_thermo_7(MAXCLK, MAXCS_7, MAXDO_7);

void setup() 
{
  pinMode(LED_PIN, OUTPUT);
  pinMode(LED_PIN2, OUTPUT);
  pinMode(PUSHBUTTON_PIN, INPUT); 
  pinMode(CD_PIN, INPUT); 
  pinMode(RTC_INT_PIN, INPUT_PULLUP);

  attachInterrupt(digitalPinToInterrupt(PUSHBUTTON_PIN), pushbuttonPress, RISING);

  samplePeriodReached = false; 
  testStarted = false; 
  missedClock = false;
  inSerialSafeRegion = false;

  dataString.reserve(15+1+115); // I'm not sure about the exact size needed.
  dataString = ""; 

  int i = 0;
  for (i=0; i<150; i++)
  {
    dataIn[i] = 0x00; 
  }
  dataInPos = 0;
  dataReceived = false;

  Serial.begin(9600);
  while (!Serial) {
    ; // wait for serial port to connect. Needed for native USB port only
  }

  // Initialize SD library
  // Note: SD card must be formatted as FAT16 or FAT32 
  while (!SD.begin()) {
    // Initialization failed. 
    //Serial.println(F("fail init. SD"));
    delay(1000);
  }

  // Get filename of new data file: 
  strcat(dataFileName, DATALOG_FILE_ROOT);
  data_file_number = getNextDataFile(); // Get unique number to use for new unique data file name
  strcat(dataFileName, String(data_file_number).c_str());
  strcat(dataFileName, ".txt");  // THERE IS A MAX LENGTH TO THIS, SO THERE'S A MAX NUMBER OF DATA FILES

  // Real-time clock (RTC) initialization 
  if (!rtc.begin()) {
    //Serial.println("Couldn't find RTC");
    while (1);
  }

  if (rtc.lostPower()) {
    //Serial.println("RTC lost power, lets set the time!");
    // following line sets the RTC to the date & time this sketch was compiled
    rtc.adjust(DateTime(F(__DATE__), F(__TIME__)));  // Note: I believe this relies on the computer's language being English and the date format being month, day, year
    // January 21, 2014 at 3am:
    // rtc.adjust(DateTime(2014, 1, 21, 3, 0, 0));
  }

  rtc.writeSqwPinMode(DS3231_SquareWave1kHz); // Important for Timer5 sample interrupts 
  
  conf.read(true);   // Read test configurations from config file, setting the date and time if needed

  Timer1.attachInterrupt( Timer1_ISR ); // Timer1 interrupt routine (see timing.cpp)

  testStarted = false;  // Begin in the "Ready" state 

}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

void loop() 
{
  
  if (testStarted)  // If test is running 
  {
    if (samplePeriodReached) // One sample period has passed - time to take a sample 
    {  
      noInterrupts();
      EIMSK &= ~(1 << INT0);      // Disable pushbutton interrupt 
      //TIMSK1 &= ~(1<<OCIE1A);   // Timer1 - disable the interrupt
      Timer1.stop(); 
      interrupts(); 
      
      // disable serial event interrupt? 
      //noInterrupts();                 // Disable interrupts

      digitalWrite(LED_PIN, led_value); // Blink LED to signify the sample period being reached
      led_value=!led_value;

      DateTime dt = rtc.now(); 
    
      // Read from sensors and put results into a string. Date format is MM/DD/YYYY. 
      dataString = (String)dt.month() + "/" + (String)dt.day() + "/" + (String)dt.year() + " " + (String)dt.hour() + ":" + (String)dt.minute() + ":" + (String)dt.second() + "\t";
      dataString += (String)digital_thermo_0.readCelsius() + "\t"; 
      dataString += (String)digital_thermo_1.readCelsius() + "\t"; 
      dataString += (String)digital_thermo_2.readCelsius() + "\t"; 
      dataString += (String)digital_thermo_3.readCelsius() + "\t"; 
      dataString += (String)digital_thermo_4.readCelsius() + "\t"; 
      dataString += (String)digital_thermo_5.readCelsius() + "\t"; 
      dataString += (String)digital_thermo_6.readCelsius() + "\t"; 
      dataString += (String)digital_thermo_7.readCelsius(); 
    
      // Write the measurements to the data file on the SD card
      printToFile(dataFileName, dataString, true); // print to file

      noInterrupts(); 
    
      samples_elapsed++;
      missedClock = false; 
      inSerialSafeRegion = true; 
      samplePeriodReached = false;

      Timer1.setPeriod((unsigned long)(conf.sample_period*1000000) - SERIAL_COMM_TIME - (unsigned long)(1000000*TCNT5/1024)); // Sample period - serial comm time - sampling routine time
      Timer1.restart(); 

      EIMSK |= (1 << INT0);  // Enable pushbutton interrupt 
      interrupts(); 
    }

    if (dataReceived && inSerialSafeRegion) 
    {
      if (Serial)  // Problem with this line? Maybe remove the Serial condition? 
      {
        ProcessData();  // See serialSync.cpp 
        dataInPos = 0; 
        dataReceived = false;
      }
    }

    if (samples_elapsed >= last_sample)  // Stop the test once it reaches the test duration
    {
      stopTest(); 
    }
    
  }
  else  // Test has not started  
  {
    //interrupts();  // Just in case 
    //EIMSK |= (1 << INT0);  // Enable pushbutton interrupt (just in case)
    
    // Communicate with computer here. Outside of the test here, there are no strict requirements for how much time serial communication can take
    if (dataReceived) 
    {
      if (Serial)  // Problem with this line? Maybe remove the Serial condition? 
      {
        ProcessData();   // See serialSync.cpp 
        dataInPos = 0; 
        dataReceived = false;
      }
    }
  }
  
}


void serialEvent(){   // Note: serialEvent() doesn't work on Arduino Due!!!
  //delay(100); 
  int inByte;
  while (Serial && Serial.available()>0) {
    // get the new byte:
    inByte = Serial.read();
    // add it to the inputString:
    dataIn[dataInPos] = inByte;
    dataInPos++;

    if (dataInPos == 150)
    {
      // Error. dataIn buffer is full.
    }
    // if the incoming character is eot, set a flag so the main loop can
    // do something about it:
    if (inByte == 0x03) {
      dataReceived = true;
      //Serial.clear();
      serial_flush_buffer();  
      break;
    }
  }
}

void serial_flush_buffer()
{
  while (Serial.read() >= 0)
   ; // do nothing
}
