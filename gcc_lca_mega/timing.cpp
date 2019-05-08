// This file contains methods that relate to timing (timers, RTC, etc.) and starting/stopping tests 

#include "fileIO.h"
#include "timing.h"

#include "TimerOne.h"   // From https://github.com/PaulStoffregen/TimerOne 

// Use variables and constants defined in gcc_lca_mega.ino and other files: 
extern Config conf;     // A singleton object for working with the config.txt file

extern bool samplePeriodReached;
extern bool testStarted;
extern volatile bool missedClock;
extern volatile bool inSerialSafeRegion;

extern unsigned long samples_elapsed;
extern unsigned long last_sample; 
extern unsigned int data_file_number; 
extern char dataFileName[16]; 
extern byte dataIn[150];
extern int dataInPos;

extern const char* CONFIG_FILE;
extern const char* SENSORS_FILE;
extern const char* DEBUG_FILE;
extern const char* DATALOG_FILE_ROOT;

extern const byte sot;
extern const byte eot;

extern const int LED_PIN;                 // Sample period LED 
extern const int LED_PIN2;                // Multipurpose LED 
extern const int PUSHBUTTON_PIN;          // Start/stop test pushbutton
extern const int CD_PIN;                  // Card detect pin 
extern const int RTC_INT_PIN;             // Real-time clock (RTC) interrupt pin 

// Initializes Timer5 so that a sample period of the argument seconds is used
void setRTCSQWInput(float seconds)
{
  // Using CPU Timer 5 (uses digital pin 47 for the external clock source) 
  
  cli(); // Stop interrupts

  TCCR5A = 0; // Set entire TCCR5A register to 0 (OCnA/OCnB/OCnC disconnected)
  TCCR5B = 0; // Same for TCCR5B (set to what I want later in this method)
  TCNT5  = 0; // Initialize counter value to 0
  
  OCR5A = (uint16_t)(1024*seconds);    // Set the value for the interrupt (Multiples of 1/8 seconds will be exact)
  
  TCCR5B |= (1<<WGM52); // Timer5 - enable the CTC mode (this is necessary!)
  
  TCCR5B |= 0x6; // B00000110 (external clock with clock on the falling edge) 
  //TCCR5B |= (1<<CS02);     // External clock source with clock on falling edge
  //TCCR5B |= (1<<CS01);     // ^ 

  TIMSK5 |= (1<<OCIE5A);   // Timer5 enable the interrupt (Output Compare A Match Interrupt Enable)

  sei();                 // Enable interrupts
}

// Set Timer5 so that a sample period of the argument seconds is used
void setCounter5(float seconds)
{
  OCR5A = (uint16_t)(1024*seconds);    // Set the value for the interrupt (Multiples of 1/8 seconds will be exact) 
  TCNT5 = 0; // Clear the counter 
}

// Tells the main loop that it is time to take a sample, and also helps synchronize the start of a test with the RTC SQW signals 
ISR(TIMER5_COMPA_vect) // This is the interrupt request
{    
  noInterrupts();
  samplePeriodReached = true; 
  if (testStarted == false) 
  {
    setCounter5(conf.sample_period/8.0f);
    TIFR5 |= (1<<OCF5A);  // Needed (clears interrupt flag)
    testStarted = true;
  }
  interrupts();
}

// 
void Timer1_ISR()
{
  if (testStarted && !samplePeriodReached)
  {
    if (missedClock)
    {
      // Error! Clock signal not received after its sample period 
      // isSerialSafeRegion = true;  // ?? (for sending error)
      //Serial.println("---sampling error!");
    }
    else if (inSerialSafeRegion)
    {
      Timer1.setPeriod(2*SERIAL_COMM_TIME); // In microseconds 
      inSerialSafeRegion = false; 
      missedClock = true; // If the clock is not missed, sampling routine in the main loop will set this to false 
    }
    
  }
  
}

// The interrupt routine for when the pushbutton is pressed 
void pushbuttonPress()
{
  if (!testStarted) 
  {
    // Start the test and send one-way message to PC to tell it that the test has started 
    startTest(false); 
  }
  else
  {
    // Stop the test and send one-way message to PC to tell it that the test has stopped  
    stopTest(); 
  }
  
}

// Start the test. If sendSerialResponse == true, sends a two-way response saying the test has started, else it sends a one-way message 
void startTest(bool sendSerialResponse)
{
  EIMSK &= ~(1 << INT0);    // Disable pushbutton interrupt (just in case)
  TIMSK5 &= ~(1<<OCIE5A);   // Timer5 disable the interrupt (Output Compare A Match Interrupt disable)
  Timer1.stop();
  testStarted = false; 
  samples_elapsed = 0;
  last_sample = conf.test_duration/(conf.sample_period/8.0f); // The last sample before the test ends. 

  // Note: Instead of sending the "Test Started" (Running state) message here, it should be done after the 
  // while loop below when the test actually starts. For now, it prevents errors from occurring in the LCA Sync application.
  // And there should be another state of the sensor package besides Ready and Running, and it should be called StartDelay. 
  // At this point in the code we don't know if starting the test is successful or not. All we know is whether or not the start delay is starting.  
  interrupts(); // There should be an interrupt routine entered from within this interrupt routine (the Timer5 interrupt routine)
  if (sendSerialResponse) // If a two-way communication response should be sent. (Test is being started via the LCA Sync Windows application)
  {
    // ProcessOtherCategory() uses startTest(true). The LCA Sync application needs a response right away, so
    //   if there's a start delay, it won't receive a response until after the start delay which is way too late. 
    //   Therefore, 
    Serial.write(sot); 
    Serial.write(dataIn[1]);
    Serial.write(dataIn[2]);
    Serial.write((byte)true);  // Sending "Test has started". In the future, this should send the current state: Ready or StartDelay 
    Serial.write(eot); 
  }
  else  // Send a one-way message instead 
  {
    Serial.write(sot);
    Serial.write(0xE); // Test Started, OneWay. In the future, this should be the current state: Ready or StartDelay 
    Serial.write(eot);
  }

  delay(conf.start_delay*1000); // Start delay

  Timer1.initialize(10000);  // Set the timer. Needs to go off during or just after the sampling routine.  !!!!
  Timer1.start(); 
  
  setRTCSQWInput(5.0/1024);  // Should make the counter event happen almost immediately 
  TIFR5 |= (1<<OCF5A);       // Clears RTC interrupt flag
  TIMSK5 |= (1<<OCIE5A);     // Timer5 enable the interrupt (Output Compare A Match Interrupt Enable)
  //interrupts(); // There should be an interrupt routine entered from within this interrupt routine
  
  while (!testStarted) {}  // Wait for program to sync with RTC pulse so that the first sample is not off by much. (This happens in the Timer5 interrupt routine) 

  // In the future, send a one-way communication message here saying that the test has started, because this is where it actually starts. 

  //EIMSK |= (1 << INT0);  // Enable pushbutton interrupt (just in case)
}

// Stop the test. Send one-way message to the PC telling it that the test has stopped. 
void stopTest()
{
  TIMSK5 &= ~(1<<OCIE5A);   // Timer5 disable the interrupt (Output Compare A Match Interrupt disable)
  TIFR5 |= (1<<OCF5A);      // Clears RTC interrupt flag
  Timer1.stop(); 
  testStarted = false; 
  
  data_file_number = getNextDataFile();   // Use a new data file for the next test.    
  // Get filename of new data file: 
  strcpy(dataFileName, DATALOG_FILE_ROOT);
  strcat(dataFileName, String(data_file_number).c_str());
  strcat(dataFileName, ".txt");  // THERE IS A MAX LENGTH TO THIS, SO THERE'S A MAX NUMBER OF DATA FILES

  // Send one-way message to PC telling it that the test has ended: 
  Serial.write(sot);
  Serial.write(0x6); // Test Ended, OneWay 
  Serial.write(eot);

  EIMSK |= (1 << INT0);  // Enable pushbutton interrupt (just in case)
}
