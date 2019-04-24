#ifndef _SERIAL_SYNC_H_
#define _SERIAL_SYNC_H_

#include <Arduino.h>

#define CONFIG_FILE "/config.txt"
#define SENSORS_FILE "/sensors.txt"
#define DEBUG_FILE "/debug.txt"
#define DATALOG_FILE_ROOT "/data"


bool ProcessData();
void ProcessConfigRequest();
void ProcessOtherCategory(); 

#endif
