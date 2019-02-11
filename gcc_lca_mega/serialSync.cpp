
#include "serialSync.h"
#include "fileIO.h"
#include "RTClib.h"  

extern Config conf; 
extern Sensor* sensors;

extern RTC_DS3231 rtc; 
//extern char dataFileName[16];

extern char eot;
extern char sot;

extern bool dataReceived;
extern String dataIn; 

bool ProcessData()
{
  printToFile("log.txt", dataIn, true);  // For testing...
  
  if (dataIn.length() < 3 || dataIn[0] != sot || !dataIn.endsWith((String)eot)) {
    dataIn = "";
    dataReceived = false;
    // Error occurred! 
    return false;
  }

  switch (dataIn[1] & 0x07)
  {
    case '0':  // Null/Error?
      // Error? 
      return false; 
    case '1':  // Ping
      if (dataIn == (String)sot+'\x01'+'\x00'+(String)eot) {
      Serial.print((String)sot+'\x01'+'\x00'+"qlc9KNMKi0mAyT4o"+(String)eot);
      return true; // true means success(?)
      } else {return false; }
    case '2': // Config 
      return ProcessConfigRequest(); 

    case '3': // Other -  Not implemented yet, so return error
      // 
      return false; 

    case '4': // Not implemented yet, so return error
      return false;
    default:
      // Error occurred! 
      return false;
  
  }
  return false; 
}


bool ProcessConfigRequest()
{
  if (dataIn.length() < 4) {
    dataIn = "";
    dataReceived = false;
    // Error occurred! 
    return false;
  }
  
  byte action = (byte)dataIn[2] >> 6;   // Upper 2 bits
  byte subcat = (byte)dataIn[2] & 0x1F; // Lower 5 bits 
  if (action == 0)  // Read
  {
    switch (subcat) 
    {
      case 0: // All
        Serial.print(sot); 
        Serial.write(dataIn[1]);
        Serial.write(dataIn[2]);
        Serial.print(conf.package_name+'\x00');  // Using null terminator to mark end of string
        byte bytebuffer[3];  // Stores bottom 3 bytes of long. 
        bytebuffer[0] = conf.test_duration >> 16;
        bytebuffer[1] = conf.test_duration >> 8;
        bytebuffer[2] = conf.test_duration;
        Serial.write(bytebuffer, 3); // Send as 3 bytes
        Serial.write((byte)conf.start_delay + (byte)conf.sample_rate + eot);
        
        return true;
      default:  // The only thing that should be read is all (0), so this would be an error
        return false;
    }
  }
  else if (action == 1) // Write 
  {
    // Not implemented yet...
    return true;
  }
  else  // Invlaid action value
  {
    // Send error message?
    return false; 
  }
  
  
}
