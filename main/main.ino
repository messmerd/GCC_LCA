#include <SPI.h>
#include <SD.h>
#include "fileIO.h"
#include "Adafruit_MAX31855.h"

extern Config conf; // A struct containing the config file info

// Example creating a thermocouple instance with software SPI on any three
// digital IO pins.
#define MAXDO_0   3
#define MAXCS_0   4
#define MAXDO_1   40
#define MAXCS_1   42
#define MAXCLK    5

#define LED_PIN 9                 // Pin 12 isn't working... Maybe it has to do with the SD card reader? 
#define PUSHBUTTON_PIN 11         // 
#define chipSelect 10             // For the SD card reader
#define THERMOCOUPLE_PIN_0 A0     // This identifies the port the data is collected from.
#define THERMOCOUPLE_PIN_1 A1     // This identifies the port the data is collected from.

#define NUMSAMPLES 5              // This is a random sample amount
//uint16_t samples[NUMSAMPLES];

unsigned int _timer = 0;
unsigned int _timer_max = 1000; 
 
boolean led_value = 0;

void initTimer0(double seconds);
double collectAnalogThermocoupleData(int pin, int samples = 5);
//double collectAnalogThermocoupleDataOld(int pin, int samples = 5);

//double collectDigitalThermocoupleData(Adafruit_MAX31855 thermo);
void printToFile(char *filename, String text, boolean append = true);

// initialize digital Thermocouple 0 
Adafruit_MAX31855 digital_thermo_0(MAXCLK, MAXCS_0, MAXDO_0);
Adafruit_MAX31855 digital_thermo_1(MAXCLK, MAXCS_1, MAXDO_1);

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
    Serial.println(F("fail init. SD"));
    delay(1000);
  }
  Serial.println("SD init.");
  
  readConfig();
  Serial.println();
  initTimer0(conf.sample_rate);

  delay(500); // Just in case things need to settle
  
  Serial.println("Press pushbutton to start test.");
  while (!digitalRead(PUSHBUTTON_PIN)) {; };  // Start test when pushbutton is pressed. (STILL NEED TO TEST) 
}

void loop() {

  //digitalWrite(LED_PIN, digitalRead(PUSHBUTTON_PIN));

  if (_timer >= _timer_max) {
    digitalWrite(LED_PIN, led_value);
    led_value=!led_value;
    _timer = 0;

    String dataString = "a0: " + (String)collectAnalogThermocoupleData(THERMOCOUPLE_PIN_0, NUMSAMPLES) + " C.\n";
    dataString += "a1: " + (String)collectAnalogThermocoupleData(THERMOCOUPLE_PIN_1, NUMSAMPLES) + " C.\n";
    dataString += "d0: " + (String)digital_thermo_0.readCelsius() + " C.\n"; 
    dataString += "d1: " + (String)digital_thermo_1.readCelsius() + " C.\n\n"; 
    //Serial.println(dataString);
    printToFile(DATALOG_FILE, dataString); // print to file
  }
  
}


double collectAnalogThermocoupleData(int pin, int num_samples = 5)
{
  uint8_t i = 0;
  double average = 0.0; // Initializes average as a variable

  for(i=0; i<num_samples; i++) { // This loop gathers a certain number of samples and sums them up
    average += analogRead(pin); // this part reads info in from the thermocouple
    //delay(10);
  }

  // This next line could be described in the "correction_eq" section for the sensor a config file: 
  // divides average by 5 so it accurately represents the average. 
  // confusing, but the information is stored on a 1023 scale, where each value of 1-1023 represents 4.9mV.  So multiplying by 4.9 converts it to mV.
  // final calculations based on the thermocouple chip math. Units are Celcius
  return 0.98*(average/num_samples) - 250.0; 
}

/*
double collectAnalogThermocoupleDataOld(int pin, int num_samples = 5)
{
  uint8_t i;
  float average; // Initializes average as a variable

  for(i=0; i< NUMSAMPLES;i++) { // This loop gathers 5 samples and stores them in an array
    samples[i] = analogRead(pin); // this part reads info in from the thermocouple
    //delay(10);
  }

  average = 0;  // This sets average to 0
  for (i=0; i< NUMSAMPLES; i++) { // takes 5 samples
    average += samples[i]; // sets average to the total of the 5 amounts in the average array
  }
  average /= NUMSAMPLES; // divides average by 5 so it accurately represents the average

  average = average *4.9; // confusing, but the information is stored on a 1023 scale, where each value of 1-1023 represents 4.9mV.  So multiplying by 4.9 converts it to mV

  float temp; // initializes the variable temp
  temp = (average - 1250.0)/5.0; // final calculations based on the thermocouple chip math

  Serial.print("Temperature "); // These output the temperature
  Serial.print(temp);
  Serial.println(" *C");
}

*/


void printToFile(char *filename, String text, boolean append = true)
{
  if (append != true) {
    // Delete file here. The file will essentially be overwritten rather than appended to. 
    
  }
  
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
    Serial.println("error w/ datalog");
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


