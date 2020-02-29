#pragma once
#include <iostream>

extern "C" _declspec(dllexport) void processPicture(unsigned char* pixelArray, unsigned char* colorRgbBytes, int size);

