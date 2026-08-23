#include <Servo.h>
#include <MD_Parola.h>
#include <MD_MAX72xx.h>
#include <SPI.h>
#include <stdlib.h>
#include <string.h>

#define HARDWARE_TYPE MD_MAX72XX::GENERIC_HW
#define MAX_DEVICES 1
#define CS_PIN 10

const int ANGLE_MAX = 180;
const int ANGLE_MIN = 0;

MD_Parola ledMatrix = MD_Parola(HARDWARE_TYPE, CS_PIN, MAX_DEVICES);
Servo servo;

const int led_pins[] = {2, 3, 4, 5, 6};
const char led_codes[] = {'R', 'G', 'B', 'Y', 'P'};
const int button_pin = 7;
const int buzzer_pin = 8;
const int servo_pin = 9;
const int pot_pin = A0;

int currentGameIndex = -1;
int level = 1;
int lives = 3;
int max_level = 20;
bool gameActive = false;

int angle_sequence[20];
char number_sequence[20];
int led_sequence[20];
int note_sequence[20];
int melody_sequence[20];
char userInput[32];
int target_note_index;
int note_count;

const int note_freqs[] = {262, 294, 330, 349, 392, 440, 494};
const char note_labels[] = {'C', 'D', 'E', 'F', 'G', 'A', 'B'};

byte commandIndex = 0;

bool debug = true;
bool new_command = false;

void blink_LED(int led_pin);
void blink_all_LEDs();
void showInstructions();
void setup();
void processCommand(const char* cmd);
void readSerialCommand();
void levelUp();
int randomAngle();
int read_potentiometer_value();
void game1();
void show_number_sequence();
void game2();
void show_led_sequence();
void print_led_code(int pin_number);
void game3();
void print_note_code(int note_code);
void play_note_sequence();
void game4();
void game5();
void loop();

void blink_LED(int led_pin){
  digitalWrite(led_pin, HIGH);
  delay(800);
  digitalWrite(led_pin, LOW);   
  delay(200);
}

void blink_all_LEDs(){

  digitalWrite(led_pins[0], HIGH);
  digitalWrite(led_pins[1], HIGH);
  digitalWrite(led_pins[2], HIGH);
  digitalWrite(led_pins[3], HIGH);
  digitalWrite(led_pins[4], HIGH);

  delay(800);

  digitalWrite(led_pins[0], LOW);
  digitalWrite(led_pins[1], LOW);
  digitalWrite(led_pins[2], LOW);
  digitalWrite(led_pins[3], LOW);
  digitalWrite(led_pins[4], LOW);

  delay(200);

}

void showInstructions() {
  if (debug){
    Serial.println("=== Welcome to Memory Game ===");
    Serial.println("Commands:");
    Serial.println("start [1-5]   → play a specific game");
    Serial.println("reset          → return to main menu");
    Serial.println("quit           → quit current game session");
    Serial.println("debug           → enable/disable debug mode");
  }
}

void setup() {
  Serial.begin(9600);
  ledMatrix.begin();
  ledMatrix.setIntensity(1);
  ledMatrix.displayClear();

  for (int i = 0; i < 5; i++) pinMode(led_pins[i], OUTPUT);
  pinMode(button_pin, INPUT);
  servo.attach(servo_pin);

  userInput[0] = '\0';

  randomSeed(analogRead(A5));

  showInstructions();
}

void processCommand(const char* cmd) {
  if (strcmp(cmd, "reset") == 0 || strcmp(cmd, "quit") == 0) {
    gameActive = false;
    showInstructions();
  } else if (strcmp(cmd, "debug") == 0) {
    debug = !debug;
    if (debug) showInstructions();
  } else if (strncmp(cmd, "start", 5) == 0 && cmd[5] == ' ') {
    int g = atoi(&cmd[6]);
    if (g >= 1 && g <= 5) {
      gameActive = true;
      currentGameIndex = g;
      level = 1;
      lives = 3;
      max_level = 1;
      if(debug)Serial.print("\nSelected Game ");
      if(debug)Serial.println(g);
    } else {
      Serial.println("\nInvalid game number. Use 1-5.");
    }
  } else {
    Serial.println("\nUnknown command!\n");
  }
  new_command = false;
}

void readSerialCommand() {
  while (Serial.available()) {
    char c = Serial.read();

    if (debug) Serial.print(c);
    
    if (c == '\n') {
      if (commandIndex > 0 && userInput[commandIndex - 1] == '\r') {
        commandIndex--; // remove carriage return
      }
      userInput[commandIndex] = '\0';
      commandIndex = 0;
      new_command = true;
    } else if (commandIndex < sizeof(userInput) - 1) {
      userInput[commandIndex++] = c;
    }
    delay(10);
  }
}

void gameOver() {
  currentGameIndex = -1;
  gameActive = false;
  blink_LED(led_pins[0]);
  blink_LED(led_pins[0]);
  blink_LED(led_pins[0]);
  Serial.print("Game over! ");
  Serial.print("Score: ");
  Serial.print(level-1);
  Serial.print("\n");
}

void levelUp() {
  blink_all_LEDs();
  blink_all_LEDs();
  blink_all_LEDs();

  level++;

  if (level > max_level) max_level = level;

  switch (currentGameIndex) {
    case 1: game1(); break;
    case 2: game2(); break;
    case 3: game3(); break;
    case 4: game4(); break;
    case 5: game5(); break;
  }
}

int randomAngle() {
  return (rand() % 7) * 30;
}

int read_potentiometer_value(){
  int analogValue = analogRead(A0);
  return map(analogValue, 0, 1023, ANGLE_MIN, ANGLE_MAX);
}

// Game 1
void game1() {
  if (debug) Serial.print("Initiating new game with values: ");
  blink_LED(led_pins[0]);
  blink_LED(led_pins[1]);
  blink_LED(led_pins[0]);

  int angle_game_index = 0;
  int user_angle = 0;
  static bool prevButtonState = LOW;
  bool currentButtonState = digitalRead(button_pin);
  int SerialCounter = 0;

  for (int i = 0; i < level; i++){
    angle_sequence[i] = randomAngle();
    servo.write(angle_sequence[i]);
    blink_LED(led_pins[4]);
    if (debug) Serial.print(angle_sequence[i]);
    if (debug) Serial.print(" ");
    delay(500);
  }
  if (debug) Serial.print("\n");

  blink_LED(led_pins[1]);
  blink_LED(led_pins[1]);
  blink_LED(led_pins[1]);

  while (angle_game_index < level) {
    SerialCounter++;
    SerialCounter %= 100;
    user_angle = read_potentiometer_value();
    currentButtonState = digitalRead(button_pin);
    if (currentButtonState == HIGH && prevButtonState == LOW){
      if (abs(user_angle - angle_sequence[angle_game_index]) > 10) {
        Serial.print("false\n");
        lives--;
        if (lives > 0){
            blink_LED(led_pins[0]);
        } else{
          gameOver();
          return;
        }
      } else {
        Serial.print("true\n");
        angle_game_index++;
      }
    } else{    
      servo.write(user_angle);
    }

    prevButtonState = currentButtonState;

    if (SerialCounter == 99){
      readSerialCommand();
      if ((strcmp(userInput, "quit") == 0 || strcmp(userInput, "reset" ) == 0 || strcmp(userInput, "debug" ) == 0) && new_command) {
        processCommand(userInput);
        return;
      }
    }
    delay(10);
  }
  levelUp();
}

void show_number_sequence(){
    for (int i = 0; i < level; i++) {
    ledMatrix.displayClear();
    delay(100);
    ledMatrix.setTextAlignment(PA_CENTER);
    ledMatrix.print(String(number_sequence[i]));
    delay(1000);
  }
  ledMatrix.displayClear();
}

// Game 2
void game2() {
  for (int i = 0; i < level; i++) {
    number_sequence[i] = '0' + (rand() % 10);
    ledMatrix.displayClear();
    delay(100);
    ledMatrix.setTextAlignment(PA_CENTER);
    ledMatrix.print(String(number_sequence[i]));
    delay(1000);
  }
  ledMatrix.displayClear();
  number_sequence[level] = '\0';
  Serial.print(number_sequence);
  Serial.print("\n");

  while (lives){
    readSerialCommand();
    
    if ((strcmp(userInput, "quit") == 0 || strcmp(userInput, "reset" ) == 0 || strcmp(userInput, "debug" ) == 0) && new_command) {
      processCommand(userInput);
      return;
    }
    if (strcmp(userInput, "false" ) == 0 && new_command) {
      lives--;
      new_command = false;
      if (lives) show_number_sequence();
    }
    else if (strcmp(userInput, "true") == 0 && new_command){
      new_command = false;
      levelUp();
      return;
    } else if (new_command) {
      new_command = false;
      Serial.println("\nUnknown input!\n");
    }
  }
  gameOver();
}

void show_led_sequence(){
  for (int i = 0; i < level; i++) {
    digitalWrite(led_sequence[i], HIGH); 
    delay(500);
    digitalWrite(led_sequence[i], LOW); 
    delay(300);
  }
}

void print_led_code(int pin_number) {
  switch (pin_number){
    case 2:
      Serial.print("Red ");
      break;
    case 3:
      Serial.print("Green ");
      break;
    case 4:
      Serial.print("Blue ");
      break;
    case 5:
      Serial.print("Yellow ");
      break;
    case 6:
      Serial.print("Purple ");
      break;
  }
}

// Game 3
void game3() {
  for (int i = 0; i < level; i++) {
    int idx = rand() % 5;
    led_sequence[i] = led_pins[idx];

    print_led_code(led_sequence[i]);

    digitalWrite(led_sequence[i], HIGH); 
    delay(500);
    digitalWrite(led_sequence[i], LOW); 
    delay(300);
  }
  led_sequence[level] = '\0';
  Serial.print("\n");

  while (lives){
    readSerialCommand();
    
    if ((strcmp(userInput, "quit") == 0 || strcmp(userInput, "reset" ) == 0 || strcmp(userInput, "debug" ) == 0) && new_command) {
      processCommand(userInput);
      new_command = false;
      return;
    }
    if (strcmp(userInput, "false") == 0 && new_command) {
      lives--;
      new_command = false;
      if (lives) show_led_sequence();
    }
    else if (strcmp(userInput, "true") == 0 && new_command){
      new_command = false;
      levelUp();
      return;
    } else if (new_command){
      new_command = false;
      Serial.println("\nUnknown input!\n");
    }
    delay(10);
  }
  gameOver();
}

void print_note_code(int note_code) {
  switch (note_code){
    case 0:
      Serial.print("Do ");
      break;
    case 1:
      Serial.print("Re ");
      break;
    case 2:
      Serial.print("Mi ");
      break;
    case 3:
      Serial.print("Fa ");
      break;
    case 4:
      Serial.print("So ");
      break;
    case 5:
      Serial.print("La ");
      break;
    case 6:
      Serial.print("Si ");
      break;
  }
}

void play_note_sequence(){
  for (int i = 0; i < level; i++) {
    tone(buzzer_pin, note_freqs[note_sequence[i]]);
    delay(400);
    noTone(buzzer_pin);
    delay(150);
  }
}

// Game 4
void game4() {
  for (int i = 0; i < level; i++) {
    int n = rand() % 7;
    note_sequence[i] = n;
    print_note_code(n);
    tone(buzzer_pin, note_freqs[n]);
    delay(400);
    noTone(buzzer_pin);
    delay(150);
  }
  Serial.print("\n");

  while (lives){
    readSerialCommand();
    
    if ((strcmp(userInput, "quit") == 0 || strcmp(userInput, "reset" ) == 0 || strcmp(userInput, "debug" ) == 0) && new_command) {
      processCommand(userInput);
      return;
    }
    if (strcmp(userInput, "false") == 0 && new_command) {
      lives--;
      new_command = false;
      if (lives) play_note_sequence();
    }
    else if (strcmp(userInput, "true") == 0 && new_command){
      new_command = false;
      levelUp();
      return;
    } else if (new_command){
      new_command = false;
      Serial.println("\nUnknown input!\n");
    }
    delay(10);
  }
  gameOver();
}

// Game 5
void game5() {
  for (int i = 0; i < level; i++) {
    int n = rand() % 7;
    melody_sequence[i] = n;
    print_note_code(n);
    tone(buzzer_pin, note_freqs[n]);
    delay(400);
    noTone(buzzer_pin);
    delay(150);
  }
  Serial.print("\n");

  while (lives){
    readSerialCommand();
    
    if ((strcmp(userInput, "quit") == 0 || strcmp(userInput, "reset" ) == 0 || strcmp(userInput, "debug" ) == 0) && new_command) {
      processCommand(userInput);
      return;
    }
    if (strcmp(userInput, "false") == 0 && new_command) {
      lives--;
      new_command = false;
      if (lives) play_note_sequence();
    }
    else if (strcmp(userInput, "true") == 0 && new_command){
      new_command = false;
      levelUp();
      return;
    } else if (new_command) {
      new_command = false;
      Serial.println("\nUnknown input!\n");
    }
    delay(10);
  }
  gameOver();
}

void loop() {
  readSerialCommand();

  if (new_command) {
    processCommand(userInput);
    switch (currentGameIndex) {
      level = 0;
      case 1: game1(); break;
      case 2: game2(); break;
      case 3: game3(); break;
      case 4: game4(); break;
      case 5: game5(); break;
      default: break; 
    }
  }

  delay(10);
}
