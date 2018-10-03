/*
*
* Main.cpp: Main File for ULCFSA
* Author: Dalton Mesmer and Ben Spooner
* Date created: 10/3/2018
*
*/

//sd card daata
    //test settings
  String sd_full_data; //stores all of the sd card data for later manipualtion
  String sd_pkg_name; //package name
  String sd_tst_dur; //test duration
  String sd_delay;  //delay in seconds between the button push and the test start
  String sd_spl_rt; //sample rate
  String sd_dt_tm; //date and time
  String sd_rst_dt; //says whether to reset the date and time or not
    //sensor settings
  String sd_snr_nms; //list of the one or more sensors being hooked up
  String sd_a_d; //tells whether each sensor is analog or digital
  String sd_commp; //tells what each senors communication protocol is
  String sd_pins; //tells which pins the sensors are hooked up to
  String sd_adjust; //tells whether there is a scaling factor or offset for each sensor
  String sd_isr; //gives the sensors ISR code

//Sensor inputs for this test
    int t_0 = A0;
    int t_1 = A1;
    int t_2 = A2;
    int t_3 = A3;

//Manual Inputs
    int button = 2; //just a push button for starting and stopping the test

//DataLogging Shield Inputs--apparently all these pins are set in the shield header file, so thats cool.
    //int SPI_clk = 13;
    //int SPI_miso = 12;
    //int SPI_mosi = 11;
    //int SDA = A4;
    //int SCL = A5;
    const int chipSelect = 10;

//Abstract Variables, such as for the SDcard file, etc.
    

void setup(){
  //sensor pin setup, will be set up in a function in a later version
  pinMode(t_0, INPUT_PULLDOWN); //first thermocouple input
  pinMode(t_1, INPUT_PULLDOWN); //second thermocouple input
  pinMode(t_2, INPUT_PULLDOWN); //third thermocouple input
  pinMode(t_3, INPUT_PULLDOWN); //fourth thermocouple input
  
  //button setup, will probably always be here even in later versions
  pinMode(button, INPUT_PULLDOWN);
}

void loop(){
  
}
