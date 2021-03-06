
// To do: Arduino can only connect to computer when in main loop and not at the end of a test. 
// Change it so that it can connect any time. May need to disable the serialEvent interrupt 
// during SD card operations or reading sensors. How would this effect communication with
// the computer? 

/*
  Libraries that should work: 
  SD, Max Thermocouple, SPI

  Libraries that will not work: 
  TimerOne, RTCLib (?) 
  
*/

// WARNING: Should stop using Timer0 b/c apparently millis() uses it.   
/*
// For the Due, maybe this would work:
void serialEventRun(void)  // Must use this name
{
  //if (Serial.available()) serialEvent();
  if (SerialUSB.available()) serialEvent();
  if (Serial1.available()) serialEvent1();
  if (Serial2.available()) serialEvent2();
  if (Serial3.available()) serialEvent3();
}
// See: https://forum.arduino.cc/index.php?topic=205779.0 
*/

#include <SPI.h>
#include <SD.h>
#include "fileIO.h"
#include <Adafruit_MAX31856.h>    // For universal thermocouple amplifiers 
#include "RTClib.h"  // Use https://github.com/messmerd/RTClib 
#include <DueTimer.h>   // Edited to remove definition of TC2_Handler. 

extern Config conf;     // An singleton object for working with the config file and sensor file

bool COMP_MODE;         // Whether in computer mode or not 

RTC_DS3231 rtc;         // Real-time clock (RTC) object
DateTime dt;

//char daysOfTheWeek[7][12] = {"Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"};

// Digital thermocouple chip select and data output pins:
// Note: pins 50 and 51 appear to not work on this Mega (50, 51, and 52 are used for SPI apparently)

#define MAXCS_0   2
#define MAXCS_1   3 
#define MAXCS_2   4
#define MAXCS_3   5
#define MAXCS_4   46
#define MAXCS_5   48
#define MAXCS_6   50
#define MAXCS_7   52

//#define MAXCLK    33              // Shared clock for all digital thermocouples

#define LED_PIN 47                // Sample period LED 
#define LED_PIN2 45               // CD (card detect) pin? 
#define PUSHBUTTON_PIN 33         // Start test pushbutton 
#define CD_PIN 13                 // Card detect pin 
#define RTC_INT_PIN 31            // RTC interrupt pin 
#define chipSelect 44             // For the SD card reader

#define NUMSAMPLES 5              // This is a random sample amount

#define SERIAL_COMM_TIME 300000   // The max time in microseconds needed for serial communications in one sample period (need to determine experimentally)

unsigned long samples_elapsed = 0; 
unsigned long last_sample = 0; 

unsigned int data_file_number = 0; 

bool dataReceived = false;
byte dataIn[150];
int dataInPos;
byte sot, eot; 

volatile bool testStarted;
bool samplePeriodReached;
volatile bool missedClock = 0;
volatile bool inSerialSafeRegion;
bool inSetup; 

boolean led_value = 0;

char dataFileName[16] = DATALOG_FILE_ROOT;  // There's a max length to this! "data9999.txt" is the last file because the SD library uses short 8.3 names for files. 10,000 total data files.   

String dataString;

//void initTimer0(double seconds);
bool ProcessData();
void Timer1_ISR();
void Timer3_ISR(); 


// Initialize the digital thermocouples (using hardware SPI): 
Adafruit_MAX31856 digital_thermo_0(MAXCS_0);
Adafruit_MAX31856 digital_thermo_1(MAXCS_1);
Adafruit_MAX31856 digital_thermo_2(MAXCS_2);
Adafruit_MAX31856 digital_thermo_3(MAXCS_3);
Adafruit_MAX31856 digital_thermo_4(MAXCS_4);
Adafruit_MAX31856 digital_thermo_5(MAXCS_5);
Adafruit_MAX31856 digital_thermo_6(MAXCS_6);
Adafruit_MAX31856 digital_thermo_7(MAXCS_7);

//extern SPISettings max31856_spisettings;

#define SYS_BOARD_PLLAR (CKGR_PLLAR_ONE | CKGR_PLLAR_MULA(18UL) | CKGR_PLLAR_PLLACOUNT(0x3fUL) | CKGR_PLLAR_DIVA(1UL))
#define SYS_BOARD_MCKR ( PMC_MCKR_PRES_CLK_2 | PMC_MCKR_CSS_PLLA_CLK)

void setup() 
{
  inSetup = true; 

  // Use the divide-by-64 prescaler on main clock 
  //REG_PMC_MCKR |= PMC_MCKR_PRES_CLK_1; 
  // Wait until main clock is ready
  //while (!(REG_PMC_SR & PMC_SR_MCKRDY));

  //20MHz / 4 = 5MHz, 5MHz * (9+1) = 50MHz
  //REG_CKGR_PLLAR |= CKGR_PLLAR_MULA(9);  // was 9 
  //REG_CKGR_PLLAR |= CKGR_PLLAR_DIVA(4);

  //select PLLB as the master clock
  //master clock source selection - choose main clock
  //REG_PMC_MCKR |= PMC_MCKR_CSS_PLLA_CLK;

  //while (!(REG_PMC_SR & PMC_SR_MCKRDY));

  //Set FWS according to SYS_BOARD_MCKR configuration
  ////EFC0->EEFC_FMR = EEFC_FMR_FWS(4); //4 waitstate flash access
  ////EFC1->EEFC_FMR = EEFC_FMR_FWS(4);
  // Initialize PLLA to 114MHz
  ////PMC->CKGR_PLLAR = SYS_BOARD_PLLAR;
  ////while (!(PMC->PMC_SR & PMC_SR_LOCKA)) {}
  ////PMC->PMC_MCKR = SYS_BOARD_MCKR;
  ////while (!(PMC->PMC_SR & PMC_SR_MCKRDY)) {}
  // Re-initialize some stuff with the new speed
  ////SystemCoreClockUpdate();
  
  pinMode(LED_PIN, OUTPUT);
  pinMode(LED_PIN2, OUTPUT);
  pinMode(PUSHBUTTON_PIN, INPUT_PULLUP); 
  //digitalWrite(PUSHBUTTON_PIN, LOW);
  ////pinMode(RTC_INT_PIN, INPUT_PULLUP);  // ???
  pinMode(CD_PIN, INPUT); 

  //Adafruit_MAX31856::setSPISettings(new SPISettings(1000000, MSBFIRST, SPI_MODE1)); // was 500000 (500 KHz) 

  digital_thermo_0.begin();
  digital_thermo_1.begin();
  digital_thermo_2.begin();
  digital_thermo_3.begin();
  digital_thermo_4.begin();
  digital_thermo_5.begin();
  digital_thermo_6.begin();
  digital_thermo_7.begin();

  digital_thermo_0.setThermocoupleType(MAX31856_TCTYPE_T);
  digital_thermo_1.setThermocoupleType(MAX31856_TCTYPE_T);
  digital_thermo_2.setThermocoupleType(MAX31856_TCTYPE_T);
  digital_thermo_3.setThermocoupleType(MAX31856_TCTYPE_T);
  digital_thermo_4.setThermocoupleType(MAX31856_TCTYPE_T);
  digital_thermo_5.setThermocoupleType(MAX31856_TCTYPE_T);
  digital_thermo_6.setThermocoupleType(MAX31856_TCTYPE_T);
  digital_thermo_7.setThermocoupleType(MAX31856_TCTYPE_T);

  Serial.begin(9600);
  while (!Serial) {
    ; // wait for serial port to connect. Needed for native USB port only
  }
  
  Serial.println("Serial began..");
  
  attachInterrupt(digitalPinToInterrupt(PUSHBUTTON_PIN), pushbuttonPress, RISING);
  
  samplePeriodReached = false; 
  testStarted = false; 
  missedClock = false;
  inSerialSafeRegion = false;

  dataString.reserve(15+1+115); // I'm not sure about the exact size needed.
  dataString = ""; 

  int i = 0;
  for (i=0; i<150; i++)
  {
    dataIn[i] = 0x00; 
  }
  dataInPos = 0;
  dataReceived = false;
  sot = 0x02; //'\x02'; //'!';
  eot = 0x03; //'\x03'; //'.';

  digitalWrite(LED_PIN2, digitalRead(CD_PIN));  // LED is on when SD card is inserted and off when it is not.
  
  // Initialize SD library
  // Note: SD card must be formatted as FAT16 or FAT32 
  while (!SD.begin(chipSelect)) {
    //Serial.println(F("fail init. SD"));
    delay(1000);
  }
  //Serial.println("SD init.");
  
  digitalWrite(LED_PIN2, LOW);

  data_file_number = getNextDataFile(); // Get unique number to use for new unique data file name
  strcat(dataFileName, String(data_file_number).c_str());
  strcat(dataFileName, ".txt");  // THERE IS A MAX LENGTH TO THIS, SO THERE'S A MAX NUMBER OF DATA FILES
  //Serial.println("Output data file: " + (String)dataFileName);

  // RTC init
  if (!rtc.begin()) {
    //Serial.println("Couldn't find RTC");
    while (1);
  }

  if (rtc.lostPower()) {
    //Serial.println("RTC lost power, lets set the time!");
    // following line sets the RTC to the date & time this sketch was compiled
    rtc.adjust(DateTime(F(__DATE__), F(__TIME__)));
    // This line sets the RTC with an explicit date & time, for example to set
    // January 21, 2014 at 3am you would call:
    // rtc.adjust(DateTime(2014, 1, 21, 3, 0, 0));
  }

  ////pinMode(RTC_INT_PIN, INPUT_PULLUP);
  ////rtc.writeSqwPinMode(DS3231_SquareWave1kHz); // For sample interrupts 
  
  conf.read(true);   // Read from config file, setting the date and time if needed
  //Serial.println("Just read conf.");
  //Serial.println(conf.sample_period); 
  //Serial.println(conf.test_duration);

  // NEED TO CHECK THAT CONFIG STUFF IS VALID - especially the sample rate

  delay(500); // Just in case things need to settle
  
  last_sample = conf.test_duration/(conf.sample_period/8.0f); // The last sample before the test ends. 
  //Serial.print("last samp. "); Serial.println(last_sample);

  digitalWrite(LED_PIN2,LOW);
  ////Timer1.attachInterrupt( Timer1_ISR ); // attach the service routine here
  Timer3.attachInterrupt( Timer3_ISR ); // attach the service routine here

  testStarted = false;
  inSetup = false; 

  pushbuttonPress(); // This shouldn't be here. I just put it here because the pushbutton isn't working and I wanted to test the rest of the program.

}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

void loop() 
{
  if (testStarted)
  {
    if (samplePeriodReached) // One sample period has passed
    {  
      // Old: EIMSK &= ~(1 << INT0);  // Disable pushbutton interrupt 
      ////REG_PIOC_PDR |= PIO_PDR_P1;  // Disable pushbutton interrupt on digital pin 33

      Serial.println("Sample period reached.");
      
      //TIMSK1 &= ~(1<<OCIE1A);   //timer1 disable the interrupt
      ////Timer1.stop(); 
      
      // disable serial event interrupt? 
      //noInterrupts();                 //Disable interrupts  (!!!)
      
      //unsigned long _time00 = micros(); 

      digitalWrite(LED_PIN, led_value); // Blink LED to signify the sample period being reached
      led_value=!led_value;

      dt = rtc.now(); 
    
      // Read from sensors and put results into a string
      //dataString = "#" + (String)samples_elapsed + "\t";

      /*
      dataString = "#" + (String)dt.day() + "/" + (String)dt.month() + "/" + (String)(dt.year()) + " " + (String)dt.hour() + ":" + (String)dt.minute() + ":" + (String)dt.second() + "\t";
      dataString += (String)digital_thermo_0.readThermocoupleTemperature() + "\t"; 
      dataString += (String)digital_thermo_1.readThermocoupleTemperature() + "\t"; 
      dataString += (String)digital_thermo_2.readThermocoupleTemperature() + "\t"; 
      dataString += (String)digital_thermo_3.readThermocoupleTemperature() + "\t"; 
      dataString += (String)digital_thermo_4.readThermocoupleTemperature() + "\t"; 
      dataString += (String)digital_thermo_5.readThermocoupleTemperature() + "\t"; 
      dataString += (String)digital_thermo_6.readThermocoupleTemperature() + "\t"; 
      dataString += (String)digital_thermo_7.readThermocoupleTemperature(); 
      */
      
      File dataFile = SD.open(dataFileName, FILE_WRITE);
      if (dataFile) 
      {
        dataFile.print(dt.day()); 
        dataFile.print("/");
        dataFile.print(dt.month()); 
        dataFile.print("/");
        dataFile.print(dt.year()); 
        dataFile.print(" ");
        dataFile.print(dt.hour()); 
        dataFile.print(":");
        dataFile.print(dt.minute()); 
        dataFile.print(":");
        dataFile.print(dt.second()); 
        dataFile.print("\t");
        dataFile.print(digital_thermo_0.readThermocoupleTemperature());
        dataFile.print("\t");
        dataFile.print(digital_thermo_1.readThermocoupleTemperature());
        dataFile.print("\t");
        dataFile.print(digital_thermo_2.readThermocoupleTemperature());
        dataFile.print("\t");
        dataFile.print(digital_thermo_3.readThermocoupleTemperature());
        dataFile.print("\t");
        dataFile.print(digital_thermo_4.readThermocoupleTemperature());
        dataFile.print("\t");
        dataFile.print(digital_thermo_5.readThermocoupleTemperature());
        dataFile.print("\t");
        dataFile.print(digital_thermo_6.readThermocoupleTemperature());
        dataFile.print("\t");
        dataFile.println(digital_thermo_7.readThermocoupleTemperature());
        
        dataFile.close();
      }
      
    
      // Write the measurements to the data file on the SD card
      //printToFile(dataFileName, dataString, true); // print to file

    
      //Serial.println((String)(micros()-_time00));
      //Serial.println((unsigned long)(conf.sample_rate*1000000) - SERIAL_COMM_TIME - (unsigned long)(1000000*TCNT5/1024));
      //Serial.println(conf.sample_rate); 
      //Serial.println(SERIAL_COMM_TIME);
      //Serial.println(TCNT5); 
      //printToFile(dataFileName, datStr, true);

      //delay(100);  // needed for prints? 
    
      samples_elapsed++;
      Serial.print("Samp. Elap. "); Serial.println(samples_elapsed);
    
      missedClock = false; 
      inSerialSafeRegion = true; 

      samplePeriodReached = false;
      //TIMSK1 |= (1<<OCIE1A);   //timer1 enable the interrupt  // This causes everything to stop working!

      //Serial.print("TIFR1's interrupt flag is ");
      //Serial.println((TIFR1 & (1<<OCF1A))>>1);

      ////Timer1.setPeriod((unsigned long)(conf.sample_period*125000) - SERIAL_COMM_TIME - (unsigned long)(1000000*(TC1->TC_CHANNEL[0].TC_CV/1024))); // Sample period - serial comm time - sampling routine time
      ////Timer1.stop(); Timer1.start(); // Was: Timer1.restart();  

      //interrupts();                 //Enable interrupts  (!!!)
      // Old: EIMSK |= (1 << INT0);  // Enable pushbutton interrupt 
      ////REG_PIOC_PER |= PIO_PER_P1;  // Enable pushbutton interrupt on digital pin 33
    }

    //if (Serial.available()) serialEvent(); 
    
    if (dataReceived) 
    {
      //digitalWrite(LED_PIN2,HIGH);
      //noInterrupts();
      if (Serial)  // Problem with this line? Maybe remove the Serial condition? 
      {
        if (ProcessData())
        {
  
        }
        //dataIn.clear();  // Not needed if you set dataInPos to 0. Will be overwritten.
        dataInPos = 0; 
        dataReceived = false;
      }
      //interrupts();
    }

    //digitalWrite(LED_PIN2, digitalRead(CD_PIN));

    //while (samples_elapsed >= last_sample) {}; // Ends the test by going into an infinite loop. It works for now, but we should probably change it later. 
    if (samples_elapsed >= last_sample)  // Stop the test once it reaches the test duration
    {
      stopTest(); 
    }
    
  }
  else  // Test has not started  
  {
    //if (Serial.available()) serialEvent();
    // Communicate with computer here. Outside of the test here, there are no strict requirements for how much time serial communication can take
    if (dataReceived) 
    {
      if (Serial)  // Problem with this line? Maybe remove the Serial condition? 
      {
        if (ProcessData())
        {}
        //dataIn.clear();  // Not needed if you set dataInPos to 0. Will be overwritten.
        dataInPos = 0; 
        dataReceived = false;
      }
    }
  }

  //Serial.println(TC0->TC_CHANNEL[2].TC_CV);  // Just for testing 
  ////Serial.println(TC_ReadCV(TC0,2));  // Just for testing 
  //Serial.print("   ");  // Just for testing 
  //Serial.println(TC_ReadRC(TC0,2));  // Just for testing 
  ////delay(500);  // Just for testing 
  
}



// Increments _timer every millisecond. After _timer reaches _timer_max, the sample period has been reached. 
//ISR(TIMER1_COMPA_vect){    //This is the interrupt request
//  _timer++;
//}

void Timer1_ISR()
{
  Serial.println("In Timer1_ISR");
  return;
  
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
  
  //_timer++;
}

void Timer3_ISR()
{
  if (testStarted)
  {
    samplePeriodReached = true; 
  }
  
}


void serialEvent(){   // Note: serialEvent() doesn't work on Arduino Due!!!
  //delay(100); 

  int inByte;
  while (Serial && Serial.available()>0) {
    // get the new byte:
    inByte = Serial.read();
    // add it to the inputString:
    dataIn[dataInPos] = inByte;
    dataInPos++;

    if (dataInPos == 150)
    {
      // Error. dataIn buffer is full.
    }
    // if the incoming character is a newline, set a flag so the main loop can
    // do something about it:
    if (inByte == 0x03) {
      dataReceived = true;
      //Serial.clear();
      serial_flush_buffer();  
      break;
    }
  }
}

void serial_flush_buffer()
{
  while (Serial.read() >= 0)
   ; // do nothing
}


void setRTCSQWInput(float seconds)
{
  // Using timer 5 (uses digital pin 47 for the external clock source) 

  // now using digital pin 31 as clock source 
  
  noInterrupts();//stop interrupts

  IRQn irq = TC2_IRQn;
  Tc *tc = TC0;
  uint32_t channel = 2;  
  //uint32_t frequency = 1/seconds; 

  //see 37.7.9
  REG_TC0_WPMR = 0x54494D00;
  //enable configuring the io registers. see 32.7.42
  REG_PIOA_WPMR = 0x50494F00;
  REG_PIOA_PDR |= PIO_PDR_P7;
  REG_PIOA_ABSR &= ~PIO_ABSR_P7;  // assign the io line to the peripheral

  pmc_set_writeprotect(false);

  //REG_PIOA_ABSR = ~PIO_ABSR_P7;  // Is this right? 
  //PIOA_ABSR_P7 = 0; // Peripheral AB Select = A, for digital pin 1 to be used as the external clock for TC2 (TC0, channel 2). Don't know if this works. 
  //REG_PIOA_PDR = ~PIO_PDR_P7;  // ??? 

  
  //pmc_enable_periph_clk((uint32_t)irq);
  pmc_enable_periph_clk(ID_TC2);
  
  TC_Configure(tc, channel, TC_CMR_CLKI | TC_CMR_ETRGEDG_FALLING | TC_CMR_CPCTRG | TC_CMR_TCCLKS_XC2);
  //uint32_t rc = VARIANT_MCK/128/frequency; //128 because we selected TIMER_CLOCK4 above
  //TC_SetRA(tc, channel, rc/2); //50% high, 50% low
  TC_SetRC(tc, channel, 1024*seconds);  // was TC_SetRC(tc, channel, rc);
  TC_Start(tc, channel);
  tc->TC_CHANNEL[channel].TC_IER = TC_IER_CPCS;  // Use the Register C compare interrupt 
  tc->TC_CHANNEL[channel].TC_IDR = ~TC_IDR_CPCS; // Disable all other types of interrupts for this timer except Reg. C compare
  NVIC_EnableIRQ(irq);
  
  interrupts();                 // Enable interrupts
}

void setCounter5(float seconds)
{
  pmc_set_writeprotect(false);
  
  // Was: OCR5A = (uint16_t)(1024*seconds);    // Set the value for the interrupt (Multiples of 1/8 seconds will be exact)
  TC_SetRC(TC0, 2, 1024*seconds); 
  
  // From data sheet (about TCNTn, OCRnA/B/C, and ICRn registers): "To do a 16-bit write, the high byte must be written before the low byte." 
  // Old way:  TCNT5 = 0; // Clear the counter 
  TC0->TC_CHANNEL[2].TC_CV = 0;  // Clear the counter 
}


void TC2_Handler() // This is the interrupt request
{    
  Serial.println("In TC2_Handler");
  TC_GetStatus(TC0, 2);  // This is needed apparently 
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
    // Old: TIFR5 |= (1<<OCF5A);  // Needed (clears interrupt flag)
    NVIC_ClearPendingIRQ(TC2_IRQn);  // I think this is the same as clearing the interrupt flag, but I could be wrong 
    
    //Serial.print("--New OCR5A: ");
    //Serial.println(OCR5A);
    testStarted = true;
  }
  interrupts();
}


void pushbuttonPress()
{
  Serial.println("Pushbutton press..");
  if (inSetup)
  {
    return; 
  }
  
  //stopTestWhenPossible = true; 
  if (!testStarted) 
  {
    startTest2(); 
  }
  else
  {
    stopTest2(); 
  }
  
}

void startTest()
{
  testStarted = false; 
  samples_elapsed = 0;
  delay(conf.start_delay*1000); // Start delay

  Timer1.setPeriod(10000);  // Was: Timer1.initialize(10000); // set the timer. Needs to go off during or just after the sampling routine.  !!!!
  Timer1.start(); 
    
  // Was: TIMSK5 &= ~(1<<OCIE5A);   //timer5 disable the interrupt (Output Compare A Match Interrupt disable)
  NVIC_DisableIRQ(TC2_IRQn);
  
  setRTCSQWInput(5.0/1024);  // Should make the counter event happen almost immediately 
  // Was: TIFR5 |= (1<<OCF5A);       // clears RTC interrupt flag
  NVIC_ClearPendingIRQ(TC2_IRQn);  // I think this is the same as clearing the interrupt flag, but I could be wrong 
  // Was: TIMSK5 |= (1<<OCIE5A);     // timer5 enable the interrupt (Output Compare A Match Interrupt Enable)
  NVIC_EnableIRQ(TC2_IRQn);
  interrupts(); // There should be an interrupt routine entered from within this interrupt routine
  
  while (!testStarted) {}  // Wait for program to sync with RTC pulse so that the first sample is not off by much 
  
}


void startTest2()  // For when internal timer handles the timing for sampling 
{
  samples_elapsed = 0;
  delay(conf.start_delay*1000); // Start delay

  ////Timer1.setPeriod(10000);  // Was: Timer1.initialize(10000); // set the timer. Needs to go off during or just after the sampling routine.  !!!!
  ////Timer1.start(); 
  //Serial.print("in us: ");
  //Serial.println(conf.sample_period*125000);
  Timer3.setPeriod(conf.sample_period*125000); // microseconds 
  Timer3.start(); 
  
  samplePeriodReached = true; 
  testStarted = true; 
  interrupts(); // There should be an interrupt routine entered from within this interrupt routine
  
}


void stopTest()
{
  // Was: TIMSK5 &= ~(1<<OCIE5A);   //timer5 disable the interrupt (Output Compare A Match Interrupt disable)
  NVIC_DisableIRQ(TC2_IRQn);
  Timer1.stop(); 
  testStarted = false; 
  data_file_number++;
  strcat(dataFileName, String(data_file_number).c_str());
  strcat(dataFileName, ".txt");  // THERE IS A MAX LENGTH TO THIS, SO THERE'S A MAX NUMBER OF DATA FILES
  
}

void stopTest2()
{
  ////Timer1.stop(); 
  Timer3.stop(); 
  testStarted = false; 
  data_file_number++; 
  strcat(dataFileName, String(data_file_number).c_str());
  strcat(dataFileName, ".txt");  // THERE IS A MAX LENGTH TO THIS, SO THERE'S A MAX NUMBER OF DATA FILES
  
  
}
