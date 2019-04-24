
#include "fileIO.h"	
#include "RTClib.h"
#include <SPI.h>
#include <SD.h>

Config conf; 

Sensor* sensors;

extern RTC_DS3231 rtc; 
//extern char dataFileName[16];

extern byte eot;

Config::Config() // Create default constructor later
{};  

boolean Config::read(boolean setRTC)
{
  // Open file for reading
  File file = SD.open(CONFIG_FILE);

  if (!file) {
    // Failed to open
    sample_period = 8; 
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
            test_duration = 30; 
          }
          break; 
        case 2: 
          start_delay = (unsigned int)filetext.toInt(); 
          break;
        case 3: 
          sample_period = (byte)(filetext.toInt()); 
          if (sample_period == 0) 
          {
            sample_period = 8;  // default is 1 second 
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
    //Serial.println(i);
  }

  //Serial.println(package_name);
  //Serial.println(test_duration);
  //Serial.println(start_delay);
  //Serial.println(sample_rate);
  //Serial.println(temp_units);
  //Serial.println(initial_date);
  //Serial.println(initial_time);
  //Serial.println(reset_date_time);

  if (setRTC && reset_date_time) // If you want to set the RTC and it needs to be set
  {
    //Serial.print("Setting the date and time...");
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

    
    reset_date_time = 0; 
    
    file.close(); 
    updateConfigFile(); 
    
    //Serial.println("done!");
  }
  
  file.close();
  return 0; 
}


// Prints text to a file specified by filename. May append to file or overwrite. 
void printToFile(char* filename, String text, boolean append)
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
    Serial.println(text+(String)eot);
  }
  // if the file isn't open, pop up an error:
  else {
    //Serial.println("error w/ datalog"+(String)eot);
  }
}



boolean Config::updateConfigFile()
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
