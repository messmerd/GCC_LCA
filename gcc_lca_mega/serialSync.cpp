
#include "ArduinoJson.h"
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

      byte byte_array[] = {0x02, 0x01, 0xF0, 0x71, 0x6c, 0x63, 0x39, 0x4b, 0x4e, 0x4d, 0x4b, 0x69, 0x30, 0x6d, 0x41, 0x79, 0x54, 0x34, 0x6f, 0x03 };
      Serial.write(byte_array, 20);
      
      dataInPos = 0; 
      dataReceived = false;
      
      Serial.flush(); // ? 
      
      return true; // true means success(?)
      } else {return false; }
    case 0x02: // Config 
      ProcessConfigRequest(); 
      dataInPos = 0; 
      dataReceived = false;
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


void ProcessConfigRequest()
{
  if (dataInPos < 4) {
    dataReceived = false;
    // Error occurred! 
    return;
  }
  
  byte action = (byte)dataIn[2] >> 6;   // Upper 2 bits
  byte subcat = (byte)dataIn[2] & 0x1F; // Lower 5 bits 
  if (action == 0)  // Read variables
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
        Serial.write((byte)(conf.start_delay + 0x4));
        Serial.write((byte)(conf.sample_period + 0x4));
        Serial.write(eot);
        
        break; 
      default:  // The only thing that should be read is all (0), so this would be an error
        break;
    }
  }
  else if (action == 1) // Write variables
  {
    if (subcat == 1) // Package name
    {
      conf.package_name.reserve(32);
      conf.package_name = "";
      int i = 3;
      for (i; i<dataInPos-1; i++)
      {
        conf.package_name += (char)dataIn[i];
      }
      if (conf.updateConfigFile())
      {
        // Error!        
      }
      else
      {
        Serial.write(sot); 
        Serial.write(dataIn[1]);
        Serial.write(dataIn[2]);
        Serial.write(eot); 
      }
    }
    else if (subcat == 2)  // Test duration
    {
      if (dataInPos == 10) 
      {
        conf.test_duration = 0;
        conf.test_duration |= (dataIn[3] >= 'A') ? (dataIn[3] - 'A' + 10) : (dataIn[3] - '0');
        conf.test_duration = conf.test_duration << 4; 
        conf.test_duration |= (dataIn[4] >= 'A') ? (dataIn[4] - 'A' + 10) : (dataIn[4] - '0');
        conf.test_duration = conf.test_duration << 4; 
        conf.test_duration |= (dataIn[5] >= 'A') ? (dataIn[5] - 'A' + 10) : (dataIn[5] - '0');
        conf.test_duration = conf.test_duration << 4; 
        conf.test_duration |= (dataIn[6] >= 'A') ? (dataIn[6] - 'A' + 10) : (dataIn[6] - '0');
        conf.test_duration = conf.test_duration << 4; 
        conf.test_duration |= (dataIn[7] >= 'A') ? (dataIn[7] - 'A' + 10) : (dataIn[7] - '0');
        conf.test_duration = conf.test_duration << 4; 
        conf.test_duration |= (dataIn[8] >= 'A') ? (dataIn[8] - 'A' + 10) : (dataIn[8] - '0');
        if (conf.updateConfigFile())
        {
          // Error!        
        }
        else
        {
          Serial.write(sot); 
          Serial.write(dataIn[1]);
          Serial.write(dataIn[2]);
          Serial.write(eot); 
        }
      }
      else
      {
        // Error! 
      }
      
    }
    else if (subcat == 4) // Sample period 
    {
      if (dataInPos == 5)
      {
        conf.sample_period = dataIn[3] - 0x4; 
        if (conf.updateConfigFile())
        {
          // Error!        
        }
        else
        {
          Serial.write(sot); 
          Serial.write(dataIn[1]);
          Serial.write(dataIn[2]);
          Serial.write(eot); 
        }
      }
      
    }
    
    // Not implemented yet...
    return;
  }
  else  // Invalid action value
  {
    // Send error message?
    return; 
  }
  
  
}
