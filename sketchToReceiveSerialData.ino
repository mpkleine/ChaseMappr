// Chasemappr receive test

void setup() {
  Serial.begin(9600);
}

void loop() {
  while (!Serial) {
    delay(2);  //delay to settle serial
    String incoming = 
           Serial.readString();
    Serial.println(incoming);
  } 
}
