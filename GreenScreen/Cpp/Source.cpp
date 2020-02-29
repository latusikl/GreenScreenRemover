#include "Header.h"


_declspec(dllexport) void processPicture(unsigned char* pixelArray, unsigned char* colorRgbBytes, int size)
{
	for (int i = 0; i < size; i += 4)
	{
			if ( pixelArray[i+1] == colorRgbBytes[0] &&
			     pixelArray[i+2] == colorRgbBytes[1] &&
			     pixelArray[i+3] == colorRgbBytes[2])
		{
			pixelArray[i] = 0; //A
			pixelArray[i + 1] = 0; //R
			pixelArray[i + 2] = 0; //G
			pixelArray[i + 3] = 0; //B
		}
	}
}




 