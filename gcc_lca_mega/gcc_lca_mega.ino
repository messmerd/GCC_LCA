#include <SPI.h>
#include <SD.h>
#include <Wire.h>
#include "fileIO.h"
#include "Adafruit_MAX31855.h"
#include "RTClib.h"  

extern Config conf;     // An singleton object for working with the config file and sensor file

RTC_DS3231 rtc;         // Real-time clock (RTC) object

//char daysOfTheWeek[7][12] = {"Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"};

// Digital thermocouple chip select and data output pins:
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
#define MAXDO_5   3
#define MAXCS_5   2
#define MAXDO_6   5
#define MAXCS_6   4
#define MAXDO_7   7
#define MAXCS_7   6

#define MAXCLK    33              // Shared clock for all digital thermocouples

#define LED_PIN 12                // Sample period LED 
#define PUSHBUTTON_PIN 11         // Start test pushbutton
#define chipSelect 10             // For the SD card reader

#define NUMSAMPLES 5              // This is a random sample amount

unsigned int _timer = 0;
unsigned int _timer_max = 1000; 

unsigned int samples_elapsed = 0; 
unsigned int last_sample = 0; 

unsigned int data_file_number = 0; 

boolean led_value = 0;

char dataFileName[16] = DATALOG_FILE_ROOT;  // There's a max length to this! 

void initTimer0(double seconds);

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
  pinMode(PUSHBUTTON_PIN, INPUT); 

  // This is only used for analog thermocouples currently, but we are no longer using them: 
  analogReference(DEFAULT); // 5v on Uno and Mega. Note: Due uses 3.3 v reference which would cause analog thermocouples to not work (given the way things are currently set up)

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
  
  Serial.println("Press pushbutton to start test.");
  while (!digitalRead(PUSHBUTTON_PIN)) {; };  // Start test when pushbutton is pressed. 

  delay(conf.start_delay*1000); // Start delay
  
  initTimer0(conf.sample_rate); // Configure internal Timer 0 and _timer_max

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
    printToFile(dataFileName, dataString, true); // print to file

    // The time on the ChronoDot real-time clock (RTC) wasn't working, so RTC timestamps are disabled for now. 
    /*
    DateTime right_now = rtc.now(); 
    Serial.println("Year: " + String(right_now.year()) + ". Month: " + String(right_now.month()) + ". Day: " + String(right_now.day()));
    Serial.println("Hour: " + String(right_now.hour()) + ". Minute: " + String(right_now.minute()) + ". Second: " + String(right_now.second()));
    */
    samples_elapsed++;
    //interrupts();                 //Enable interrupts  (!!!)
  }

  while (samples_elapsed >= last_sample) {}; // Ends the test by going into an infinite loop. It works for now, but we should probably change it later. 
  
}


// Increments _timer every millisecond. After _timer reaches _timer_max, the sample period has been reached. 
ISR(TIMER0_COMPA_vect){    //This is the interrupt request
  _timer++;
}


// Sets internal Timer 0 to 1 ms, starts it, and finds value of _timer_max needed for the current sample period (seconds). 
void initTimer0(double seconds) {
  // 1 ms minimum (0.001 seconds)
  //OCR0A=(250000*seconds)-1;
  _timer = 0;
  _timer_max = 1000.0*seconds; 
  TCCR0A|=(1<<WGM01);    // Set the CTC mode
  OCR0A=0xF9;            // Set the value for 1ms
  TIMSK0|=(1<<OCIE0A);   // Set the interrupt request
  sei();                 // Enable interrupt
  TCCR0B|=(1<<CS01);     // Set the prescale 1/64 clock
  TCCR0B|=(1<<CS00);
}


