using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amib.Threading;

namespace GreenScreen.Model
{
    class ThreadsManager
    {


        
        
        private static int CalculateArrayElementsAmount(int threadsAmount, byte[] pixelArray)
        {
            int singleArraySize = 102400; //25600 pixel per Array

            if (pixelArray.Length < threadsAmount)
                return pixelArray.Length;
           
            while (pixelArray.Length < singleArraySize * threadsAmount)
            {
                singleArraySize -= 4;
            }


            if (singleArraySize <= 0)
                return pixelArray.Length;


            return singleArraySize;
        }

        public static byte[] MergeArray(List<byte[]> splitPixelArray)
        {
            byte[] newPixelArray = new byte[splitPixelArray.Sum(array => array.Length)];
            int offset = 0;

            foreach (var elem in splitPixelArray)
            {
                elem.CopyTo(newPixelArray,offset);
                offset += elem.Length;
            }

            return newPixelArray;
        }

        public static List<byte[]> SplitPixelArray(byte[] pixelArray, int threadsAmount)
        {
            List<byte[]> splitPixelArray = new List<byte[]>();

            int arraysElementsAmount = CalculateArrayElementsAmount(threadsAmount, pixelArray);
            double arraysAmountDouble = pixelArray.Length / (double)arraysElementsAmount;
            int arraysAmount = (int)Math.Floor(arraysAmountDouble);
            int startIndex = 0;
            for (int i = 0; i < arraysAmount ; i++)
            {
                byte[]splitArray = new byte[arraysElementsAmount];
                Array.Copy(pixelArray,startIndex,splitArray,0,arraysElementsAmount);
                splitPixelArray.Add(splitArray);
                startIndex+=arraysElementsAmount;
            }

            int leftovers = pixelArray.Length - (arraysAmount * arraysElementsAmount);
            if (leftovers > 0)
            {
                byte[] splitArray = new byte[leftovers];
                Array.Copy(pixelArray, startIndex, splitArray, 0, leftovers);
                splitPixelArray.Add(splitArray);
            }
            return splitPixelArray;
        }
        
        public static void RunThreads(Action<byte[],byte[],int> function, List<byte[]> arrayList, byte[] arrayRgbColorBytes, int threadsAmount)
        {

            SmartThreadPool smartThreadPool = new SmartThreadPool(60 * 1000, threadsAmount, threadsAmount);


            foreach (var byteArray in arrayList)
            {
                smartThreadPool.QueueWorkItem(function, byteArray, arrayRgbColorBytes, byteArray.Length);
            }

            smartThreadPool.Start();
            smartThreadPool.WaitForIdle();
            smartThreadPool.Shutdown();

        }
    }
}
