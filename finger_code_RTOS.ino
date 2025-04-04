#include <Arduino_FreeRTOS.h>

#define thumb A0
#define index A1
#define middle A2
#define ring A3
#define pinky A4


TaskHandle_t TaskReadJoystick;
TaskHandle_t TaskControlLED;

void readJoystickTask(void *pvParameters);
void controlLEDBrightnessTask(void *pvParameters);

void setup() {
  Serial.begin(9600);
  pinMode(thumb, INPUT);
  pinMode(index, INPUT);
  pinMode(middle, INPUT);
  pinMode(ring, INPUT); 
  pinMode(pinky, INPUT);

  xTaskCreate(readJoystickTask, "ReadJoystick", 128, NULL, 1, &TaskReadJoystick);
  xTaskCreate(controlLEDBrightnessTask, "ControlLED", 128, NULL, 1, &TaskControlLED);

  vTaskStartScheduler();

/*
  analogWrite(8, 50);
  delay(150);
  analogWrite(8, 100);
  delay(150);
  analogWrite(8, 150);
  delay(150);
  analogWrite(8, 200);
  delay(150);
  analogWrite(8, 250);
  delay(150);*/
}

void loop() {
}

// Task to read joystick data and send to Unity
void readJoystickTask(void *pvParameters) {
  for (;;) {
    int thumbread = analogRead(thumb);
    int indexread = analogRead(index);
    int middleread = analogRead(middle);
    int ringread = analogRead(ring);
    int pinkyread = analogRead(pinky);


   

    Serial.print("#");
    Serial.print(thumbread);
    Serial.print(",");
    Serial.print(indexread);
    Serial.print(",");
    Serial.print(middleread);
    Serial.print(",");
    Serial.print(ringread);
    Serial.print(",");
    Serial.print(pinkyread);
    Serial.print(";");
    Serial.print("\n");

    vTaskDelay(10 / portTICK_PERIOD_MS);
  }
}

// Task to control LED brightness based on Unity input
int currentBrightness = 0; // Store current brightness value

// Task to control LED brightness based on Unity input
void controlLEDBrightnessTask(void *pvParameters) {
  String input = "";
  bool readingMessage = false;

  for (;;) {
    while (Serial.available() > 0) {
      char incomingChar = Serial.read();

      if (incomingChar == '#') { 
        input = "";  
        readingMessage = true;
      }
      else if (incomingChar == ';' && readingMessage) {  
        readingMessage = false;
        int targetBrightness = input.toInt();  
        targetBrightness = constrain(targetBrightness, 0, 255);

        // Gradually change brightness
        currentBrightness = currentBrightness + (targetBrightness - currentBrightness) / 10;  // Smooth transition


        // Debugging to check if the brightness value is reasonable
        Serial.print("#Received Brightness: ");
        Serial.println(currentBrightness);
      }
      else if (readingMessage) {
        input += incomingChar;
      }
    }
    vTaskDelay(100 / portTICK_PERIOD_MS);  // Increased delay to smoothen updates
  }
}

