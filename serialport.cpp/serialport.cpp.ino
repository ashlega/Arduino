#include <SoftwareSerial.h>
SoftwareSerial btSerial(4, 2); // RX, TX

String command = ""; // Stores response of the HC-06 Bluetooth device
char inputChar;

const String dataDelimiter = "|";

int motionPin = 8; // Input for HC-S501
const int trigPin = 9;
const int echoPin = 10;
const int relayPin = 12;

long duration;
int distance;
int motionValue;

void setup() {
  // Open serial communications:
  Serial.begin(9600);
  // The HC-06 defaults to 9600 according to the datasheet.
  //btSerial.begin(9600);

  //pinMode(trigPin, OUTPUT); // Sets the trigPin as an Output
  //pinMode(echoPin, INPUT); // Sets the echoPin as an Input
  pinMode(motionPin, INPUT);
  pinMode(relayPin, OUTPUT);
}


void ReadCommand()
{
  if (Serial.available()) {
    inputChar = (char)Serial.read();
    command += inputChar;
  }
}
void ProcessBTCommands()
{
   if (Serial.available()) 
   {
      ReadCommand();
      int i = command.indexOf(dataDelimiter);
      while(i > 0)
      {
        String currentCommand = command.substring(0, i);
        if(i+1 < command.length()){
          command = command.substring(i+1);
        }
        else command = "";
        if(currentCommand == "RON")
        {
          //ProcessDistance();
          digitalWrite(relayPin, HIGH);
        }
        else if(currentCommand == "ROFF")
        {
          //ProcessDistance();
          digitalWrite(relayPin, LOW);
        }
        else if(currentCommand == "D")
        {
          //ProcessDistance();
          motionValue = digitalRead(motionPin);
          Serial.print("MN:");
          Serial.print(motionValue);
          Serial.print(dataDelimiter);
        }
        else{
          Serial.println(currentCommand);//AT command response
        }
        i = command.indexOf(dataDelimiter);
      }
  }
}

void ProcessDistance()
{
  digitalWrite(trigPin, LOW);
  delayMicroseconds(2);
  digitalWrite(trigPin, HIGH);
  delayMicroseconds(10);
  digitalWrite(trigPin, LOW);
  duration = pulseIn(echoPin, HIGH);
  distance= duration*0.034/2;
  Serial.print("D:");
  Serial.print(distance);
  Serial.print(dataDelimiter);
}

void loop() {
  //ProcessDistance();

    ProcessBTCommands();
    /*  
    while (Serial.available()){
      inputChar = Serial.read();
      btSerial.write(inputChar);   
      delay(10); 
    }
    */
    //delay(50);
}
