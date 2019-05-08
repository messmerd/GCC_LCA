// This file contains methods that relate to timing (timers, RTC, etc.) and starting/stopping tests 

#ifndef _TIMING_H_
#define _TIMING_H_ 

#include <Arduino.h>

#define SERIAL_COMM_TIME 300000   // The max time in microseconds needed for serial communications in one sample period (need to determine experimentally)

void setRTCSQWInput(float seconds); 
void setCounter5(float seconds); 
void Timer1_ISR();
void pushbuttonPress(); 
void startTest(bool sendSerialResponse);
void stopTest();

#endif 
