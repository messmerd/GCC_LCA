
#include "serialSync.h"
#include "fileIO.h"
#include "RTClib.h"  

extern Config conf; 
extern Sensor* sensors;

extern RTC_DS3231 rtc; 
//extern char dataFileName[16];

extern byte eot;
extern byte sot;

extern bool dataReceived;
extern byte dataIn[150]; 
extern int dataInPos;


bool ProcessData()
{
  printToFile("log.txt", dataIn, true);  // For testing...
  
  if (dataInPos < 3 || dataIn[0] != sot || dataIn[dataInPos-1] != eot) {
    dataReceived = false;
    // Error occurred! 
    return false;
  }

  switch (dataIn[1] & 0x07)
  {
    case 0x00:  // Null/Error?
      // Error? 
      return false; 
    case 0x01:  // Ping
      if (dataInPos==4 && dataIn[0]==sot && dataIn[1]==0x01 && dataIn[2]==0xF0 && dataIn[3]==eot) {

      // Windows is expecting: 02-01-f0-71-6c-63-39-4b-4e-4d-4b-69-30-6d-41-79-54-34-6f-03
      // \x02\x01\xf0\x71\x6c\x63\x39\x4b\x4e\x4d\x4b\x69\x30\x6d\x41\x79\x54\x34\x6f\x03

      // Changed this to 7b:
      byte byte_array[] = {0x02, 0x01, 0xF0, 0x71, 0x6c, 0x63, 0x39, 0x4b, 0x4e, 0x4d, 0x4b, 0x69, 0x30, 0x6d, 0x41, 0x79, 0x54, 0x34, 0x6f, 0x03 };
      
      //Serial.write("\x02\x01\xF0qlc9KNMKi0mAyT4o\x03");  // not using sot and eot variables...
      //Serial.write("\x02\x01\xf0\x71\x6c\x63\x39\x4b\x4e\x4d\x4b\x69\x30\x6d\x41\x79\x54\x34\x6f\x03");
      
      Serial.write(byte_array, 20);
      dataInPos = 0; 
      dataReceived = false;
      
      Serial.flush(); // ? 
      
      digitalWrite(10,HIGH);
      return true; // true means success(?)
      } else {return false; }
    case 0x02: // Config 
      ProcessConfigRequest(); 
      return true;
    case 0x03: // Other -  Not implemented yet, so return error
      // 
      return false; 

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
  if (dataInPos < 4) {
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
        Serial.write(sot); 
        Serial.write(dataIn[1]);
        Serial.write(dataIn[2]);
        Serial.print(conf.package_name);  // Using null terminator to mark end of string
        Serial.write(0x00);
        //byte bytebuffer[3];  // Stores bottom 3 bytes of long. 
        //bytebuffer[0] = (conf.test_duration >> 16);
        //bytebuffer[1] = (conf.test_duration >> 8);
        //bytebuffer[2] = conf.test_duration;
        //Serial.write(bytebuffer, 3); // Send as 3 bytes
        Serial.print(conf.test_duration, HEX);
        Serial.write(0x00);
        Serial.write((byte)(conf.start_delay+0x4));
        Serial.write((byte)(conf.sample_rate+0x4));
        Serial.write(eot);
        
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
