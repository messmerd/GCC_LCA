
short LED_PIN = 12; 
short PUSHBUTTON_PIN = 11; 

double SAMPLE_RATE = 1; // in seconds. 1 ms minimum. 

int _timer = 0;
int _timer_max = 1000; 
 
int led_value = 0;

void initTimer0(double seconds);

void setup() {

  pinMode(LED_PIN, OUTPUT);
  pinMode(PUSHBUTTON_PIN, INPUT); 
  Serial.begin(9600);
  initTimer0(SAMPLE_RATE);
}

void loop() {

  //digitalWrite(LED_PIN, digitalRead(PUSHBUTTON_PIN));

  if (_timer >= _timer_max) {
    digitalWrite(LED_PIN, led_value);
    led_value=!led_value;
    _timer = 0;
  }
  
}

ISR(TIMER0_COMPA_vect){    //This is the interrupt request
  _timer++;
}

void initTimer0(double seconds) {
  // 1 ms minimum (0.001 seconds)
  //OCR0A=(250000*seconds)-1;
  _timer = 0;
  _timer_max = 1000.0*seconds; 
  TCCR0A|=(1<<WGM01);    //Set the CTC mode
  OCR0A=0xF9;            //Set the value for 1ms
  TIMSK0|=(1<<OCIE0A);   //Set the interrupt request
  sei();                 //Enable interrupt
  TCCR0B|=(1<<CS01);    //Set the prescale 1/64 clock
  TCCR0B|=(1<<CS00);
  
}


