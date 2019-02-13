// To do: Arduino can only connect to computer when in main loop and not at the end of a test. 
// Change it so that it can connect any time. May need to disable the serialEvent interrupt 
// during SD card operations or reading sensors. How would this effect communication with
// the computer? 

// WARNING: Should stop using Timer0 b/c apparently millis() uses it.   
/*
// For the Due, maybe this would work:
void serialEventRun(void)  // Must use this name
{
  //if (Serial.available()) serialEvent();
  if (SerialUSB.available()) serialEvent();
  if (Serial1.available()) serialEvent1();
  if (Serial2.available()) serialEvent2();
  if (Serial3.available()) serialEvent3();
}
// See: https://forum.arduino.cc/index.php?topic=205779.0 
*/

#include <SPI.h>
#include <SD.h>
#include <Wire.h>
#include "fileIO.h"
#include "Adafruit_MAX31855.h"
#include "RTClib.h"  
#include "TimerOne.h"

extern Config conf;     // An singleton object for working with the config file and sensor file

bool COMP_MODE;         // Whether in computer mode or not 

RTC_DS3231 rtc;         // Real-time clock (RTC) object

//char daysOfTheWeek[7][12] = {"Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"};

// Digital thermocouple chip select and data output pins:
// Note: pins 50 and 51 appear to not work on this Mega (50, 51, and 52 are used for SPI apparently)
#define MAXDO_0   48
#define MAXCS_0   49
#define MAXDO_1   46
#define MAXCS_1   47
#define MAXDO_2   44
#define MAXCS_2   45
#define MAXDO_3   42
#define MAXCS_3   43
#define MAXDO_4   40
#define MAXCS_4   41
#define MAXDO_5   38
#define MAXCS_5   39
#define MAXDO_6   36
#define MAXCS_6   37
#define MAXDO_7   34
#define MAXCS_7   35

#define MAXCLK    33              // Shared clock for all digital thermocouples

#define LED_PIN 12                // Sample period LED 
#define LED_PIN2 10               // CD (card detect) pin? 
#define PUSHBUTTON_PIN 11         // Start test pushbutton
#define CD_PIN 13                 // Card detect pin 
#define chipSelect 53             // For the SD card reader

#define NUMSAMPLES 5              // This is a random sample amount

unsigned int _timer = 0;
unsigned int _timer_max = 1000; 

unsigned long samples_elapsed = 0; 
unsigned long last_sample = 0; 

unsigned int data_file_number = 0; 

bool dataReceived = false;
String dataIn;
char sot, eot; 

boolean led_value = 0;

char dataFileName[16] = DATALOG_FILE_ROOT;  // There's a max length to this! "data9999.txt" is the last file because the SD library uses short 8.3 names for files. 10,000 total data files.   

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

void setup() {

  pinMode(LED_PIN, OUTPUT);
  pinMode(LED_PIN2, OUTPUT);
  pinMode(PUSHBUTTON_PIN, INPUT); 
  pinMode(CD_PIN, INPUT); 

  // This is only used for analog thermocouples currently, but we are no longer using them: 
  analogReference(DEFAULT); // 5v on Uno and Mega. Note: Due uses 3.3 v reference which would cause analog thermocouples to not work (given the way things are currently set up)

  dataIn.reserve(200);
  dataIn = "";
  dataReceived = false;
  sot = 2; //'\x02'; //'!';
  eot = 3; //'\x03'; //'.';

  digitalWrite(LED_PIN2, digitalRead(CD_PIN));  // LED is on when SD card is inserted and off when it is not.

  Serial.begin(9600);
  while (!Serial) {
    ; // wait for serial port to connect. Needed for native USB port only
  }

  // Initialize SD library
  // Note: SD card must be formatted as FAT16 or FAT32 
  while (!SD.begin(chipSelect)) {
    Serial.println(F("fail init. SD"));
    delay(1000);
  }
  Serial.println("SD init.");
  
  digitalWrite(LED_PIN2, digitalRead(CD_PIN));

  data_file_number = getNextDataFile(); // Get unique number to use for new unique data file name
  strcat(dataFileName, String(data_file_number).c_str());
  strcat(dataFileName, ".txt");  // THERE IS A MAX LENGTH TO THIS, SO THERE'S A MAX NUMBER OF DATA FILES
  Serial.println("Output data file: " + (String)dataFileName);

  // RTC init
  if (! rtc.begin()) {
    Serial.println("Couldn't find RTC");
    while (1);
  }

  conf.read(true);   // Read from config file, setting the date and time if needed
  Serial.println();

  // NEED TO CHECK THAT CONFIG STUFF IS VALID - especially the sample rate

  delay(500); // Just in case things need to settle
  
  last_sample = conf.test_duration/conf.sample_rate; // The last sample before the test ends. 

  digitalWrite(LED_PIN2,LOW);

  Serial.println("Press pushbutton to start test.");
  while (!digitalRead(PUSHBUTTON_PIN)) {; };  // Start test when pushbutton is pressed. 


  delay(conf.start_delay*1000); // Start delay
  
  //initTimer0(conf.sample_rate); // Configure internal Timer 0 and _timer_max

  _timer_max = 1000.0*conf.sample_rate; 
  Timer1.initialize(1000); // set a timer of length 1000 microseconds (or 1 ms)
  Timer1.attachInterrupt( Timer1_ISR ); // attach the service routine here

  _timer = _timer_max; // The first sample will start right away
}

void loop() {
  if (_timer >= _timer_max) {  // One sample period has passed
    //noInterrupts();                 //Disable interrupts  (!!!)
    digitalWrite(LED_PIN, led_value); // Blink LED to signify the sample period being reached
    led_value=!led_value;
    _timer = 0;

    
    // Read from sensors and put results into a string
    String dataString = "#" + (String)samples_elapsed + "\t";
    dataString += (String)digital_thermo_0.readCelsius() + "\t"; 
    dataString += (String)digital_thermo_1.readCelsius() + "\t"; 
    dataString += (String)digital_thermo_2.readCelsius() + "\t"; 
    dataString += (String)digital_thermo_3.readCelsius() + "\t"; 
    dataString += (String)digital_thermo_4.readCelsius() + "\t"; 
    dataString += (String)digital_thermo_5.readCelsius() + "\t"; 
    dataString += (String)digital_thermo_6.readCelsius() + "\t"; 
    dataString += (String)digital_thermo_7.readCelsius(); 

    
    
    // Write the measurements to the data file on the SD card
    //printToFile(dataFileName, dataString, true); // print to file

    // The time on the ChronoDot real-time clock (RTC) wasn't working, so RTC timestamps are disabled for now. 
    /*
    DateTime right_now = rtc.now(); 
    Serial.println("Year: " + String(right_now.year()) + ". Month: " + String(right_now.month()) + ". Day: " + String(right_now.day()));
    Serial.println("Hour: " + String(right_now.hour()) + ". Minute: " + String(right_now.minute()) + ". Second: " + String(right_now.second()));
    */
    samples_elapsed++;
    //interrupts();                 //Enable interrupts  (!!!)
  }

  if (dataReceived) {
    //digitalWrite(LED_PIN2,HIGH);
    //noInterrupts();
    if (Serial)  // Problem with this line? Maybe remove the Serial condition? 
    {
      if (ProcessData())
      {
        dataIn = "";
        dataReceived = false;
      }
      dataIn = "";
      dataReceived = false;
    }
    //interrupts();
  }

  //digitalWrite(LED_PIN2, digitalRead(CD_PIN));

  while (samples_elapsed >= last_sample) {}; // Ends the test by going into an infinite loop. It works for now, but we should probably change it later. 
  
}



// Increments _timer every millisecond. After _timer reaches _timer_max, the sample period has been reached. 
//ISR(TIMER1_COMPA_vect){    //This is the interrupt request
//  _timer++;
//}

void Timer1_ISR()
{
  _timer++;
}

/*
// Sets internal Timer 1 to 1 ms, starts it, and finds value of _timer_max needed for the current sample period (seconds). 
void initTimer1(double seconds) {
  // 1 ms minimum (0.001 seconds)
  //OCR0A=(250000*seconds)-1;
  _timer = 0;
  _timer_max = 1000.0*seconds; 
  
  //TCCR0A|=(1<<WGM01);    // Set the CTC mode
  TCCR1B |= (1<<WGM12); //timer1 - enable the CTC mode
  
  OCR1A=0xF9;            // Set the value for 1ms
  
  //TIMSK0|=(1<<OCIE0A);   // Set the interrupt request
  TIMSK1 |= (1<<OCIE1A);   //timer1 enable the interrupt
  
  sei();                 // Enable interrupt
  
  TCCR1B|=(1<<CS01);     // Set the prescale 1/64 clock
  TCCR1B|=(1<<CS00);

  TCCR1A = 0;
  TCCR1B = 0;
  
}
*/ 

void serialEvent(){   // Note: serialEvent() doesn't work on Arduino Due!!!
  //delay(100); 

  char inChar;
  while (Serial && Serial.available()>0) {
    // get the new byte:
    inChar = (char)Serial.read();
    // add it to the inputString:
    dataIn += inChar;
    // if the incoming character is a newline, set a flag so the main loop can
    // do something about it:
    if (inChar == eot) {
      dataReceived = true;
    }
  }
}
