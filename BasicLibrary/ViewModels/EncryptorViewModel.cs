using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using MvvmCross.ViewModels;
using System.Drawing;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using BasicLibrary.Converters;
using MvvmCross.Converters;
using System.Windows;
using System.Diagnostics;

namespace BasicLibrary.ViewModels
{
    public class EncryptorViewModel : MvxViewModel
    {
        #region Interfaces

        private readonly IMvxNavigationService _navigationService;
        public IMvxCommand SwitchViewToDecoderCommand { get; set; }
        public IMvxCommand EncryptMessageCommand { get; set; }
        public IMvxCommand LoadMessageCommand { get; set; }
        public IMvxCommand SaveMessageCommand { get; set; }
        public IMvxCommand LoadImageCommand { get; set; }
        public IMvxCommand SaveImageCommand { get; set; }


        #endregion Interfaces


        #region ctor
        public EncryptorViewModel(IMvxNavigationService mvxNavigationService)
        {
            _navigationService = mvxNavigationService;

            SwitchViewToDecoderCommand = new MvxCommand(SwitchViewToDecoder);
            EncryptMessageCommand = new MvxCommand(EncryptMessage);
            LoadMessageCommand = new MvxCommand(LoadMessage);
            SaveMessageCommand = new MvxCommand(SaveMessage);
            LoadImageCommand = new MvxCommand(LoadImage);
            SaveImageCommand = new MvxCommand(SaveImage);
            Aes = new AesCryptoServiceProvider();
        }
        #endregion ctor

        public override async Task Initialize()
        {
            await base.Initialize();
        }


        #region Binding Properties
        private string _messageToHide;

        public string MessageToHide
        {
            get
            { return _messageToHide; }

            set
            {
                SetProperty(ref _messageToHide, value);
                NumOfCharactersLeftInMessage = MaxCharactersForMessage != null && Convert.ToInt32(MaxCharactersForMessage) > 0 ? Convert.ToInt32(MaxCharactersForMessage) - (value != null ? value.Length : 0) : NumOfCharactersLeftInMessage;
                RaisePropertyChanged("NumOfCharactersLeftInMessage");

                LengthOfMessage = MessageToHide == null ? 0 : MessageToHide.Length;
                RaisePropertyChanged("LengthOfMessage");

                SaveMessageIsEnabled = String.IsNullOrEmpty(MessageToHide) ? false : true;
                RaisePropertyChanged("SaveMessageIsEnabled");

                if (!String.IsNullOrEmpty(MessageToHide) && OriginalImage != null)
                {
                    IsEnabled = true;
                    RaisePropertyChanged("IsEnabled");

                }
                else
                {
                    IsEnabled = false;
                    RaisePropertyChanged("IsEnabled");

                }
            }
        }

        private int _messageLenKeeper;

        public int MessageLenKeeper
        {
            get
            { return _messageLenKeeper; }

            set
            {
                SetProperty(ref _messageLenKeeper, value);
                RaisePropertyChanged(() => MessageLenKeeper);
            }
        }

        private bool _messageIsEnabled = false;

        public bool MessageIsEnabled
        {
            get
            { return _messageIsEnabled; }

            set
            {
                SetProperty(ref _messageIsEnabled, value);
                RaisePropertyChanged(() => MessageIsEnabled);
            }
        }

        private bool _enableEnteringMessage = false;

        public bool EnableEnteringMessage
        {
            get
            { return _enableEnteringMessage; }

            set
            {
                SetProperty(ref _enableEnteringMessage, value);
                RaisePropertyChanged(() => EnableEnteringMessage);
            }
        }

        private string _encryptedMessage;

        public string EncryptedMessage
        {
            get
            { return _encryptedMessage; }

            set
            {
                SetProperty(ref _encryptedMessage, value);
                RaisePropertyChanged(() => MessageToHide);
            }
        }

        private byte[] _encryptedMessageByte;

        public byte[] EncryptedMessageByte
        {
            get
            { return _encryptedMessageByte; }

            set
            {
                SetProperty(ref _encryptedMessageByte, value);
                RaisePropertyChanged(() => MessageToHide);
            }
        }

        private int _numOfCharactersLeftInMessage;

        public int NumOfCharactersLeftInMessage
        {
            get
            { return _numOfCharactersLeftInMessage; }

            set
            {
                MessageIsEnabled = value < 1 ? true : false;
                SetProperty(ref _numOfCharactersLeftInMessage, value);
                RaisePropertyChanged(() => NumOfCharactersLeftInMessage);
                RaisePropertyChanged("MessageIsEnabled");

            }
        }

        private string _maxCharactersForMessage;

        public string MaxCharactersForMessage
        {
            get
            { return _maxCharactersForMessage; }

            set
            {
                SetProperty(ref _maxCharactersForMessage, value);
                RaisePropertyChanged(() => MaxCharactersForMessage);
            }
        }

        private int _lengthOfMessage;
        public int LengthOfMessage
        {
            get
            { return _lengthOfMessage; }

            set
            {
                SetProperty(ref _lengthOfMessage, value);

                if (OriginalImage != null && ImgWidth < 800 && LengthOfMessage > 400)
                {
                    WidthColor = "Red";
                    RaisePropertyChanged("WidthColor");

                }
                else if (OriginalImage != null && (ImgWidth < 1100) && (LengthOfMessage >= 400 && LengthOfMessage < 500))
                {
                    WidthColor = "Red";
                    RaisePropertyChanged("WidthColor");

                }
                else if (OriginalImage != null && (ImgWidth < 1400) && (LengthOfMessage >= 500 && LengthOfMessage <= 600))
                {
                    WidthColor = "Red";
                    RaisePropertyChanged("WidthColor");

                }
                else
                {
                    if (OriginalImage != null)
                    {
                        WidthColor = "Green";
                        RaisePropertyChanged("WidthColor");
                    }
                    else
                    {
                        WidthColor = "Black";
                        RaisePropertyChanged("WidthColor");
                    }


                }


                if (OriginalImage != null && ImgHeight < 500 && LengthOfMessage > 400)
                {
                    HeightColor = "Red";
                    RaisePropertyChanged("HeightColor");

                }
                else
                {
                    if (OriginalImage != null)
                    {
                        HeightColor = "Green";
                        RaisePropertyChanged("HeightColor");
                    }
                    else
                    {
                        HeightColor = "Black";
                        RaisePropertyChanged("HeightColor");
                    }

                }


            }
        }

        private string _imageSourcePath;

        public string ImageSourcePath
        {
            get
            { return _imageSourcePath; }

            set
            {
                SetProperty(ref _imageSourcePath, value);
            }
        }


        private bool _isEnabled;

        public bool IsEnabled
        {
            get
            { return _isEnabled; }

            set
            {
                SetProperty(ref _isEnabled, value);

            }
        }

        private bool _saveMessageIsEnabled;

        public bool SaveMessageIsEnabled
        {
            get
            { return _saveMessageIsEnabled; }

            set
            {
                SetProperty(ref _saveMessageIsEnabled, value);

            }
        }

        private bool _saveImageIsEnabled;

        public bool SaveImageIsEnabled
        {
            get
            { return _saveImageIsEnabled; }

            set
            {
                SetProperty(ref _saveImageIsEnabled, value);

            }
        }

        private Bitmap _originalImage;
        public Bitmap OriginalImage
        {
            get
            { return _originalImage; }

            set
            {
                SetProperty(ref _originalImage, value);
                ImgHeight = OriginalImage != null ? OriginalImage.Height : ImgHeight;
                RaisePropertyChanged("ImgHeight");

                ImgWidth = OriginalImage != null ? OriginalImage.Width : ImgWidth;
                RaisePropertyChanged("ImgWidth");
                RaisePropertyChanged("LengthOfMessage");

                if (OriginalImage != null && !String.IsNullOrEmpty(MessageToHide))
                {
                    IsEnabled = true;
                    RaisePropertyChanged("IsEnabled");

                }
                else
                {
                    IsEnabled = false;
                    RaisePropertyChanged("IsEnabled");

                }
            }
        }

        private Bitmap _imageWithHiddenMessage;
        public Bitmap ImageWithHiddenMessage
        {
            get
            { return _imageWithHiddenMessage; }

            set
            {
                SetProperty(ref _imageWithHiddenMessage, value);
                SaveImageIsEnabled = ImageWithHiddenMessage == null ? false : true;
                RaisePropertyChanged("SaveImageIsEnabled");

            }
        }

        private string _originalImageSourcePath;
        public string OriginalImageSourcePath
        {
            get
            { return _originalImageSourcePath; }

            set
            {
                if (OriginalImageSourcePath != value)
                {
                    SetProperty(ref _originalImageSourcePath, value);
                    RaisePropertyChanged("OriginalImage");
                }
            }
        }

        private string _imageWithHiddenMessageSourcePath;
        public string ImageWithHiddenMessageSourcePath
        {
            get
            { return _imageWithHiddenMessageSourcePath; }

            set
            {
                if (_imageWithHiddenMessageSourcePath != value)
                {
                    SetProperty(ref _imageWithHiddenMessageSourcePath, value);
                }

            }
        }

        private int _imgHeight;
        public int ImgHeight
        {
            get
            { return _imgHeight; }

            set
            {
                if (_imgHeight != value)
                {
                    SetProperty(ref _imgHeight, value);
                }

            }
        }

        private int _imgWidth;
        public int ImgWidth
        {
            get
            { return _imgWidth; }

            set
            {
                if (_imgWidth != value)
                {
                    SetProperty(ref _imgWidth, value);
                }

            }
        }

        private string _heightColor = "Black";
        public string HeightColor
        {
            get
            {
                return _heightColor;
            }
            set
            {
                if (_heightColor != value)
                {
                    SetProperty(ref _heightColor, value);
                }

            }
        }

        private string _widthColor = "Black";
        public string WidthColor
        {
            get
            {

                return _widthColor;
            }

            set
            {
                if (_widthColor != value)
                {
                    SetProperty(ref _widthColor, value);
                }

            }
        }

        #endregion Binding Properties

        #region Fields
        public AesCryptoServiceProvider Aes { get; set; }

        #endregion Fields



        #region Methods

        public byte[] EncryptMessageToBytes(string messageToEncrypt, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (messageToEncrypt == null || messageToEncrypt.Length <= 0)
                throw new ArgumentNullException("messageToEncrypt");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;

            MessageLenKeeper = messageToEncrypt.Length;
            // Create an AesCryptoServiceProvider object
            // with the specified key and IV.
            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt, new UTF8Encoding()))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(messageToEncrypt);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            /*int counter = 0;
            List<byte> catched = new List<byte>();
            foreach (byte b in encrypted)
            {
                for (int i = 0; i < encrypted.Length; i++)
                {
                    if (b == encrypted[i] && Array.IndexOf(encrypted, b) != i)
                    {
                        counter += 1;
                        catched.Add(b);
                    }
                }
            }*/
            return encrypted;
        }


        //Change pixel color
        public Bitmap ChangePixelColor(byte[] tabOfBytes, byte[] key, byte[] iv)
        {
            Bitmap bmp;
            string fileName = OriginalImageSourcePath;
            bmp = (Bitmap)Image.FromFile(fileName);
            bmp = ChangeColor(bmp, tabOfBytes, key, iv);
            ImageWithHiddenMessage = bmp;
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string fName = "ImageWithMessage.jpg";
            string fullPathToFile = path + "\\" + fName;
            if (bmp != null)
            {
                MessageBox.Show("Message hidden successfully.\nImage saved as " + fName + " on desktop", "Message hide", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.Yes);

            }
            else
            {
                return bmp;

            }
            if (File.Exists(fullPathToFile))
            {
                ImageWithHiddenMessage = null;
                File.Delete(fullPathToFile);
            }

            bmp.Save(fullPathToFile);
            ImageWithHiddenMessageSourcePath = fullPathToFile;
            return bmp;
        }

        public byte[] CalculateRGBFromMessage(byte valueR, byte valueG, byte valueB, byte encryptedMessageByte)
        {
            int newR;
            int newG;
            int newB;

            if (encryptedMessageByte > valueR)
            {
                int bytesDiff = encryptedMessageByte - valueR;
                int mod = bytesDiff % 10;
                int x = bytesDiff / 10;
                newR = valueR + x;
                newG = valueG + mod < 256 ? valueG + mod : valueG - mod;
                newB = valueB;
            }
            else if (encryptedMessageByte < valueR)
            {
                int bytesDiff = valueR - encryptedMessageByte - 1;
                int mod = bytesDiff % 10;
                int x = bytesDiff / 10;
                newR = valueR - 1;
                newG = valueG + x < 256 ? valueG + x : valueG - x;
                newB = valueB + mod < 256 ? valueB + mod : valueB - mod;
            }
            else
            {
                newR = valueR;
                newG = valueG;
                newB = valueB < 255 ? valueB + 1 : valueB - 1;
            }

            return new byte[] { Convert.ToByte(newR), Convert.ToByte(newG), Convert.ToByte(newB) };
        }

        //Get pixel color
        public Bitmap ChangeColor(Bitmap sourceBitmap, byte[] encryptedMessageInBytes, byte[] key, byte[] iv)
        {
            ImgHeight = sourceBitmap.Height;
            ImgWidth = sourceBitmap.Width;

            int coordsForPixelsChangeIndexer = 0;

            int keyIndexer = 0;
            int ivIndexer = 0;

            Color newColor;
            Color actualColor;
            Bitmap newBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);


            int bottomSpot = sourceBitmap.Height - 4;
            int rightSpot = sourceBitmap.Width - 4;
            int pixelCountInArea = bottomSpot * rightSpot;
            int availablePixelsCount = pixelCountInArea % 2 == 0 ? pixelCountInArea / 2 : pixelCountInArea / 2 + 1;

            int msgLen = encryptedMessageInBytes.Length;
            MaxCharactersForMessage = availablePixelsCount.ToString();
            if (sourceBitmap.Width < 70 || sourceBitmap.Height < 36)
            {
                MessageBox.Show("Image's resolution has to be 70 x 36 at least", "Invalid size", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes);
                return null;
            }

            //Walking through Image's pixels loop
            for (int i = 0; i < sourceBitmap.Width; i++)
            {
                for (int j = 0; j < sourceBitmap.Height; j++)
                {
                    //color of specific pixel image
                    actualColor = sourceBitmap.GetPixel(i, j);

                    if (i < rightSpot && j < bottomSpot && msgLen > coordsForPixelsChangeIndexer && i % 2 == 0 && j % 2 == 0)
                    {

                        byte[] valuesRGB = CalculateRGBFromMessage(actualColor.R, actualColor.G, actualColor.B, encryptedMessageInBytes[coordsForPixelsChangeIndexer]);

                        //setting new color on new bitmap
                        newColor = Color.FromArgb(actualColor.A, valuesRGB[0], valuesRGB[1], valuesRGB[2]);
                        newBitmap.SetPixel(i, j, newColor);

                        coordsForPixelsChangeIndexer++;

                    }
                    else if (i < rightSpot && j < bottomSpot && msgLen > coordsForPixelsChangeIndexer && i % 2 == 1 && j % 2 == 1)
                    {

                        byte[] valuesRGB = CalculateRGBFromMessage(actualColor.R, actualColor.G, actualColor.B, encryptedMessageInBytes[coordsForPixelsChangeIndexer]);

                        //setting new color on new bitmap
                        newColor = Color.FromArgb(actualColor.A, valuesRGB[0], valuesRGB[1], valuesRGB[2]);
                        newBitmap.SetPixel(i, j, newColor);

                        coordsForPixelsChangeIndexer++;

                    }
                    //Setting Initial Vector byte array
                    else if (i == sourceBitmap.Width - 2 && j % 2 == 0 && ivIndexer < iv.Length)
                    {
                        byte[] valuesRGB = CalculateRGBFromMessage(actualColor.R, actualColor.G, actualColor.B, iv[ivIndexer]);

                        newColor = Color.FromArgb(actualColor.A, valuesRGB[0], valuesRGB[1], valuesRGB[2]);
                        newBitmap.SetPixel(i, j, newColor);

                        ivIndexer++;

                    }
                    //Setting Key byte array
                    else if (j == sourceBitmap.Height - 2 && i % 2 == 0 && keyIndexer < key.Length)
                    {

                        byte[] valuesRGB = CalculateRGBFromMessage(actualColor.R, actualColor.G, actualColor.B, key[keyIndexer]);

                        newColor = Color.FromArgb(actualColor.A, valuesRGB[0], valuesRGB[1], valuesRGB[2]);
                        newBitmap.SetPixel(i, j, newColor);

                        keyIndexer++;

                    }
                    else
                    {
                        newBitmap.SetPixel(i, j, actualColor);
                    }

                }

            }

            MessageToHide = newBitmap != null ? String.Empty : MessageToHide;
            return newBitmap;
        }
        #endregion Methods

        #region Commands

        public void SwitchViewToDecoder()
        {
            _navigationService.Navigate<DecryptorViewModel>();
        }

        public void EncryptMessage()
        {

            int imgWidth = OriginalImage.Width;
            int imgHeight = OriginalImage.Height;
            int imgPixCount = imgWidth * imgHeight;



            Stopwatch sw = new Stopwatch();

            sw.Start();
            string encryptionStart = $"{DateTime.Now}: Encryption started";


            byte[] encryptedMessage = null;
            if (!String.IsNullOrEmpty(MessageToHide))
            {
                encryptedMessage = EncryptMessageToBytes(MessageToHide, Aes.Key, Aes.IV);
            }


            ChangePixelColor(encryptedMessage, Aes.Key, Aes.IV);

            int msgLen = MessageLenKeeper;
            float availablePixelsCountInPerc = (float)Convert.ToInt32(MaxCharactersForMessage) / (float)imgPixCount;
            float pixelsCountOccupiedByMessageInPerc = (float)Convert.ToInt32(msgLen) / (float)imgPixCount;

            string encryptionEnd = $"{DateTime.Now}: Encryption finished";

            sw.Stop();

            Console.WriteLine("Elapsed={0}", sw.Elapsed);

            // Create a string with a line of text.
            string resLog = $"{DateTime.Now}: Image resolution {imgWidth} X {imgHeight} px";
            string messageLenLog = $"{DateTime.Now}: Message length {msgLen} chars";
            string availablePixelsCountInPercLog = $"{DateTime.Now}: Max {availablePixelsCountInPerc * 100}% of image pixels available te by occupied by message bytes";
            string pixelsCountOccupiedByMessageInPercLog = $"{DateTime.Now}: {pixelsCountOccupiedByMessageInPerc * 100}% of image pixels occupied for message";
            string execTimeLog = $"{DateTime.Now}: Encryption elapsed time {sw.Elapsed}";
            string sep = "--------------------------------------------";
            // Set a variable to the Documents path.
            string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            // Append new lines of text to the file
            File.AppendAllLines(Path.Combine(docPath, $"ENCRYPTION_time_measurements.txt"), new string[] { Environment.NewLine, resLog, messageLenLog, availablePixelsCountInPercLog, pixelsCountOccupiedByMessageInPercLog, encryptionStart, encryptionEnd, execTimeLog, sep });
        }


        #region Loading and Saving
        public void SaveMessage()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Save your text message to txt file";
            sfd.FileName = "";
            sfd.DefaultExt = ".txt";
            sfd.Filter = "Text File (.txt)|*.txt";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = sfd.ShowDialog();


            if (result == true && !String.IsNullOrEmpty(MessageToHide))
            {
                File.WriteAllText(sfd.FileName, MessageToHide);
            }
        }

        public void LoadMessage()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Choose .txt file";
            ofd.FileName = "";
            ofd.DefaultExt = ".txt";
            ofd.Filter = "Text File (.txt)|*.txt";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = ofd.ShowDialog();


            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                StreamReader streamReader = new StreamReader(File.OpenRead(ofd.FileName));

                //read txt file
                //encryptorViewModel.MessageToHide = streamReader.ReadToEnd();
                MessageToHide = streamReader.ReadToEnd();

                //close the stream
                streamReader.Dispose();
            }

        }

        public void SaveImage()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Save your image";
            sfd.FileName = "";
            sfd.DefaultExt = ".png";
            sfd.Filter = "JPG Files (.jpg)|*.jpg|PNG Files (.png)|*.png";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = sfd.ShowDialog();


            if (result == true && OriginalImage != null)
            {
                OriginalImage.Save(sfd.FileName);
            }
        }

        public void LoadImage()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Choose image";
            ofd.FileName = "";
            ofd.DefaultExt = ".png";
            ofd.Filter = "JPG Files (.jpg)|*.jpg|PNG Files (.png)|*.png";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = ofd.ShowDialog();


            if (result == true)
            {
                //getting image's link for source
                Bitmap loadedImage = new Bitmap(Image.FromFile(ofd.FileName));

                if (loadedImage.Width < 900 || loadedImage.Height < 500)
                {
                    MessageBox.Show("Image's resolution is too small.", "Invalid size", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes);
                }
                else
                {
                    OriginalImage = loadedImage;
                    OriginalImageSourcePath = ofd.FileName;
                    LengthOfMessage = MessageToHide != null ? MessageToHide.Length : LengthOfMessage;

                    int bottomSpot = loadedImage.Height - 4;
                    int rightSpot = loadedImage.Width - 4;
                    int pixelCountInArea = bottomSpot * rightSpot;
                    int availablePixelsCount = (pixelCountInArea % 2 == 0 ? pixelCountInArea / 2 : pixelCountInArea / 2 + 1) - 30;
                    MaxCharactersForMessage = availablePixelsCount.ToString();
                    NumOfCharactersLeftInMessage = Convert.ToInt32(MaxCharactersForMessage);
                    EnableEnteringMessage = true;
                }

            }
        }

        #endregion Loading and Saving


        #endregion Commands

    }

}
