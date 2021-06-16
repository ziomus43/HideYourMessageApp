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
//using System.Windows.Media;

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
                NumOfCharactersLeftInMessage = MessageToHide == null ? 400 : 400 - value.Length;
                RaisePropertyChanged("NumOfCharactersLeftInMessage");

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

        private int _numOfCharactersLeftInMessage = 400;

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

        private string _maxCharactersForMessage = "400";

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
                SaveImageIsEnabled = ImageWithHiddenMessage==null ? false : true;
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

        public AesCryptoServiceProvider Aes { get; set; }

        #endregion Binding Properties



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
            //messageToEncrypt = "przeciwieństwie do zwykłego: „tekst, tekst, tekst”, sprawiającego, że wygląda to „zbyt czytelnie” po polsku. Wielu webmasterów i designerów używa Lorem Ipsum jako domyślnego modelu tekstu i wpisanie w internetowej wyszukiwarce ‘lorem ipsum’ spowoduje znalezienie bardzo wielu stron, które wciąż są w budowie. Wiele wersji tekstu ewoluowało i zmieniało się przez lata, czasem przez przypadek, czasem specjalnie (humorystyczne wstawki itd). dney w Virginii, przyjrzał się uważniej jednemu z najbardziej niejasnych słów w Lorem Ipsum";
            byte[] encrypted;

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
            int counter = 0;
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
            }
            return encrypted;
        }

        //For Check before making whole Decryption page
        public string DecryptMessageToString(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an AesCryptoServiceProvider object
            // with the specified key and IV.
            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt, new UTF8Encoding()))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }



        //Change pixel color
        public Bitmap ChangePixelColor(byte[] tabOfBytes, byte[] key, byte[] iv)
        {
            Bitmap bmp;
            string fileName = OriginalImageSourcePath;
            bmp = (Bitmap)Image.FromFile(fileName);
            bmp = ChangeColor(bmp, tabOfBytes, key, iv);
            ImageWithHiddenMessage = bmp;
            /*BitmapData bitmap = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);
            List<Colors> colors = new List<Colors>();   
            BitmapPalette bitmapPalette = new BitmapPalette(bmp.Palette);
            BitmapSource bitmapSource = BitmapSource.Create(
                bmp.Width,
                bmp.Height,
                bmp.HorizontalResolution,
                bmp.VerticalResolution,
                PixelFormats.Bgra32,
                bmp.Palette,
                new byte[bmp.Height * bitmap.Stride],
                bitmap.Stride);*/
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string fName = "ImageWithMessage.jpg";
            string fullPathToFile = path + "\\" + fName;

            if (File.Exists(fullPathToFile))
            {
                File.Delete(fullPathToFile);
            }

            bmp.Save(fullPathToFile);
            ImageWithHiddenMessageSourcePath = fullPathToFile;
            return bmp;
        }

        //Get pixel color
        public Bitmap ChangeColor(Bitmap sourceBitmap, byte[] tabOfBytes, byte[] key, byte[] iv)
        {
            //TESTS VALUES

            int valueDifferenceR = 30;
            int valueDifferenceG = 4;
            int valueDifferenceB = 4;

            int coordsForPixelsChangeIndexer = 0;

            int keyIndexer = 0;
            int ivIndexer = 0;

            //splitting img height for 2 parts
            int firstPartTopBorder = 0;
            int firstPartBottomBorder = sourceBitmap.Height / 2 - 50;

            int secondPartTopBorder = sourceBitmap.Height / 2 - 48;
            int secondPartBottomBorder = sourceBitmap.Height - 48;

            int separatorsCount = (sourceBitmap.Width - 50) / tabOfBytes.Length;
            int leftLimitValue = 0;
            int rightLimitValue = separatorsCount;

            //to set 2 pixels for same i value
            int even = 0;
            Color newColor;
            Color actualColor;
            Bitmap newBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);



            //setting coords for pixel changing
            List<Tuple<int, int>> coordsForPixelsChange = new List<Tuple<int, int>>();
            for (int i = 0; i < tabOfBytes.Length / 2; i++)
            {

                coordsForPixelsChange.Add(new Tuple<int, int>(new Random().Next(leftLimitValue, rightLimitValue), new Random().Next(firstPartTopBorder, firstPartBottomBorder)));
                coordsForPixelsChange.Add(new Tuple<int, int>(coordsForPixelsChange[even].Item1, new Random().Next(secondPartTopBorder, secondPartBottomBorder)));

                leftLimitValue += separatorsCount + 1;
                rightLimitValue += separatorsCount + 1;
                even += 2;
            }

            //Walking through Image's pixels loop
            for (int i = 0; i < sourceBitmap.Width; i++)
            {
                for (int j = 0; j < sourceBitmap.Height; j++)
                {
                    actualColor = sourceBitmap.GetPixel(i, j);

                    if (coordsForPixelsChangeIndexer < coordsForPixelsChange.Count
                        && i == coordsForPixelsChange[coordsForPixelsChangeIndexer].Item1
                        && j == coordsForPixelsChange[coordsForPixelsChangeIndexer].Item2)
                    {

                        //testing RGB values of pixels changing for specific difference
                        //int newR = actualColor.R - valueDifferenceR < 0 ? 0 : actualColor.R - valueDifferenceR;
                        //int newG = actualColor.G - valueDifferenceG < 0 ? 0 : actualColor.G - valueDifferenceG;
                        //int newB = actualColor.B - valueDifferenceB < 0 ? 0 : actualColor.B - valueDifferenceB;

                        int newR = actualColor.R;
                        int newG = actualColor.G;
                        int newB = actualColor.B;

                        if (tabOfBytes[coordsForPixelsChangeIndexer] > actualColor.R)
                        {
                            //reducing rgb values difference between images pixels
                            if (actualColor.R < tabOfBytes[coordsForPixelsChangeIndexer])
                            {
                                int bytesDiff = tabOfBytes[coordsForPixelsChangeIndexer] - actualColor.R;
                                int mod = bytesDiff % 10;
                                int x = bytesDiff / 10;
                                int xMultipliedBy10 = x * 10;
                                newR = actualColor.R + x;
                                newG = actualColor.G + mod < 256 ? actualColor.G + mod : actualColor.G - mod;
                            }
                        }
                        if (actualColor.R > tabOfBytes[coordsForPixelsChangeIndexer])
                        {
                            if (tabOfBytes[coordsForPixelsChangeIndexer] < actualColor.R)
                            {
                                int bytesDiff = actualColor.R - tabOfBytes[coordsForPixelsChangeIndexer] - 1;
                                int mod = bytesDiff % 10;
                                int x = bytesDiff / 10;
                                newR = actualColor.R - 1;
                                newG = actualColor.G + x < 256 ? actualColor.G + x : actualColor.G - x;
                                newB = actualColor.B + mod < 256 ? actualColor.B + mod : actualColor.B - mod;
                            }
                        }
                        if (actualColor.R == tabOfBytes[coordsForPixelsChangeIndexer])
                        {
                            newB = actualColor.B < 255 ? actualColor.B + 1 : actualColor.B - 1;
                        }

                        newColor = Color.FromArgb(actualColor.A, Convert.ToByte(newR), Convert.ToByte(newG), Convert.ToByte(newB));
                        newBitmap.SetPixel(i, j, newColor);

                        coordsForPixelsChangeIndexer++;
                    }
                    else if (i == sourceBitmap.Width - 25 && j % 16 == 0 && ivIndexer < iv.Length)
                    {
                        newColor = Color.FromArgb(actualColor.A, iv[ivIndexer], actualColor.G, actualColor.B);
                        newBitmap.SetPixel(i, j, newColor);

                        if (actualColor.R == iv[ivIndexer])
                        {
                            newColor = Color.FromArgb(actualColor.A, iv[ivIndexer], actualColor.G, actualColor.B < 255 ? actualColor.B + 1 : actualColor.B - 1);
                            newBitmap.SetPixel(i, j, newColor);
                        }
                        ivIndexer++;

                    }
                    else if (j == sourceBitmap.Height - 25 && i > 50 && i < 500 && i % 10 == 0 && keyIndexer < key.Length)
                    {
                        newColor = Color.FromArgb(actualColor.A, key[keyIndexer], actualColor.G, actualColor.B);
                        newBitmap.SetPixel(i, j, newColor);

                        if (actualColor.R == key[keyIndexer])
                        {
                            newColor = Color.FromArgb(actualColor.A, key[keyIndexer], actualColor.G, actualColor.B < 255 ? actualColor.B + 1 : actualColor.B - 1);
                            newBitmap.SetPixel(i, j, newColor);
                        }
                        keyIndexer++;

                    }
                    else
                    {
                        newBitmap.SetPixel(i, j, actualColor);
                    }

                }

            }
            if (newBitmap != null)
            {
                MessageBox.Show("Message hidden successfully.", "Message hide", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.Yes);

            }
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
            byte[] encryptedMessage = null;
            if (!String.IsNullOrEmpty(MessageToHide))
            {
                encryptedMessage = EncryptMessageToBytes(MessageToHide, Aes.Key, Aes.IV);
            }


            MessageToHide = String.Empty;
            ChangePixelColor(encryptedMessage, Aes.Key, Aes.IV);
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
                Bitmap loadedImage=new Bitmap(Image.FromFile(ofd.FileName));

                if (loadedImage.Width<900 || loadedImage.Height<500)
                {
                    MessageBox.Show("Image's resolution is too small.", "Invalid size", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes);
                }
                else
                {
                    OriginalImage = loadedImage;
                    OriginalImageSourcePath = ofd.FileName;
                }

            }
        }

        #endregion Loading and Saving


        #endregion Commands

    }

}
