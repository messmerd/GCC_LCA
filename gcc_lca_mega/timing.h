
#ifndef _TIMING_H_
#define _TIMING_H_ 

#include <Arduino.h>

#define CONFIG_FILE "/config.txt"
#define SENSORS_FILE "/sensors.txt"
#define DEBUG_FILE "/debug.txt"
#define DATALOG_FILE_ROOT "/data"

void setRTCSQWInput(float seconds); 
void setCounter5(float seconds); 
void pushbuttonPress(); 
void startTest();
void stopTest();

#endif 
