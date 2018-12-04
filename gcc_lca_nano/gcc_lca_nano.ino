/*
 * This code is meant to be used with an Arduino Nano, 
 * A Adafruit MAX 31855 Digital Thermocouple, and a Vellemer VMA307 RGB LED Module
 * 
 * The goal of the code is to change the color of the LED based off of the 
 * temperatre reading of the thermocouple. (Red for hot, Green for mid temp, and blue for cold
 * 
 * WHile it was designed for the Arduino Nano it can still be used by similar 8  bit arduino products
 * like the leonardo, uno, mega, and maybe the micro (did not work with our micro because of a bug)
 */

#include <SPI.h>
#include "Adafruit_MAX31855.h"
#define COMMON_ANNODE 

// Default connection is using software SPI, but comment and uncomment one of
// the two examples below to switch between software SPI and hardware SPI:

#define MAXDO   3
#define MAXCS   4
#define MAXCLK  5
//Defines what digital pins each part of the thermocouple is linked with

int ledDigitalOne[] = {11,10,9}; //the three digital ports the LED is plugged into
// 11 = red, 10 = blue, 9  = green

const boolean ON = HIGH;
const boolean OFF = LOW;
//Defines on and off to be a low voltage and a high voltage respectivly. 

const boolean RED[] = {ON, OFF, OFF};
const boolean BLUE[] = {OFF,OFF,ON};
const boolean GREEN[] = {OFF,ON,OFF};
//Defines primary LED colors based on which LED lights up on the board


// initialize the Thermocouple
Adafruit_MAX31855 thermocouple(MAXCLK, MAXCS, MAXDO);

// Example creating a thermocouple instance with hardware SPI
// on a given CS pin.
//#define MAXCS   10
//Adafruit_MAX31855 thermocouple(MAXCS);

void setup() {
  Serial.begin(9600);
  
 
  while (!Serial) delay(1); // wait for Serial on Leonardo/Zero, etc

  Serial.println("MAX31855 test");
  // wait for MAX chip to stabilize

  for (int i = 0; i < 3; i++)
  {
    pinMode(ledDigitalOne[i],OUTPUT);
  }
  //initializes the led pins to be able to recieve an output
  
  delay(500);
}

void loop() {
  // basic readout test, just print the current temp
  
  
   Serial.print("Internal Temp = ");
   Serial.println(thermocouple.readInternal());
   // Prints the internal temperature of the thermocouple which the amplifier uses to generate a celcius temperature.
   //THIS IS NOT NEEDED

   double c = thermocouple.readCelsius();
   if (isnan(c)) 
   //Checks to make sure the thermocouple is running, if it isn't it prints that out
   {
     Serial.println("Something wrong with thermocouple!");
   } 
   else
   //This runs when the thermouple is connected
   {
     Serial.print("C = "); 
     Serial.println(c); 
     //Prints the temperature read by the thermocouple in Celcius
   }
   if(c < 10)
   {
      setColor(ledDigitalOne, BLUE);
      // calls a user created function (found at the bottom of the code) used to light up the RGB LED blue
   }
   else if(c > 60)
   {
      setColor(ledDigitalOne,RED);
      //calls setColor to light the LED up red
   }
   else
   {
        setColor(ledDigitalOne,GREEN);
        //calls setColor to light the LED up green
   }
  
   
   //Serial.print("F = ");
   //Serial.println(thermocouple.readFarenheit());
   // this is for if you want the temperature to be in Farenheit
 
   delay(1000);
}

void setColor(int* led, boolean* color)
//Function takes in a specific LED and a changeable set of colors
//Writes to the LED to change the color based on the color input
{
  for(int i = 0; i< 3; i++)
  {
    digitalWrite(led[i],color[i]);
  }
  // writes to the led to change the color based on the boolean input color
}

void setColor(int* led, const boolean* color)
//Function takes in an LED pin location and a constant set of colors
// Changes that constant into a changeable boolean and call setColor again
{
  boolean tempColor[] = {color[0],color[1],color[2]};
  //Redefines the constant boolean as a changeable boolean
  
  setColor(led,tempColor);
  //Recalls setColor with a changeable boolean
  }
//
