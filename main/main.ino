#include <SPI.h>
#include <SD.h>
#include "fileIO.h"

extern Config conf; // A struct containing the config file info

const char *CONFIG_FILE = "/config.txt";
const char *SENSORS_FILE = "/sensors.txt";
const char *DEBUG_FILE = "/debug.txt";
char *DATALOG_FILE = "/datalog0.txt";

const short LED_PIN = 9; // Pin 12 isn't working... Maybe it has to do with the SD card reader? 
const short PUSHBUTTON_PIN = 11;  
const int chipSelect = 10; // For the SD card reader

int _timer = 0;
int _timer_max = 1000; 
 
int led_value = 0;

void initTimer0(double seconds);
void collectAndWriteData();
//void collectData();

void setup() {

  pinMode(LED_PIN, OUTPUT);
  pinMode(PUSHBUTTON_PIN, INPUT); 
  
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
    //Serial.print("Timer event. led_value=");
    //Serial.println(led_value);
    collectAndWriteData(); 
  }
  
}


void collectAndWriteData()
{
  // make a string for assembling the data to log:
  String dataString = "";
  
  // read three sensors and append to the string:
  for (int analogPin = 0; analogPin < 3; analogPin++) {
    int sensor = analogRead(analogPin);
    dataString += String(sensor);
    if (analogPin < 2) {
      dataString += ",";
    }
  }
  
  // open the file. note that only one file can be open at a time,
  // so you have to close this one before opening another.
  File dataFile = SD.open(DATALOG_FILE, FILE_WRITE);
  
  // if the file is available, write to it:
  if (dataFile) {
    dataFile.println(dataString);
    dataFile.close();
    // print to the serial port too:
    Serial.println(dataString);
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


