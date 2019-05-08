// This file contains methods for reading/writing to the SD card, and working with the test configuration

#include <SPI.h>
#include <SD.h>
#include "fileIO.h"	

#include "RTClib.h"     // From https://github.com/messmerd/RTClib 

Config conf; 
Sensor sensors;

// Use variables and constants defined in gcc_lca_mega.ino: 
extern RTC_DS3231 rtc; 

extern const char* CONFIG_FILE;
extern const char* SENSORS_FILE;
extern const char* DEBUG_FILE;
extern const char* DATALOG_FILE_ROOT;

extern const byte sot;
extern const byte eot;

Config::Config() // Create default constructor later
{};  

// Reads from SD card's config file, updates values of config variables stored on Arduino, and also sets RTC if needed. 
bool Config::read(bool setRTC)
{
  // Open file for reading
  File file = SD.open(CONFIG_FILE);

  if (!file) {
    // Failed to open
    sample_period = 8;   // Default 
    test_duration = 30;  // Default 
    return 1; // error 
  }
  
  String filetext; 
  filetext.reserve(33);
  filetext = ""; 

  int line = 0; 
  int i = 0;
  while (file.available() && line < 7 && i < 33)
  {
    filetext += (char)file.read(); 
    if (filetext.charAt(i) == '\t' || !file.available()) 
    {
      if (filetext.charAt(i) == '\t') {filetext.remove(i, 1); }  // Remove the '\t' at the end if it exists
      switch (line) 
      {
        case 0: 
          package_name = filetext; 
          break;
        case 1: 
          test_duration = (unsigned long)filetext.toInt(); 
          if (test_duration == 0) 
          {
            test_duration = 30; // 30 seconds is the default 
          }
          break; 
        case 2: 
          start_delay = (unsigned int)filetext.toInt(); 
          break;
        case 3: 
          sample_period = (byte)(filetext.toInt()); 
          if (sample_period < 8)  // Don't allow sample periods less than one second. (Less than 1 second is probably too fast for the Arduino)
          {
            sample_period = 8;  // default is 1 second (1 second / 0.125 = 8)
          }
          break;
        case 4: 
          initial_date = filetext; 
          break;
        case 5: 
          initial_time = filetext; 
          break;
        case 6: 
          reset_date_time = (boolean)filetext.toInt(); 
          break;
        default: 
          // Error 
          break; 
      }
      filetext = "";
      i = 0; 
      line++;
    }
    else
    {
      i++;
    }
  }

  if (setRTC && reset_date_time) // If you want to set the RTC and it needs to be set
  {
    // initial_date, initial_time, and reset_date_time fields in the config.txt file can be used to set the RTC if you can't use the LCA Sync application 
    
    // Maybe there's a more efficient way of doing these:
    int16_t y = atoi(initial_date.substring(6, 10).c_str());
    int8_t m = atoi(initial_date.substring(3, 5).c_str());
    int8_t d = atoi(initial_date.substring(0, 2).c_str());

    int8_t h = atoi(initial_time.substring(0, 2).c_str());
    int8_t mi = atoi(initial_time.substring(3, 5).c_str());
    int8_t s = atoi(initial_time.substring(6, 8).c_str());
   
    rtc.adjust(DateTime(y, m, d, h, mi, s));  // Set ChronoDot RTC

    reset_date_time = 0; // Don't set the RTC next time
    
    file.close(); 
    updateConfigFile(); // Apply the change to reset_date_time 
  }
  
  file.close();
  return 0; 
}


// Prints text to a file specified by filename. May append to file or overwrite. 
void printToFile(char* filename, String text, bool append)
{
  if (append != true) {
    // Delete file here. The file will essentially be overwritten rather than appended to. 
    SD.remove(filename); // So that it gets overwritten
  }
  
  // open the file. note that only one file can be open at a time,
  // so you have to close this one before opening another.
  File dataFile = SD.open(filename, FILE_WRITE);
  
  // If the file is available, write to it:
  if (dataFile) {
    dataFile.println(text);
    dataFile.close();
    // print to the serial port too:
    //Serial.println(text+(String)eot);
  }
  // If the file isn't open, print an error:
  else {
    //Serial.println("error w/ datalog"+(String)eot);
  }
}

bool Config::updateConfigFile()
{
  SD.remove(CONFIG_FILE);   // So that it gets overwritten
  File file = SD.open(CONFIG_FILE, FILE_WRITE);
  if (!file) {return 1; } // Error! 
  file.print(package_name); file.print('\t');
  file.print(test_duration); file.print('\t');
  file.print(start_delay); file.print('\t');
  file.print((int)sample_period); file.print('\t');
  file.print(initial_date); file.print('\t');
  file.print(initial_time); file.print('\t');
  file.print(reset_date_time);
  file.close(); 
  return 0; 
}


// Finds a new unique data file name to use for the current test. 
// Returns the unique number to use in the new data file name.
unsigned int getNextDataFile()
{
  unsigned int num = 0; 
  while (SD.exists(DATALOG_FILE_ROOT + (String)num + ".txt")) {
    num++; 
  }
  //Serial.println("Output data file: " + (String)DATALOG_FILE_ROOT + (String)num + ".txt");
  return num; // Returns the unique number to use in the new data file name
}
