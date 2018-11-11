// Bang-Bang Temperature Control
#include <SPI.h>
#include "Adafruit_MAX31855.h"

#define COMMON_ANNODE 

// Upper and lower temperature bounds in degrees Celcius: 
#define TEMP_UPPER_BOUND 50
#define TEMP_LOWER_BOUND 40

// Time between temperature measurements in milliseconds: 
#define SAMPLE_RATE 1000

// Defines what digital pins each part of the thermocouple is connected to: 
#define MAXDO   3
#define MAXCS   4
#define MAXCLK  5

#define LEDR       2    // Red LED pin
#define RELAY_PIN  11   // Relay pin 

// Defines three temperature regions for determining which color to set the RGB LED (in Celcius): 
#define LED_TEMP_LOWER_BOUND  43 
#define LED_TEMP_UPPER_BOUND  47 

int ledDigitalOne[] = {6,8,7}; // The three digital ports the LED is plugged into
// 6 = red, 8 = blue, 7 = green

// Defines on and off to be a low voltage and a high voltage respectivly:
#define ON HIGH
#define OFF LOW

//Defines primary LED colors based on which LED lights up on the board:
const boolean RED[] = {ON, OFF, OFF};
const boolean BLUE[] = {OFF,ON,OFF};
const boolean GREEN[] = {OFF,OFF,ON};

// Initialize the thermocouple
Adafruit_MAX31855 thermocouple(MAXCLK, MAXCS, MAXDO);

// Example creating a thermocouple instance with hardware SPI
// on a given CS pin.
//#define MAXCS   10
//Adafruit_MAX31855 thermocouple(MAXCS);

void setup() {
  Serial.begin(9600);

  // Initialize output pins: 
  pinMode(LEDR,OUTPUT);
  pinMode(RELAY_PIN,OUTPUT);
  for (int i = 0; i < 3; i++) 
  {
    pinMode(ledDigitalOne[i],OUTPUT);   // Initialize RGB LED
  }
  
  while (!Serial) delay(1);   // Wait for Serial
  
  delay(500);
}

void loop() {
   double temp = thermocouple.readCelsius();  // Read thermocouple 
   
   if (isnan(temp)) // If the thermocouple is not working properly
   {
     Serial.println("Something wrong with thermocouple!");
   } 
   else             // Else, the thermocouple is working properly
   {
     Serial.println(temp);   //Prints the temperature read by the thermocouple in Celcius
   }

    if(temp <= TEMP_LOWER_BOUND)        // If the temperature is less than or equal to the lower bound
    {
      // Turn relay and RGB LED on: 
      digitalWrite(RELAY_PIN, HIGH);    // Relay on; Voltage is now across resistor; Temperature will rise
      digitalWrite(LEDR, HIGH);
    }
    else if (temp >= TEMP_UPPER_BOUND)  // Else if the temperature is greater than or equal to the upper bound
    {
      // Turn relay and RGB LED off: 
      digitalWrite(RELAY_PIN, LOW);     // Relay off; No voltage across resistor; Temperature will decrease
      digitalWrite(LEDR, LOW);
    }

   // This if-else statement sets the color of the RGB LED based on the temperature:
   if (temp < LED_TEMP_LOWER_BOUND)     // In the lower temperature region
   {
      // Calls a user-created function (found at the bottom of the code) used to light up the RGB LED blue:
      setColor(ledDigitalOne, BLUE);
   }
   else if(temp > LED_TEMP_UPPER_BOUND) // In the upper temperature region 
   {
      // Calls setColor to light the LED up red:
      setColor(ledDigitalOne, RED);
   }
   else                                 // In the middle temperature region 
   {
      // Calls setColor to light the LED up green:
      setColor(ledDigitalOne,GREEN);
   }

   delay(SAMPLE_RATE);  // Delay before next temperature reading 
}


// Function takes in an LED pin location and a constant set of colors
// Changes that constant into a changeable boolean and call setColor again
void setColor(int* led, const boolean* color)
{
  for (int i = 0; i < 3; i++)
  {
    // Writes to the RGB LED to change the color based on the input color
    digitalWrite(led[i], color[i]);
  }

}
//

