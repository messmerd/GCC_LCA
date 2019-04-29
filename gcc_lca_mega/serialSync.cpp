
#include "ArduinoJson.h"
#include "serialSync.h"
#include "fileIO.h"
#include "RTClib.h" 
#include "timing.h" 

extern Config conf; 
extern Sensor* sensors;

extern RTC_DS3231 rtc; 
//extern char dataFileName[16];

#define sot 0x02
#define eot 0x03

extern bool dataReceived;
extern byte dataIn[150]; 
extern int dataInPos;

extern bool testStarted; 

#define LED_PIN2 23               // CD (card detect) pin? 

void ProcessData()
{
  //printToFile("log.txt", dataIn, true);  // For testing...
  
  if (dataInPos < 3 || dataIn[0] != sot || dataIn[dataInPos-1] != eot) {
    dataReceived = false;
    
    // Error occurred! 
    return;
  }

  switch (dataIn[1] & 0x07)
  {
    case 0x00:  // Null/Error?
      // Error? 
      break; 
    case 0x01:  // Ping
      if (dataInPos==4 && dataIn[0]==sot && dataIn[1]==0x01 && dataIn[2]==0xF0 && dataIn[3]==eot) 
      {
        byte byte_array[] = {sot, 0x01, 0xF0, (byte)(testStarted), eot };
        Serial.write(byte_array, 5);
        
        dataInPos = 0; 
        dataReceived = false;
        
        Serial.flush(); // ?  
      } 
      break;
    case 0x02: // Config 
      ProcessConfigRequest(); 
      dataInPos = 0; 
      dataReceived = false;
      break;
    case 0x03: // Other (commands and such)
      ProcessOtherCategory(); 
      dataInPos = 0; 
      dataReceived = false;
      break; 

    case 0x04: // Not implemented yet, so return error
      break;
    default:
      // Error occurred! 
      break;
  
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
  else if (action == 1 && !testStarted) // Write variables  (can only be done when a test is not running!)
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
    else if (subcat == 3) // Start delay 
    {
      if (dataInPos == 5)
      {
        conf.start_delay = dataIn[3] - 0x4; 
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

void ProcessOtherCategory()
{
  if (dataInPos < 4) {
    dataReceived = false;
    // Error occurred! 
    return;
  }
  
  byte action = (byte)dataIn[2] >> 6;   // Upper 2 bits
  byte subcat = (byte)dataIn[2] & 0x1F; // Lower 5 bits 
  if (action == 0)  // READVAR
  {
    // Not implemented yet 
    // Reading the RTC would go here     
  }
  else if (action == 1) // SENDCOMMAND or WRITEVAR
  {
    if (subcat == 0 || subcat == 1) // Start test or end test 
    {
      if (dataInPos == 4)
      {
        if (subcat == 0 && !testStarted)
        {
          startTest(); 
          Serial.write(sot); 
          Serial.write(dataIn[1]);
          Serial.write(dataIn[2]);
          Serial.write((byte)testStarted);
          Serial.write(eot); 
        }
        else if (subcat == 1 && testStarted) 
        {
          stopTest(); 
          Serial.write(sot); 
          Serial.write(dataIn[1]);
          Serial.write(dataIn[2]);
          Serial.write((byte)testStarted);
          Serial.write(eot); 
        }
        else
        {
          // Error 
          Serial.write(sot);
          Serial.write((byte)(((byte)testStarted << 3) | 0x6)); // Test Started/Ended, OneWay 
          Serial.write(eot);
        }
      }
    }
    else if (subcat == 2 && !testStarted) // writing to the RTC  (can only be done when a test is not running!)
    {
      if (dataInPos <= 13 && dataInPos >= 10) // There's a range because the length of the year is not fixed (0 to 9999)
      {
        // The data is structured as: sot, code byte, code byte, hr byte, min byte, sec byte, month byte, day byte, year (1 to 4 bytes), eot 
        char* yr = new char[dataInPos-9];
        for (int i = 8; i < dataInPos - 1; i++)
        {
          yr[i-8] = (char)dataIn[i];
        }
        rtc.adjust(DateTime ((uint16_t)atoi(yr), (uint8_t)(dataIn[6]-4), (uint8_t)(dataIn[7]-4), (uint8_t)(dataIn[3]-4), (uint8_t)(dataIn[4]-4), (uint8_t)(dataIn[5]-4)));
        digitalWrite(LED_PIN2, !digitalRead(LED_PIN2));
        Serial.write(sot); 
        Serial.write(dataIn[1]);
        Serial.write(dataIn[2]);
        Serial.write(eot); 
      }
      else
      {
        Serial.write(sot);
        Serial.write(22); // Doesn't mean anything yet. Just here to know when an error occurs. 
        Serial.write(eot); 
      }
      
    }

    
  }


  
}
