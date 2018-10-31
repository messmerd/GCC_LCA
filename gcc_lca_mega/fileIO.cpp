#include <ArduinoJson.h>
#include "fileIO.h"	
#include "RTClib.h"
#include <SPI.h>
#include <SD.h>

Config conf; 

Sensor* sensors;

extern RTC_DS3231 rtc; 

Config::Config() {};  // Create default constructor later

boolean Config::read(boolean setRTC)
{
  // Open file for reading
  File file = SD.open(CONFIG_FILE);

  StaticJsonBuffer<400> jsonBuffer;  // The size of this buffer can make or break this code. 400 was enough. I don't know the minimum  

  // Parse the root object
  JsonObject &root = jsonBuffer.parseObject(file);

  if (!root.success()) {
    Serial.println(F("config read fail, using default"));
    return 1; // error 
  }

  package_name = root["pkg_name"] | "Untitled";    // Package name 
  test_duration = root["test_dur"] | 1200;         // 1200 second (20 minute) default test duration
  start_delay = root["start_delay"] | 0;           // 0 second default start delay
  sample_rate = root["smpl_rate"] | 1.0;           // 1 second default sample rate
  temp_units = root["temp_units"] | 'C';           // Celcius is default 
  initial_date = root["init_date"] | "01/01/2000"; // 01/01/2000 initial date (DD/MM/YYYY)
  initial_time = root["init_time"] | "00:00:00";   // 00:00:00 initial time
  reset_date_time = root["reset_date_time"] | 1;   // Time and date are reset by default.  

  Serial.println(package_name);
  Serial.println(test_duration);
  Serial.println(start_delay);
  Serial.println(sample_rate);
  Serial.println(temp_units);
  Serial.println(initial_date);
  Serial.println(initial_time);
  Serial.println(reset_date_time);

  if (setRTC && reset_date_time) // If you want to set the RTC and it needs to be set
  {
    Serial.print("Setting the date and time...");
    // Maybe there's a more efficient way of doing these:
    int16_t y = atoi(initial_date.substring(6, 10).c_str());
    int8_t m = atoi(initial_date.substring(3, 5).c_str());
    int8_t d = atoi(initial_date.substring(0, 2).c_str());

    int8_t h = atoi(initial_time.substring(0, 2).c_str());
    int8_t mi = atoi(initial_time.substring(3, 5).c_str());
    int8_t s = atoi(initial_time.substring(6, 8).c_str());
    
    /*
    Serial.println((String)y);
    Serial.println((String)m);
    Serial.println((String)d);
    Serial.println((String)h);
    Serial.println((String)mi);
    Serial.println((String)s);
    */
    
    rtc.adjust(DateTime(y, m, d, h, mi, s));

    root["reset_date_time"] = 0; 
    
    file.close(); 
    SD.remove(CONFIG_FILE); // So that it gets overwritten
    file = SD.open(CONFIG_FILE, FILE_WRITE);
    
    // Serialize JSON to file
    if (root.printTo(file) == 0) {
      Serial.println(F("Failed to write to file"));
    }
    
    Serial.println("done!");
  }
  
  file.close();
  return 0; 
}


void printToFile(String filename, String text, boolean append)
{
  if (append != true) {
    // Delete file here. The file will essentially be overwritten rather than appended to. 
    SD.remove(filename); // So that it gets overwritten
  }
  
  // open the file. note that only one file can be open at a time,
  // so you have to close this one before opening another.
  File dataFile = SD.open(filename, FILE_WRITE);
  
  // if the file is available, write to it:
  if (dataFile) {
    dataFile.println(text);
    dataFile.close();
    // print to the serial port too:
    Serial.println(text);
  }
  // if the file isn't open, pop up an error:
  else {
    Serial.println("error w/ datalog");
  }
}

unsigned int getNextDataFile()
{
  unsigned int num = 0; 
  while (SD.exists(DATALOG_FILE_ROOT + (String)num + ".txt")) {
    num++; 
  }
  return num; 
}



