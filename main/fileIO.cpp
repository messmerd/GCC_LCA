#include <ArduinoJson.h>
#include "fileIO.h"	
#include <SPI.h>
#include <SD.h>

Config conf; 

Sensor* sensors;

boolean readConfig()
{
  // Open file for reading
  File file = SD.open(CONFIG_FILE);

  StaticJsonBuffer<400> jsonBuffer;  // The size of this buffer can make or break this code 
  
  // Parse the root object
  JsonObject &root = jsonBuffer.parseObject(file);
  
  if (!root.success()) {
    Serial.println(F("Failed to read file, using default configuration"));
    return 1; // error 
  }

  conf.package_name = root["package_name"] | "Default";     // Package name 
  conf.test_duration = root["test_duration"] | 1200;        // 1200 second (20 minute) default test duration
  conf.start_delay = root["start_delay"] | 0;               // 0 second default start delay
  conf.sample_rate = root["sample_rate"] | 1.0;             // 1 second default sample rate
  conf.temp_units = root["temp_units"] | 'C';               // Celcius is default 
  conf.initial_date = root["initial_date"] | "01/01/2000";  // 01/01/2000 initial date 
  conf.initial_time = root["initial_time"] | "00:00:00";    // 00:00:00 initial time
  conf.reset_date_time = root["initial_time"] | 1;          // Time and date are reset by default.  
     
  Serial.println(conf.package_name);
  Serial.println(conf.test_duration);
  Serial.println(conf.start_delay);
  Serial.println(conf.sample_rate);
  Serial.println(conf.temp_units);
  Serial.println(conf.initial_date);
  Serial.println(conf.initial_time);
  Serial.println(conf.reset_date_time);

  file.close();
  return 0; 
}


boolean readConfig(char *config_file, char *sensors_file, char *debug_file)
{
  CONFIG_FILE = config_file;
  SENSORS_FILE = sensors_file; 
  DEBUG_FILE = debug_file;

  return readConfig();
}


