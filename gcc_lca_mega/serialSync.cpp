
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
    case 0x00:  // Null/Error?
      // Error? 
      return; 
    case 0x01:  // Ping
      if (dataIn == (String)sot+'\x01'+'\xF0'+(String)eot) {
      //Serial.print((String)sot+(String)'\x01'+(String)'\xF0'+"qlc9KNMKi0mAyT4o"+(String)eot);

      // Windows is expecting: 02-01-f0-71-6c-63-39-4b-4e-4d-4b-69-30-6d-41-79-54-34-6f-03
      // \x02\x01\xf0\x71\x6c\x63\x39\x4b\x4e\x4d\x4b\x69\x30\x6d\x41\x79\x54\x34\x6f\x03
      
      //Serial.write("\x02\x01\xF0qlc9KNMKi0mAyT4o\x03");  // not using sot and eot variables...
      Serial.write("\x02\x01\xf0\x71\x6c\x63\x39\x4b\x4e\x4d\x4b\x69\x30\x6d\x41\x79\x54\x34\x6f\x03");

      
      //Serial.print("qlc9KNMKi0mAyT4o"+(String)eot);
      //Serial.write(sot+'\x01'+'\xF0');
      //Serial.write("qlc9KNMKi0mAyT4o"+eot); 
      
      digitalWrite(10,HIGH);
      return; // true means success(?)
      } else {return; }
    case 0x02: // Config 
      ProcessConfigRequest(); 
      return;
    case 0x03: // Other -  Not implemented yet, so return error
      // 
      return; 

    case 0x04: // Not implemented yet, so return error
      return;
    default:
      // Error occurred! 
      return;
  
  }
  return; 
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
    return false;
  }
  else  // Invlaid action value
  {
    // Send error message?
    return false; 
  }
  
  
}
