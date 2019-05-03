
#include <SPI.h>
#include <SD.h>
#include "fileIO.h"
#include "Adafruit_MAX31855.h"
#include "RTClib.h" 
#include "TimerOne.h" 
#include "timing.h"

extern Config conf;     // An singleton object for working with the config file and sensor file

bool COMP_MODE;         // Whether in computer mode or not 

RTC_DS3231 rtc;         // Real-time clock (RTC) object

// Digital thermocouple chip select and data output pins:
// Note: pins 50 and 51 appear to not work on this Mega (50, 51, and 52 are used for SPI apparently)
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

#define LED_PIN 22                // Sample period LED 
#define LED_PIN2 23               // CD (card detect) pin? 
#define PUSHBUTTON_PIN 2          // Start test pushbutton
#define CD_PIN 13                 // Card detect pin 
#define RTC_INT_PIN 47            // RTC interrupt pin 
#define chipSelect 53             // For the SD card reader

unsigned long samples_elapsed = 0; 
unsigned long last_sample = 0; 

unsigned int data_file_number = 0; 

bool dataReceived = false;
byte dataIn[150];
int dataInPos;
#define sot 0x02
#define eot 0x03

volatile bool testStarted;
bool samplePeriodReached;
volatile bool missedClock = 0;
volatile bool inSerialSafeRegion;

boolean led_value = 0;

char dataFileName[16] = DATALOG_FILE_ROOT;  // There's a max length to this! "data9999.txt" is the last file because the SD library uses short 8.3 names for files. 10,000 total data files.   

String dataString;

//void initTimer0(double seconds);
bool ProcessData();
void Timer1_ISR();


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

  attachInterrupt(digitalPinToInterrupt(PUSHBUTTON_PIN), pushbuttonPress, RISING);

  // This is only used for analog thermocouples currently, but we are no longer using them: 
  //analogReference(DEFAULT); // 5v on Uno and Mega. Note: Due uses 3.3 v reference which would cause analog thermocouples to not work (given the way things are currently set up)

  samplePeriodReached = false; 
  testStarted = false; 
  missedClock = false;
  inSerialSafeRegion = false;

  dataString.reserve(15+1+115); // I'm not sure about the exact size needed.
  dataString = ""; 

  //dataIn.reserve(200);
  //dataIn = "";
  //dataIn = new byte[150];
  int i = 0;
  for (i=0; i<150; i++)
  {
    dataIn[i] = 0x00; 
  }
  dataInPos = 0;
  dataReceived = false;

  digitalWrite(LED_PIN2, digitalRead(CD_PIN));  // LED is on when SD card is inserted and off when it is not.

  Serial.begin(9600);
  while (!Serial) {
    ; // wait for serial port to connect. Needed for native USB port only
  }

  // Initialize SD library
  // Note: SD card must be formatted as FAT16 or FAT32 
  while (!SD.begin(chipSelect)) {
    //Serial.println(F("fail init. SD"));
    delay(1000);
  }
  //Serial.println("SD init.");
  
  digitalWrite(LED_PIN2, LOW);

  data_file_number = getNextDataFile(); // Get unique number to use for new unique data file name
  strcat(dataFileName, String(data_file_number).c_str());
  strcat(dataFileName, ".txt");  // THERE IS A MAX LENGTH TO THIS, SO THERE'S A MAX NUMBER OF DATA FILES
  //Serial.println("Output data file: " + (String)dataFileName);

  // RTC init
  if (!rtc.begin()) {
    //Serial.println("Couldn't find RTC");
    while (1);
  }

  if (rtc.lostPower()) {
    //Serial.println("RTC lost power, lets set the time!");
    // following line sets the RTC to the date & time this sketch was compiled
    rtc.adjust(DateTime(F(__DATE__), F(__TIME__)));  // Note: I believe this relies on the computer's language being English and the date format being month, day, year
    // This line sets the RTC with an explicit date & time, for example to set
    // January 21, 2014 at 3am you would call:
    // rtc.adjust(DateTime(2014, 1, 21, 3, 0, 0));
  }

  pinMode(RTC_INT_PIN, INPUT_PULLUP);
  rtc.writeSqwPinMode(DS3231_SquareWave1kHz); // For sample interrupts 
  
  conf.read2(true);   // Read from config file, setting the date and time if needed
  //Serial.println();

  // NEED TO CHECK THAT CONFIG STUFF IS VALID - especially the sample rate

  //delay(500); // Just in case things need to settle
  
  digitalWrite(LED_PIN2,LOW);
  Timer1.attachInterrupt( Timer1_ISR ); // attach the service routine here

  testStarted = false;

}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

void loop() 
{
  
  if (testStarted)
  {
    if (samplePeriodReached) // (_timer >= _timer_max)   // One sample period has passed
    {  
      noInterrupts();
      EIMSK &= ~(1 << INT0);  // Disable pushbutton interrupt 
      //TIMSK1 &= ~(1<<OCIE1A);   //timer1 disable the interrupt
      Timer1.stop(); 
      interrupts(); 
      
      // disable serial event interrupt? 
      //noInterrupts();                 //Disable interrupts  (!!!)
    
      //unsigned long _time00 = micros(); 

      digitalWrite(LED_PIN, led_value); // Blink LED to signify the sample period being reached
      led_value=!led_value;

      DateTime dt = rtc.now(); 
    
      // Read from sensors and put results into a string
      //dataString = "#" + (String)samples_elapsed + "\t";
    
      dataString = "#" + (String)dt.day() + (String)dt.month() + (String)(dt.year()-2000) + " " + (String)dt.hour() + ":" + (String)dt.minute() + ":" + (String)dt.second() + "\t";
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

    
      //Serial.println((String)(micros()-_time00));
      //Serial.println((unsigned long)(conf.sample_rate*1000000) - SERIAL_COMM_TIME - (unsigned long)(1000000*TCNT5/1024));
      //Serial.println(conf.sample_rate); 
      //Serial.println(SERIAL_COMM_TIME);
      //Serial.println(TCNT5); 
      //printToFile(dataFileName, datStr, true);

      noInterrupts(); 
    
      samples_elapsed++;
    
      missedClock = false; 
      inSerialSafeRegion = true; 

      samplePeriodReached = false;
      //TIMSK1 |= (1<<OCIE1A);   //timer1 enable the interrupt  // This causes everything to stop working!

      //Serial.print("TIFR1's interrupt flag is ");
      //Serial.println((TIFR1 & (1<<OCF1A))>>1);

      Timer1.setPeriod((unsigned long)(conf.sample_period*1000000) - SERIAL_COMM_TIME - (unsigned long)(1000000*TCNT5/1024)); // Sample period - serial comm time - sampling routine time
      Timer1.restart(); 

      //interrupts();                 //Enable interrupts  (!!!)
      EIMSK |= (1 << INT0);  // Enable pushbutton interrupt 

      interrupts(); 
    }

    if (dataReceived) 
    {
      //digitalWrite(LED_PIN, !digitalRead(LED_PIN));
      //digitalWrite(LED_PIN2,HIGH);
      //noInterrupts();
      if (Serial)  // Problem with this line? Maybe remove the Serial condition? 
      {
        ProcessData();
        //dataIn.clear();  // Not needed if you set dataInPos to 0. Will be overwritten.
        dataInPos = 0; 
        dataReceived = false;
      }
      //interrupts();
    }

    //digitalWrite(LED_PIN2, digitalRead(CD_PIN));

    //while (samples_elapsed >= last_sample) {}; // Ends the test by going into an infinite loop. It works for now, but we should probably change it later. 
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
      //digitalWrite(LED_PIN, !digitalRead(LED_PIN));
      if (Serial)  // Problem with this line? Maybe remove the Serial condition? 
      {
        ProcessData();
        //dataIn.clear();  // Not needed if you set dataInPos to 0. Will be overwritten.
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
