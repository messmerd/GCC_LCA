#ifndef _FILE_IO_H_
#define _FILE_IO_H_

#include <Arduino.h>

#define CONFIG_FILE "/config.txt"
#define SENSORS_FILE "/sensors.txt"
#define DEBUG_FILE "/debug.txt"
#define DATALOG_FILE_ROOT "/data"

// Class for working with config file and sensors file on SD card 
class Config
{
public: 
  Config();                   // Default constructor - needs to be implemented

  String package_name;        // Package name
  unsigned long test_duration;// Test duration
  unsigned int start_delay;   // Delay in seconds between the button push and the test start
  byte sample_period;         // Sample period
  char temp_units;            // C, F, or K
  String initial_date;        // Initial date
  String initial_time;        // Initial time
  boolean reset_date_time;    // Says whether to reset the date and time or not
  
  boolean need_to_sync_sd;    // For future use
  boolean need_to_sync_bt;    // For future use

  boolean read(boolean setRTC = false); // Reads from SD card's config file, updates values of config variables stored on Arduino, and also sets RTC if needed. 
  boolean updateConfigFile(); // Overwrites the config file with the config file contents stored locally in conf.
};

// This struct stores information about a sensor so that sensors can be treated modularly and be freely added/removed from system. Currently, none of this information is used. 
struct Sensor
{ 
  String sensor_name; 
  short a_d;                  // Whether a sensor is analog or digital
  short commp;                // Which communication protocol a sensor uses
  int* pins;                  // Which Arduino pins the sensor uses
  String adjust_;             // The scaling factor or offset for a sensor. (X = input)
  String isr;                 // ISR code to be executed when sensor data is received
}; 


// Prints text to a file specified by filename. May append to file or overwrite.
void printToFile(char* filename, String text, boolean append = true);

// Finds a new unique data file name to use for the current test. 
// Returns the unique number to use in the new data file name.
unsigned int getNextDataFile();


#endif
