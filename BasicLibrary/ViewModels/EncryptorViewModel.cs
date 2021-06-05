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

namespace BasicLibrary.ViewModels
{
    public class EncryptorViewModel : MvxViewModel
    {
        #region Interfaces

        private readonly IMvxNavigationService _navigationService;
        public IMvxCommand SwitchViewToDecoderCommand { get; set; }
        public IMvxCommand EncryptMessageCommand { get; set; }
        public IMvxCommand HideMessageCommand { get; set; }
        public IMvxCommand LoadMessageCommand { get; set; }
        public IMvxCommand SaveMessageCommand { get; set; }


        #endregion Interfaces


        #region ctor
        public EncryptorViewModel(IMvxNavigationService mvxNavigationService)
        {
            _navigationService = mvxNavigationService;

            SwitchViewToDecoderCommand = new MvxCommand(SwitchViewToDecoder);
            EncryptMessageCommand = new MvxCommand(EncryptMessage);
            //HideMessageCommand = new MvxCommand(HideMessage);
            LoadMessageCommand = new MvxCommand(LoadMessage);
            SaveMessageCommand = new MvxCommand(SaveMessage);

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
                RaisePropertyChanged("IsEnabled");
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
            { return _isEnabled = (!String.IsNullOrEmpty(MessageToHide)) && (String.IsNullOrEmpty(EncryptedMessage)); }

            set
            {
                SetProperty(ref _isEnabled, value);
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
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
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
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
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
            string fileName = "C:\\Users\\Endrju\\Desktop\\Red_Image.jpg";
            bmp = (Bitmap)Image.FromFile(fileName);
            bmp = ChangeColor(bmp, tabOfBytes, key, iv);
            bmp.Save("C:\\Users\\Endrju\\Desktop\\Saved_Image.jpg");
            return bmp;
        }

        //Get pixel color
        public Bitmap ChangeColor(Bitmap sourceBitmap, byte[] tabOfBytes, byte[] key, byte[] iv)
        {
            //TESTS VALUES
            int iIndexer = 500;
            int jIndexerStart = 100;
            int jIndexerEnd = 200;

            int valueDifferenceR = 30;
            int valueDifferenceG = 4;
            int valueDifferenceB = 4;

            int coordsForPixelsChangeIndexer = 0;

            int keyIndexer = 0;
            int ivIndexer = 0;

            //splitting img height for 2 parts
            int firstPartTopBorder = 0;
            int firstPartBottomBorder= sourceBitmap.Height/2-50;

            int secondPartTopBorder= sourceBitmap.Height / 2 - 48;
            int secondPartBottomBorder= sourceBitmap.Height-48;

            int separatorsCount = /*sourceBitmap.Width> tabOfBytes.Length+50?*/(sourceBitmap.Width - 50) / tabOfBytes.Length;
            int leftLimitValue = 0;
            int rightLimitValue = separatorsCount;

            //to set 2 pixels for same i value
            int even = 0;
            Color newColor;
            Color actualColor;
            Bitmap newBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);



            //setting coords for pixel changing
            List<Tuple<int, int>> coordsForPixelsChange = new List<Tuple<int, int>>();
            for (int i = 0; i < tabOfBytes.Length/2; i++)
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
                        int newR = actualColor.R - valueDifferenceR < 0 ? 0 : actualColor.R - valueDifferenceR;
                        int newG = actualColor.G - valueDifferenceG < 0 ? 0 : actualColor.G - valueDifferenceG;
                        int newB = actualColor.B - valueDifferenceB < 0 ? 0 : actualColor.B - valueDifferenceB;

                        if(tabOfBytes[coordsForPixelsChangeIndexer] > actualColor.R)
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
                        if(actualColor.R>tabOfBytes[coordsForPixelsChangeIndexer])
                        {
                            if (tabOfBytes[coordsForPixelsChangeIndexer] < actualColor.R)
                            {
                                int bytesDiff = actualColor.R - tabOfBytes[coordsForPixelsChangeIndexer]-1;
                                int mod = bytesDiff % 10;
                                int x = bytesDiff / 10;
                                newR = actualColor.R - 1;
                                newG = actualColor.G + x < 256 ? actualColor.G + x : actualColor.G - x;
                                newB = actualColor.B + mod < 256 ? actualColor.B + mod: actualColor.B - mod;
                            }
                        }
                        if(actualColor.R== tabOfBytes[coordsForPixelsChangeIndexer])
                        {
                            newB = actualColor.B<255?actualColor.B + 1:actualColor.B-1;
                        }

                        //newR =tabOfBytes[coordsForPixelsChangeIndexer];
                        newColor = Color.FromArgb(actualColor.A, Convert.ToByte(newR), Convert.ToByte(newG), Convert.ToByte(newB));
                        newBitmap.SetPixel(i, j, newColor);

                        coordsForPixelsChangeIndexer++;
                    }
                    else if (i == sourceBitmap.Width-25 && j % 8 == 0 && ivIndexer < iv.Length)
                    {
                        newColor = Color.FromArgb(actualColor.A, iv[ivIndexer], actualColor.G, actualColor.B);
                        newBitmap.SetPixel(i, j, newColor);

                        if (actualColor.R == iv[ivIndexer])
                        {
                            newColor = Color.FromArgb(actualColor.A, iv[ivIndexer], actualColor.G, actualColor.B<255? actualColor.B + 1: actualColor.B - 1);
                            newBitmap.SetPixel(i, j, newColor);
                        }
                        ivIndexer++;

                    }
                    else if (j == sourceBitmap.Height-25 && i > 50 && i < 500 && i % 10 == 0 && keyIndexer < key.Length)
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
            string encryptedMessageToShow = null;
            byte[] encryptedMessage = null;
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();

            if (!String.IsNullOrEmpty(MessageToHide))
            {
                encryptedMessage = EncryptMessageToBytes(MessageToHide, Aes.Key, Aes.IV);
                encryptedMessageToShow = Convert.ToBase64String(encryptedMessage);
            }


            MessageToHide = encryptedMessageToShow;
            ChangePixelColor(encryptedMessage, Aes.Key, Aes.IV);
        }

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
        #endregion Commands

    }
}
