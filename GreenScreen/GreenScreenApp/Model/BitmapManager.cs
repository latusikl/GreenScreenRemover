using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;


namespace GreenScreen.Model
{
    class BitmapManager
    {
        private static byte MaxColor(byte value, double compatibilityRation)
        {
            double maxColorDouble = value + value * (1 - compatibilityRation);
            
            if (maxColorDouble >= 255)
                return (byte) 255;

            return (byte) maxColorDouble;
        }

        private static byte MinColor(byte value, double compatibilityRation)
        {
            double minColorDouble = value * compatibilityRation;
            return (byte) minColorDouble;
        }

        private static bool IsColorGood(byte value, byte min, byte max)
        {
            return (value >= min && value <= max);
        }

        //Save image to file
        public static void SaveImageToFile(string pathToSave, Bitmap bitmap)
        {
            //If not null
            if (string.IsNullOrEmpty(pathToSave) || bitmap is null)
                return;
           
            var extension = Path.GetExtension(pathToSave);
            //Save in chosen format
            switch (extension)
            {
                case ".jpg":
                    bitmap.Save(pathToSave, ImageFormat.Jpeg);
                    break;
                case ".bmp":
                    bitmap.Save(pathToSave, ImageFormat.Bmp);
                    break;
                case ".png":
                    bitmap.Save(pathToSave, ImageFormat.Png);
                    break;
            }
        }
        //Required format to show result in GUI
        public static BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            MemoryStream memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, ImageFormat.Png);
            memoryStream.Position = 0;
            
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            
            return bitmapImage;
        }

        public static byte[] process(byte[] pixelArray, byte[] colorRgbBytes)
        {
            if (colorRgbBytes.Length != 3 || pixelArray is null || colorRgbBytes is null)
                return null;

                double ratio = 0.01;
                byte red = colorRgbBytes[0];
                byte green = colorRgbBytes[1];
                byte blue = colorRgbBytes[2];

                byte minRed = MinColor(red, ratio);
                byte maxRed = MaxColor(red, ratio);
                
                byte minGreen = MinColor(green, ratio);
                byte maxGreen = MaxColor(green, ratio);

                byte minBlue = MinColor(blue, ratio);
                byte maxBlue = MaxColor(blue, ratio);

            for (int i = 0; i < pixelArray.Length; i += 4)
            {
                    if(IsColorGood(pixelArray[i+1],minRed,maxRed) &&
                       IsColorGood(pixelArray[i+2],minGreen,maxGreen) &&
                       IsColorGood(pixelArray[i+3],minBlue,maxBlue)
                    )
//                    if ( pixelArray[i+1] == red &&
//                         pixelArray[i+2] == green &&
//                         pixelArray[i+3] == blue)
                    {
                        pixelArray[i] = 0; //A
                        pixelArray[i + 1] = 0; //R
                        pixelArray[i + 2] = 0; //G
                        pixelArray[i + 3] = 0; //B
                    }
            }
            return pixelArray;
        }
        //Convert pixels from byte array to bitmap object
        public static Bitmap ToOutputBitmap(byte[] pixelArray, int width, int height)
        {
            Bitmap outputBitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            int pixelIndex = 0;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    var pixelColor = Color.FromArgb(pixelArray[pixelIndex], pixelArray[pixelIndex + 1], pixelArray[pixelIndex + 2], pixelArray[pixelIndex + 3]);
                    outputBitmap.SetPixel(j, i, pixelColor);
                    pixelIndex += 4;
                }
            }
            return outputBitmap;
        }
        //Gets pixel ARGB values as Byte array
        public static byte[] ToPixelArray(Bitmap inputBitmap)
        {
            int width = inputBitmap.Width;
            int height = inputBitmap.Height;
            int arraySize = width * height * 4;

            byte[] pixelArray = new byte[arraySize];

            int pixelIndex = 0;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    pixelArray[pixelIndex] = inputBitmap.GetPixel(j, i).A;
                    pixelArray[pixelIndex + 1] = inputBitmap.GetPixel(j, i).R;
                    pixelArray[pixelIndex + 2] = inputBitmap.GetPixel(j, i).G;
                    pixelArray[pixelIndex + 3] = inputBitmap.GetPixel(j, i).B;
                 
                    pixelIndex += 4;
                }
            }
            return pixelArray;
        }

        public static byte[] GetRgbColorBytes(string htmlColor)
        {
            if (htmlColor != null)
            {
                var color = (System.Windows.Media.Color) System.Windows.Media.ColorConverter.ConvertFromString(htmlColor);
                byte[] colorRgbBytes = new byte[3];
                colorRgbBytes[0] = color.R;
                colorRgbBytes[1] = color.G;
                colorRgbBytes[2] = color.B;
               
                return colorRgbBytes;
            }
            else
            {
                return null;
            }
        }
    }
}

