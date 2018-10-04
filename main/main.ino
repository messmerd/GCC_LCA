#include <SPI.h>
#include <SD.h>
#include "fileIO.h"

extern Config conf; // A struct containing the config file info

const char *CONFIG_FILE = "/config.txt";
const char *SENSORS_FILE = "/sensors.txt";
const char *DEBUG_FILE = "/debug.txt";
char *DATALOG_FILE = "/datalog0.txt";

#define LED_PIN 9                 // Pin 12 isn't working... Maybe it has to do with the SD card reader? 
#define PUSHBUTTON_PIN 11  
const int chipSelect = 10;        // For the SD card reader
#define THERMOCOUPLE_PIN_0 A0     // This identifies the port the data is collected from.
#define NUMSAMPLES 5              // This is a random sample amount

int _timer = 0;
int _timer_max = 1000; 
 
int led_value = 0;

void initTimer0(double seconds);
double collectThermocoupleData(int pin, int samples = 5);
void printToFile(char *filename, String text);

void setup() {

  pinMode(LED_PIN, OUTPUT);
  pinMode(PUSHBUTTON_PIN, INPUT); 
  analogReference(EXTERNAL); 
  
  Serial.begin(9600);
  while (!Serial) {
    ; // wait for serial port to connect. Needed for native USB port only
  }

  // Initialize SD library
  while (!SD.begin(chipSelect)) {
    Serial.println(F("Failed to initialize SD library"));
    delay(1000);
  }
  Serial.println("SD card initialized.");
  
  readConfig();
  Serial.println();
  initTimer0(conf.sample_rate);
}

void loop() {

  //digitalWrite(LED_PIN, digitalRead(PUSHBUTTON_PIN));

  if (_timer >= _timer_max) {
    digitalWrite(LED_PIN, led_value);
    led_value=!led_value;
    _timer = 0;

    printToFile(DATALOG_FILE, (String)collectThermocoupleData(THERMOCOUPLE_PIN_0, NUMSAMPLES)); // Read Thermocouple 1 and print to file
  }
  
}


double collectThermocoupleData(int pin, int num_samples = 5)
{
  uint8_t i;
  double average = 0.0; // Initializes average as a variable

  for(i=0; i<num_samples; i++) { // This loop gathers a certain number of samples and sums them up
    average += analogRead(pin); // this part reads info in from the thermocouple
    //delay(10);
  }

  // These next few lines could be described in the "correction_eq" section for the sensor a config file: 
  average /= num_samples; // divides average by 5 so it accurately represents the average
  average = average *4.9; // confusing, but the information is stored on a 1023 scale, where each value of 1-1023 represents 4.9mV.  So multiplying by 4.9 converts it to mV

  return (average - 1250)/5; // final calculations based on the thermocouple chip math. Units are Celcius
}


void printToFile(char *filename, String text)
{
  // open the file. note that only one file can be open at a time,
  // so you have to close this one before opening another.
  File dataFile = SD.open(filename, FILE_WRITE);
  
  // if the file is available, write to it:
  if (dataFile) {
    dataFile.println(text);
    dataFile.close();
    // print to the serial port too:
    Serial.println(text);
  }
  // if the file isn't open, pop up an error:
  else {
    Serial.println("error opening the data log file");
  }
}


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
  
}


