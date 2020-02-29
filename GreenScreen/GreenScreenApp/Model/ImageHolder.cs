using System.Drawing;


namespace GreenScreen.Model
{
    class ImageHolder
    {
        public Bitmap InputImage { get; set; }

        public Bitmap OutputImage { get; set; }

        public byte[] PixelArray { get; set; }

        public int GetPixelArraySize()
        {
            return PixelArray.Length;
        }
        public int GetInputHeight()
        {
            return InputImage.Height;
        }

        public int GetInputWidth()
        {
            return InputImage.Width;
        }
        
    }
}