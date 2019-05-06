
#include "timing.h"
#include "fileIO.h"
#include "TimerOne.h" 

// This file contains methods that relate to timing (timers, RTC, etc.) and starting/stopping tests 

extern Config conf;     // An singleton object for working with the config file and sensor file

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

#define sot 0x02
#define eot 0x03

#define LED_PIN 22                // Sample period LED 
#define LED_PIN2 23               // CD (card detect) pin? 
#define PUSHBUTTON_PIN 2          // Start test pushbutton
#define CD_PIN 13                 // Card detect pin 
#define RTC_INT_PIN 47            // RTC interrupt pin 
#define chipSelect 53             // For the SD card reader

void setRTCSQWInput(float seconds)
{
  // Using timer 5 (uses digital pin 47 for the external clock source) 
  
  cli();//stop interrupts

  TCCR5A = 0; // set entire TCCR5A register to 0 (OCnA/OCnB/OCnC disconnected)
  TCCR5B = 0; // same for TCCR5B (set to what I want later in this method)
  TCNT5  = 0; // initialize counter value to 0

  //Serial.println((uint16_t)(1024*seconds));
  
  OCR5A = (uint16_t)(1024*seconds);    // Set the value for the interrupt (Multiples of 1/8 seconds will be exact)
  
  TCCR5B |= (1<<WGM52); //timer5 - enable the CTC mode (this is necessary!)
  
  TCCR5B |= 0x6; // B00000110 (external clock with clock on the falling edge) 
  //TCCR5B |= (1<<CS02);     // External clock source with clock on falling edge
  //TCCR5B |= (1<<CS01);     // ^ 

  TIMSK5 |= (1<<OCIE5A);   //timer5 enable the interrupt (Output Compare A Match Interrupt Enable)

  sei();                 // Enable interrupts
  
}

void setCounter5(float seconds)
{
  OCR5A = (uint16_t)(1024*seconds);    // Set the value for the interrupt (Multiples of 1/8 seconds will be exact)
  
  // From data sheet (about TCNTn, OCRnA/B/C, and ICRn registers): "To do a 16-bit write, the high byte must be written before the low byte." 
  TCNT5 = 0; // Clear the counter 
}

ISR(TIMER5_COMPA_vect) // This is the interrupt request
{    
  noInterrupts();
  //Serial.println("In T5 ISR");
  samplePeriodReached = true; 
  if (testStarted == false) 
  {
    //Serial.print("--New sample period: ");
    //Serial.println(samplePeriod);
    //Serial.print("--Old OCR5A: ");
    //Serial.println(OCR5A);
    setCounter5(conf.sample_period/8.0f);
    TIFR5 |= (1<<OCF5A);  // Needed (clears interrupt flag)
    //Serial.print("--New OCR5A: ");
    //Serial.println(OCR5A);
    testStarted = true;
  }
  interrupts();
}

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
      // Set Timer1 to 2*SERIAL_COMM_TIME:
      Timer1.setPeriod(2*SERIAL_COMM_TIME); // In microseconds 
      inSerialSafeRegion = false; 
      missedClock = true; // If the clock is not missed, sampling routine in the main loop will set this to false 
    }
    
  }
  
}

void pushbuttonPress()
{
  //stopTestWhenPossible = true; 
  if (!testStarted) 
  {
    // Start the test and send one-way message to PC to tell it that the test has started 
    startTest(false); 
  }
  else
  {
    stopTest(); 
  }
  
}

void startTest(bool sendSerialResponse)
{
  EIMSK &= ~(1 << INT0);  // Disable pushbutton interrupt (just in case)
  TIMSK5 &= ~(1<<OCIE5A);   //timer5 disable the interrupt (Output Compare A Match Interrupt disable)
  Timer1.stop();
  testStarted = false; 
  samples_elapsed = 0;
  last_sample = conf.test_duration/(conf.sample_period/8.0f); // The last sample before the test ends. 

  // Note: Instead of sending the "Test Started" (Running state) message here, it should be done after the 
  // while loop below when the test actually starts. For now, it prevents errors from occurring in the LCA Sync application.
  // And there should be another state of the sensor package besides Ready and Running, and it should be called StartDelay. 
  // At this point in the code we don't know if starting the test is successful or not. All we know is whether or not the start delay is starting.  
  interrupts(); 
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


  Timer1.initialize(10000); // set the timer. Needs to go off during or just after the sampling routine.  !!!!
  Timer1.start(); 
  
  setRTCSQWInput(5.0/1024);  // Should make the counter event happen almost immediately 
  TIFR5 |= (1<<OCF5A);       // clears RTC interrupt flag
  TIMSK5 |= (1<<OCIE5A);     // timer5 enable the interrupt (Output Compare A Match Interrupt Enable)
  //interrupts(); // There should be an interrupt routine entered from within this interrupt routine
  
  while (!testStarted) {}  // Wait for program to sync with RTC pulse so that the first sample is not off by much 

  // In the future, send a one-way communication message here saying that the test has started, because this is where it actually starts. 

  //EIMSK |= (1 << INT0);  // Enable pushbutton interrupt (just in case)
  digitalWrite(LED_PIN2, HIGH);
}

void stopTest()
{
  TIMSK5 &= ~(1<<OCIE5A);   //timer5 disable the interrupt (Output Compare A Match Interrupt disable)
  TIFR5 |= (1<<OCF5A);       // clears RTC interrupt flag
  Timer1.stop(); 
  testStarted = false; 
  data_file_number++;
  strcpy(dataFileName, DATALOG_FILE_ROOT);
  strcat(dataFileName, String(data_file_number).c_str());
  strcat(dataFileName, ".txt");  // THERE IS A MAX LENGTH TO THIS, SO THERE'S A MAX NUMBER OF DATA FILES
  
  Serial.write(sot);
  Serial.write(0x6); // Test Ended, OneWay 
  Serial.write(eot);

  digitalWrite(LED_PIN2, LOW);
  EIMSK |= (1 << INT0);  // Enable pushbutton interrupt (just in case)
}
