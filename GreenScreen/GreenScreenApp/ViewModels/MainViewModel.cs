using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using GreenScreen.Model;
using Microsoft.Win32;
using MessageBox = System.Windows.MessageBox;

enum TypeOfDll
{
    Assembler,
    Cpp,
    NotChosen
};

namespace GreenScreen.ViewModels
{
    public class MainViewModel: Screen
    {
        //C++ function export from Dll.
        [DllImport("Cpp.dll")]
        public static extern unsafe void processPicture(byte* pixelArray, byte* colorRgbBytes,int size);

        //Assembler function export from Dll
        [DllImport("Assembler.dll")]
       public static extern unsafe void processPictureAssembler(byte* pixelArray, byte* colorRgbBytes, int size);

        //Variables storing buttons selection state
        private bool _isAssemblerDllSelectable = true;
        private bool _isCppDllSelectable = true;

        private string _chosenColor;
        private int _threadsAmount=1;

        private TypeOfDll _dllType=TypeOfDll.NotChosen;
        private string _inputPicturePath;

        //Dll execution time
        private string _executionTime;

        //Stores picture from input and for output
        private ImageHolder _imageHolder;

        //Bind threads amount
        public int ThreadsAmount
        {
            get => _threadsAmount;
            set
            {
                _threadsAmount = value;
                NotifyOfPropertyChange(() => ThreadsAmount);
                NotifyOfPropertyChange(() => ThreadsText);
            }
        }

        //Image to show in GUI after running algorithm
        private BitmapImage _bitmapImage;

        //Bind execution time to TextBox
        public string ExecutionTime
        {
            get => _executionTime;
            set
            {
                _executionTime = value;
                NotifyOfPropertyChange(() => ExecutionTime);
            }
        }

        //Bind image frame in xaml which informs element and pass input path
        public string InputPicturePath
        {
            get => _inputPicturePath;
            set
            {
                _inputPicturePath = value;
                NotifyOfPropertyChange(() => InputPicturePath);
            }
        }

        //Bind ColorPicker SelectedColor field 
        public string ChosenColor
        {
            get => _chosenColor;
            set
            {
                _chosenColor = value;
                NotifyOfPropertyChange(() => ChosenColor);
            }
        }

        //Bind threads amount text field
        public string ThreadsText => ThreadsAmount.ToString();

        //Stores BitmapImage object required for xaml file
        //To show algorithm result before saving image
        public BitmapImage BitmapImage
        {
            get => _bitmapImage;
            set
            {
                _bitmapImage = value;
                NotifyOfPropertyChange(() => BitmapImage);
            }
        }



        private void RunCppDll(byte[] pixelArray, byte[] colorToRemoveRgb, int size)
        {
            unsafe
            {
                fixed (byte* colorToRemoveRgbPtr = &colorToRemoveRgb[0])
                {
                    fixed (byte* pixelArrayPtr = &pixelArray[0])
                    { 
                        processPicture(pixelArrayPtr,colorToRemoveRgbPtr,size);
                    }
                }
            }
        }

        private void RunAsmDll(byte[] pixelArray, byte[] colorToRemoveRgb, int size)
        {
            unsafe
            {
                fixed (byte* colorToRemoveRgbPtr = &colorToRemoveRgb[0])
                {
                    fixed (byte* pixelArrayPtr = &pixelArray[0])
                    {
                        processPictureAssembler(pixelArrayPtr, colorToRemoveRgbPtr, size);
                    }
                }
            }
        }

        private void ShowTime(TimeSpan timeSpan)
        {
            string elapsedTime = String.Format("{0:00}:{1:000}", timeSpan.Seconds,timeSpan.Milliseconds);
            elapsedTime += " sec";
            ExecutionTime = elapsedTime;
        }

//Button responses

        public void OpenBmp()
        {
            //Creating system open dialog
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.bmp;*.jpg;*.png;|JPG|*.jpg|BMP|*.bmp|PNG|*.png",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };
            openFileDialog.ShowDialog();

            if (String.IsNullOrEmpty(openFileDialog.FileName))
            {
                MessageBox.Show("File hasn't been selected!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                InputPicturePath = openFileDialog.FileName;
                _imageHolder = new ImageHolder {InputImage = new Bitmap(InputPicturePath)};
            }
        }

        public void SavePicture()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Image Files|*.bmp;*.jpg;*.png;|JPG|*.jpg|BMP|*.bmp|PNG|*.png",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };
            saveFileDialog.ShowDialog();

            BitmapManager.SaveImageToFile(saveFileDialog.FileName,_imageHolder.OutputImage);
        }

        //Function checks if C++ Dll button can be selected.
        //Name according to Caliburn.Micro framework conversion.
        public bool CanAssemblerDllSelect
        {
            get => _isAssemblerDllSelectable;
            set
            {
                _isAssemblerDllSelectable = value;
                _isCppDllSelectable = !value;
                NotifyOfPropertyChange(() => CanAssemblerDllSelect);
                NotifyOfPropertyChange(() => CanCppDllSelect);
            }
        }
        //Function checks if Assembler Dll button can be selected.
        //Name according to Caliburn.Micro framework conversion.
        public bool CanCppDllSelect
        {
            get => _isCppDllSelectable;
            set
            {
                _isCppDllSelectable = value;
                _isAssemblerDllSelectable = !value;
                NotifyOfPropertyChange(() => CanAssemblerDllSelect);
                NotifyOfPropertyChange(() => CanCppDllSelect);
            }
        }

        //Button response "Assembler DLL"
        public void AssemblerDllSelect()
        {
            _dllType = TypeOfDll.Assembler;
            CanAssemblerDllSelect = false;
        }
        //Button response "C++ DLL"
        public void CppDllSelect()
        {
            _dllType = TypeOfDll.Cpp;
            CanCppDllSelect = false;
        }
        
        //Button response "RUN"
        public void RunAlgorithm()
        {
            if (_dllType == TypeOfDll.NotChosen)
            {
                MessageBox.Show("Please select library.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (_imageHolder?.InputImage is null)
            {
                MessageBox.Show("Please select picture.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (_chosenColor is null)
            {
                MessageBox.Show("Please select color to remove.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                _imageHolder.PixelArray = BitmapManager.ToPixelArray(_imageHolder.InputImage);
                byte[] arrayRgbColorBytes = BitmapManager.GetRgbColorBytes(_chosenColor);
                TimeSpan threadsRunTime = new TimeSpan(0, 0, 0);

                Stopwatch stopWatch = new Stopwatch();
                if (ThreadsAmount > 1)
                {
                    List<byte[]> arrayList = ThreadsManager.SplitPixelArray(_imageHolder.PixelArray, ThreadsAmount);



                    if (_dllType == TypeOfDll.Cpp)
                    { 
                        stopWatch.Start();
                        ThreadsManager.RunThreads(new Action<byte[], byte[], int>(this.RunCppDll),
                            arrayList, arrayRgbColorBytes, ThreadsAmount);
                        stopWatch.Stop();

                    }
                    else if (_dllType == TypeOfDll.Assembler)
                    {
                        stopWatch.Start();
                        ThreadsManager.RunThreads(new Action<byte[], byte[], int>(this.RunAsmDll),
                            arrayList, arrayRgbColorBytes, ThreadsAmount);
                        stopWatch.Stop();
                    }
                    _imageHolder.PixelArray = ThreadsManager.MergeArray(arrayList);

                }
                else
                {
                    
                    stopWatch.Start();
                    if (_dllType == TypeOfDll.Cpp)
                    {
                        RunCppDll(_imageHolder.PixelArray,arrayRgbColorBytes,_imageHolder.GetPixelArraySize());

                    }
                    else if (_dllType == TypeOfDll.Assembler)
                    {
                        RunAsmDll(_imageHolder.PixelArray, arrayRgbColorBytes, _imageHolder.GetPixelArraySize());
                    }
                    stopWatch.Stop();

                    
                }
                threadsRunTime = stopWatch.Elapsed;
                ShowTime(threadsRunTime);

               
                _imageHolder.OutputImage = BitmapManager.ToOutputBitmap(_imageHolder.PixelArray, _imageHolder.GetInputWidth(), _imageHolder.GetInputHeight());

                BitmapImage = BitmapManager.ToBitmapImage(_imageHolder.OutputImage);
            }
        }
    }
}
