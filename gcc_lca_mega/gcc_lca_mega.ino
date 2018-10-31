#include <SPI.h>
#include <SD.h>
#include <Wire.h>
#include "fileIO.h"
#include "Adafruit_MAX31855.h"
#include "RTClib.h"  

extern Config conf;     // An singleton object for working with the config file and sensor file

RTC_DS3231 rtc; 

//char daysOfTheWeek[7][12] = {"Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"};

#define MAXDO_0   50
#define MAXCS_0   51
#define MAXDO_1   48
#define MAXCS_1   49
#define MAXDO_2   46
#define MAXCS_2   47
#define MAXDO_3   44
#define MAXCS_3   45
#define MAXDO_4   42
#define MAXCS_4   43
#define MAXDO_5   40
#define MAXCS_5   41
#define MAXDO_6   38
#define MAXCS_6   39
#define MAXDO_7   36
#define MAXCS_7   37

#define MAXCLK    35

#define LED_PIN 9                 // Pin 12 isn't working... Maybe it has to do with the SD card reader? 
#define PUSHBUTTON_PIN 11         // Start test pushbutton
#define chipSelect 10             // For the SD card reader

#define NUMSAMPLES 5              // This is a random sample amount

unsigned int _timer = 0;
unsigned int _timer_max = 1000; 

unsigned int samples_elapsed = 0; 
unsigned int last_sample = 0; 

unsigned int data_file_number = 0; 

boolean led_value = 0;

#if !defined(ARDUINO_SAM_DUE) 
  void initTimer0(double seconds);
#endif

void printToFile(char *filename, String text, boolean append = true);

// initialize digital thermocouples 
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
  pinMode(PUSHBUTTON_PIN, INPUT); 

  #if !defined(ARDUINO_SAM_DUE) 
    analogReference(DEFAULT); // 5v on Uno and Mega. Note: Due uses 3.3 v reference which would cause analog thermocouples to not work (given the way things are currently set up)
  #endif
  
  Serial.begin(9600);
  while (!Serial) {
    ; // wait for serial port to connect. Needed for native USB port only
  }

  // Initialize SD library
  while (!SD.begin(chipSelect)) {
    Serial.println(F("fail init. SD"));
    delay(1000);
  }
  Serial.println("SD init.");

  data_file_number = getNextDataFile(); 

  // RTC init
  if (! rtc.begin()) {
    Serial.println("Couldn't find RTC");
    while (1);
  }

  // Read from config file, setting the date and time if needed
  conf.read(true);
  Serial.println();

  // NEED TO CHECK THAT CONFIG STUFF IS VALID - especially the sample rate

  delay(500); // Just in case things need to settle
  
  last_sample = conf.test_duration/conf.sample_rate; // The last sample before the test ends. 
  
  Serial.println("Press pushbutton to start test.");
  while (!digitalRead(PUSHBUTTON_PIN)) {; };  // Start test when pushbutton is pressed. (STILL NEED TO TEST) 

  delay(conf.start_delay*1000); 
  
  #if !defined(ARDUINO_SAM_DUE) 
    initTimer0(conf.sample_rate);
  #endif

  _timer = _timer_max; // The first sample will start right away
}

void loop() {
  if (_timer >= _timer_max) {  // One sample period has passed
    digitalWrite(LED_PIN, led_value);
    led_value=!led_value;
    _timer = 0;

    String dataString = "" + (String)samples_elapsed + "\t";
    dataString += (String)digital_thermo_0.readCelsius() + "\t"; 
    dataString += (String)digital_thermo_1.readCelsius() + "\t"; 
    dataString += (String)digital_thermo_2.readCelsius() + "\t"; 
    dataString += (String)digital_thermo_3.readCelsius() + "\t"; 
    dataString += (String)digital_thermo_4.readCelsius() + "\t"; 
    dataString += (String)digital_thermo_5.readCelsius() + "\t"; 
    dataString += (String)digital_thermo_6.readCelsius() + "\t"; 
    dataString += (String)digital_thermo_7.readCelsius(); 

    printToFile((String)DATALOG_FILE_ROOT + (String)data_file_number + ".txt", dataString, false); // print to file

    /*
    DateTime right_now = rtc.now(); 
    Serial.println("Year: " + String(right_now.year()) + ". Month: " + String(right_now.month()) + ". Day: " + String(right_now.day()));
    Serial.println("Hour: " + String(right_now.hour()) + ". Minute: " + String(right_now.minute()) + ". Second: " + String(right_now.second()));
    */
    samples_elapsed++;
  }

  while (samples_elapsed >= last_sample) {}; // Ends the test by going into an infinite loop. It works for now, but we should probably change it later. 
  
}


#if !defined(ARDUINO_SAM_DUE)  // These functions are for the internal timer and don't work on the Due
  ISR(TIMER0_COMPA_vect){    //This is the interrupt request
    _timer++;
  }

  void initTimer0(double seconds) {
    // 1 ms minimum (0.001 seconds)
    //OCR0A=(250000*seconds)-1;
    _timer = 0;
    _timer_max = 1000.0*seconds; 
    TCCR0A|=(1<<WGM01);    //Set the CTC mode
    OCR0A=0xF9;            //Set the value for 1ms
    TIMSK0|=(1<<OCIE0A);   //Set the interrupt request
    sei();                 //Enable interrupt
    TCCR0B|=(1<<CS01);    //Set the prescale 1/64 clock
    TCCR0B|=(1<<CS00);
    //TCCR0B|=(1<<CS02); // Testing.... /256 rather than /64
  }
#endif


