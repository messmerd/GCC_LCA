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
#define LEDR 2
//Defines what digital pins each part of the thermocouple is linked with

//Defines which digital pins the LED's should be hooked into
//This is slightly different in France
int ledDigitalOne[] = {8,7,6}; //the three digital ports the LED is plugged into
//Red = 7
//Yellow = 6
//Green = 8



const boolean ON = HIGH;
const boolean OFF = LOW;
//Defines on and off to be a low voltage and a high voltage respectivly. 

const boolean RED[] = {OFF, ON, OFF};
const boolean YELLOW[] = {OFF,OFF,ON};
const boolean GREEN[] = {ON,OFF,OFF};
//Defines primary LED colors based on which LED lights up on the board


// initialize the Thermocouple
Adafruit_MAX31855 thermocouple(MAXCLK, MAXCS, MAXDO);

// Example creating a thermocouple instance with hardware SPI
// on a given CS pin.
//#define MAXCS   10
//Adafruit_MAX31855 thermocouple(MAXCS);

void setup() {
  Serial.begin(9600);

  pinMode(2,OUTPUT);
  pinMode(11,OUTPUT);

  // initialize the 11th digital pin as an output
  // this is for the relay
  
  while (!Serial) delay(1); // wait for Serial on Leonardo/Zero, etc

  //Serial.println("MAX31855 test");
  // wait for MAX chip to stabilize

  for (int i = 0; i < 3; i++)
  {
    pinMode(ledDigitalOne[i],OUTPUT);
  }
  //initializes the led pins to be able to recieve an output
  
  delay(200);
}

void loop() {
  // basic readout test, just print the current temp
  
  
  // Serial.print("Internal Temp = ");
  //Serial.println(thermocouple.readInternal());
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
     //Serial.print("C = "); 
     Serial.println(c); 
     //Prints the temperature read by the thermocouple in Celcius
   }

    if(c <= 40)
    {
      //digitalWrite(11,HIGH);
      digitalWrite(2,LOW);
    
    }
    // if the temperature is less than or equal to 32 degrees c (as read by the thermocouple
    // writes a high voltage to the relay to let power through the resisitor
    else if (c >= 50)
    {
      //digitalWrite(11,LOW);
      digitalWrite(2,HIGH);
      
      
    }
    
    //if the temp is more than 38 degrees c
    //Writes a low voltage to the relay to stop power from going through the resistor
   
   if(c < 43)
   {
      setColor(ledDigitalOne, GREEN);
      // calls a user created function (found at the bottom of the code) used to light up the RGB LED blue
   }
   else if(c > 47)
   {
      setColor(ledDigitalOne,RED);
      //calls setColor to light the LED up red
   }
   else
   {
        setColor(ledDigitalOne,YELLOW);
        //calls setColor to light the LED up green
   }
    //sets the color of the led based off of the temperature
   
   
 
   delay(200);
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
