#ifndef _FILE_IO_H_
#define _FILE_IO_H_

#include <Arduino.h>

//extern const char *CONFIG_FILE;
//extern const char *SENSORS_FILE;
//extern const char *DEBUG_FILE;

#define CONFIG_FILE "/config.txt"
#define SENSORS_FILE "/sensors.txt"
#define DEBUG_FILE "/debug.txt"
#define DATALOG_FILE "/datalog0.txt"

struct Config
{
  String package_name;        // Package name
  unsigned int test_duration; // Test duration
  unsigned int start_delay;   // Selay in seconds between the button push and the test start
  double sample_rate;         // Sample rate
  char temp_units;            // C or F
  String initial_date;        // Initial date
  String initial_time; 
  boolean reset_date_time;             //says whether to reset the date and time or not

  boolean need_to_sync_sd;    // 
  boolean need_to_sync_bt;    //  
};


struct Sensor
{ 
  String sensor_name; 
  short a_d;                  //tells whether each sensor is analog or digital
  short commp;                //tells what each senors communication protocol is
  int* pins;                  //tells which pins the sensors are hooked up to
  String adjust_;             //tells whether there is a scaling factor or offset for each sensor
  String isr;                 //gives the sensors ISR code
}; 

boolean readConfig();

#endif

