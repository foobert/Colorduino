// -*- C++ -*-

//#include <stdlib.h>
#include <Arduino.h>
#include <Colorduino.h>
#include <SerialBuffer.h>

#define SERIAL_PORT Serial
#define BUFFER_SIZE 64
byte buffer[BUFFER_SIZE];
SerialBuffer serialBuffer;

#define BAUD_RATE 9600

#define CMD_NEW_ANIMATION 0x01
#define CMD_NEW_FRAME 0x02
#define CMD_APPEND_FRAME 0x03
#define CMD_START_ANIMATION 0x04
#define CMD_FILL 0x05
#define CMD_PLASMA 0x06

#define MODE_PLAY_ANIMATION 1
#define MODE_PLASMA 2
#define MODE_STATIC 3

byte currentMode;

#define PACKED_FRAME_QUEUE_LEN 2 // number of frames we can buffer
#define SERIAL_WAIT_TIME_IN_MS 18 //((PKTBUFLEN * 8 * 1000)/ BAUD_RATE)

typedef struct
{
  unsigned char r;
  unsigned char g;
  unsigned char b;
} ColorRGB;

//a color with 3 components: h, s and v
typedef struct 
{
  unsigned char h;
  unsigned char s;
  unsigned char v;
} ColorHSV;

typedef struct Frame {
  unsigned int duration;
  byte currentPixelIndex;
  ColorRGB* pixels;
  Frame* nextFrame;
} Frame;

struct Frame* frame_new() {
  Frame* frame = (Frame*)malloc(sizeof(Frame));
  frame->duration = 0;
  frame->currentPixelIndex = 0;
  frame->nextFrame = 0;
  frame->pixels = (ColorRGB*)malloc(sizeof(ColorRGB) * 64);
  return frame;
}

void frame_delete(Frame* frame) {
  if (frame == NULL)
    return;
  
  Frame* nextFrame = frame->nextFrame;
  free(frame->pixels);
  free(frame);
  frame_delete(nextFrame);
}

Frame* frameFirst = NULL;
Frame* frameCurrent = NULL;
Frame* frameLast = NULL;
unsigned int frameDuration = 0;

void cmd_new_animation(byte len, byte* buffer) {
  //Serial.println("cmd_new_animation");
  currentMode = MODE_STATIC;
  frame_delete(frameFirst);
  frameCurrent = NULL;
  frameFirst = NULL;
  frameLast = NULL;
  frameDuration = 0;
}

void cmd_new_frame(byte len, byte* buffer) {
  //Serial.println("cmd_new_frame");
  
  Frame* newFrame = frame_new();

  unsigned int duration = (buffer[0] << 8) | buffer[1];
  newFrame->duration = duration;
  
 //Serial.print("frame duration: ");
 //Serial.println(duration, DEC);

  if (frameLast == NULL) {
    frameFirst = newFrame;
  } else {
    frameLast->nextFrame = newFrame;
  }
  frameLast = newFrame;
}

void cmd_append_frame_data(byte len, byte* buffer) {
  //Serial.println("cmd_append_frame_data");
  
  Frame* frame = frameLast;
  if (frame == NULL)
    return;
  
  //Serial.print("len: ");
  //Serial.println(len, DEC);
  //Serial.print("old currentPixelIndex: ");
  //Serial.println(frame->currentPixelIndex, DEC);
  
  for (byte i = 0; i < len; frame->currentPixelIndex < 64) {
    frame->pixels[frame->currentPixelIndex].r = buffer[i++];
    frame->pixels[frame->currentPixelIndex].g = buffer[i++];
    frame->pixels[frame->currentPixelIndex].b = buffer[i++];
    frame->currentPixelIndex++;
  }
  //Serial.print("new currentPixelIndex: ");
  //Serial.println(frame->currentPixelIndex, DEC);

}

void cmd_start_animation(byte len, byte* buffer) {
  //Serial.println("cmd_start_animation");
  currentMode = MODE_PLAY_ANIMATION;
  frameCurrent = frameFirst;
  frameDuration = frameCurrent->duration;
}

long paletteShift;

//Converts an HSV color to RGB color
void HSVtoRGB(void *vRGB, void *vHSV) 
{
  float r, g, b, h, s, v; //this function works with floats between 0 and 1
  float f, p, q, t;
  int i;
  ColorRGB *colorRGB=(ColorRGB *)vRGB;
  ColorHSV *colorHSV=(ColorHSV *)vHSV;

  h = (float)(colorHSV->h / 256.0);
  s = (float)(colorHSV->s / 256.0);
  v = (float)(colorHSV->v / 256.0);

  //if saturation is 0, the color is a shade of grey
  if(s == 0.0) {
    b = v;
    g = b;
    r = g;
  }
  //if saturation > 0, more complex calculations are needed
  else
  {
    h *= 6.0; //to bring hue to a number between 0 and 6, better for the calculations
    i = (int)(floor(h)); //e.g. 2.7 becomes 2 and 3.01 becomes 3 or 4.9999 becomes 4
    f = h - i;//the fractional part of h

    p = (float)(v * (1.0 - s));
    q = (float)(v * (1.0 - (s * f)));
    t = (float)(v * (1.0 - (s * (1.0 - f))));

    switch(i)
    {
      case 0: r=v; g=t; b=p; break;
      case 1: r=q; g=v; b=p; break;
      case 2: r=p; g=v; b=t; break;
      case 3: r=p; g=q; b=v; break;
      case 4: r=t; g=p; b=v; break;
      case 5: r=v; g=p; b=q; break;
      default: r = g = b = 0; break;
    }
  }
  colorRGB->r = (int)(r * 255.0);
  colorRGB->g = (int)(g * 255.0);
  colorRGB->b = (int)(b * 255.0);
}

unsigned int RGBtoINT(void *vRGB)
{
  ColorRGB *colorRGB=(ColorRGB *)vRGB;

  return (((unsigned int)colorRGB->r)<<16) + (((unsigned int)colorRGB->g)<<8) + (unsigned int)colorRGB->b;
}


float
dist(float a, float b, float c, float d) 
{
  return sqrt((c-a)*(c-a)+(d-b)*(d-b));
}


void
plasma_morph()
{
  unsigned char x,y;
  float value;
  ColorRGB colorRGB;
  ColorHSV colorHSV;

  for(x = 0; x < ColorduinoScreenWidth; x++) {
    for(y = 0; y < ColorduinoScreenHeight; y++)
      {
	value = sin(dist(x + paletteShift, y, 128.0, 128.0) / 8.0)
	  + sin(dist(x, y, 64.0, 64.0) / 8.0)
	  + sin(dist(x, y + paletteShift / 7, 192.0, 64) / 7.0)
	  + sin(dist(x, y, 192.0, 100.0) / 8.0);
	colorHSV.h=(unsigned char)((value) * 128)&0xff;
	colorHSV.s=255; 
	colorHSV.v=255;
	HSVtoRGB(&colorRGB, &colorHSV);
	
	Colorduino.SetPixel(x, y, colorRGB.r, colorRGB.g, colorRGB.b);
      }
  }
  paletteShift++;

  Colorduino.FlipPage(); // swap screen buffers to show it
}

/********************************************************
Name: ColorFill
Function: Fill the frame with a color
Parameter:R: the value of RED.   Range:RED 0~255
          G: the value of GREEN. Range:RED 0~255
          B: the value of BLUE.  Range:RED 0~255
********************************************************/
void ColorFill(unsigned char R,unsigned char G,unsigned char B)
{
  unsigned char i,j;
  
  for (i = 0;i<ColorduinoScreenWidth;i++) {
    for(j = 0;j<ColorduinoScreenHeight;j++) {
      PixelRGB *p = Colorduino.GetPixel(i,j);
      p->r = R;
      p->g = G;
      p->b = B;
    }
  }
  
  Colorduino.FlipPage();
}

void setup()
{
  Colorduino.Init(); // initialize the board

  serialBuffer.buffer = buffer;
  serialBuffer.bufferSize = BUFFER_SIZE;
  serialBuffer.reset();
  SERIAL_PORT.begin(BAUD_RATE);
  
  // compensate for relative intensity differences in R/G/B brightness
  // array of 6-bit base values for RGB (0~63)
  // whiteBalVal[0]=red
  // whiteBalVal[1]=green
  // whiteBalVal[2]=blue
  unsigned char whiteBalVal[3] = {36,63,63}; // for LEDSEE 6x6cm round matrix
  Colorduino.SetWhiteBal(whiteBalVal);

  // plasma
  paletteShift=128000;
  
  currentMode = MODE_PLASMA;
}

uint16_t checksum(byte* buffer, int len) {
  uint16_t sum1 = 0;
  uint16_t sum2 = 0;
  for (int i = 0; i < len; i++) {
    sum1 = (sum1 + buffer[i]) % 255;
    sum2 = (sum2 + sum1) % 255;
  }
  return (sum2 << 8) | sum1;
}

bool verify(byte* buffer, int len) {
  uint16_t actual = checksum(buffer, len - 2);
  uint16_t expected = (buffer[len - 2] << 8) | buffer[len - 1];
  return actual == expected;
}

void loop()
{

  int maxBytes = SERIAL_PORT.available();
  while (maxBytes--) {
    byte inputByte = SERIAL_PORT.read();
    int bufferStatus = serialBuffer.receive(inputByte);
    if (bufferStatus >= 0) {
      byte id = buffer[0];
      byte c = buffer[1];
      byte sum1 = buffer[bufferStatus - 1];
      byte sum2 = buffer[bufferStatus - 2];
      bool verified = verify(buffer, bufferStatus);
      /*
      Serial.print("received cmd ");
      Serial.print(c, HEX);
      Serial.print(", id: ");
      Serial.print(id, DEC);
      Serial.print(", len: ");
      Serial.print(bufferStatus - 2, DEC);
      Serial.print(", verified: ");
      Serial.println(verified);
      */
      if (verified) {
        switch (c) {
        case CMD_NEW_ANIMATION:
          cmd_new_animation(bufferStatus - 4, buffer + 2);
          break;
        case CMD_NEW_FRAME:
          cmd_new_frame(bufferStatus - 4, buffer + 2);
          break;
        case CMD_APPEND_FRAME:
          cmd_append_frame_data(bufferStatus - 4, buffer + 2);
          break;
        case CMD_START_ANIMATION:
          cmd_start_animation(bufferStatus - 4, buffer + 2);
          break;
        case CMD_PLASMA:
          currentMode = MODE_PLASMA;
          break;
        }
        Serial.write(id);
      }
    }
  }

  switch (currentMode) {
    case MODE_PLAY_ANIMATION:
    {
      if (--frameDuration <= 0) {
        // next frame
        frameCurrent = frameCurrent->nextFrame;
        if (frameCurrent == NULL)
          frameCurrent = frameFirst;
        frameDuration = frameCurrent->duration;
      }
      
      byte index = 0;
      for (byte i = 0; i < 8; i++) {
        for(byte j = 0; j < 8; j++) {
          ColorRGB rgb = frameCurrent->pixels[index++];
          Colorduino.SetPixel(i, j, rgb.r, rgb.g, rgb.b);
        }
      }
      Colorduino.FlipPage();
      delay(1);
      break;
    }
    case MODE_PLASMA:
      plasma_morph();
      break;
    case MODE_STATIC:
      break;
  }
}
