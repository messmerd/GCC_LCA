
#ifndef _TIMING_H_
#define _TIMING_H_ 

#include <Arduino.h>

#define CONFIG_FILE "/config.txt"
#define SENSORS_FILE "/sensors.txt"
#define DEBUG_FILE "/debug.txt"
#define DATALOG_FILE_ROOT "/data"

#define SERIAL_COMM_TIME 300000   // The max time in microseconds needed for serial communications in one sample period (need to determine experimentally)

void setRTCSQWInput(float seconds); 
void setCounter5(float seconds); 
void Timer1_ISR();
void pushbuttonPress(); 
void startTest(bool sendSerialResponse);
void stopTest();

#endif 
