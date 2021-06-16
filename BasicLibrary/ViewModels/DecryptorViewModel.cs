using Microsoft.Win32;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace BasicLibrary.ViewModels
{
    public class DecryptorViewModel : MvxViewModel, IMvxNotifyPropertyChanged
    {
        #region Interfaces

        private readonly IMvxNavigationService _navigationService;
        public IMvxCommand SwitchViewToEncoderCommand { get; set; }
        public IMvxCommand DecryptMessageToStringCommand { get; set; }
        public IMvxCommand LoadImageWithMessageCommand { get; set; }
        public IMvxCommand LoadOriginalImageCommand { get; set; }
        public IMvxCommand SaveMessageCommand { get; set; }

        #endregion Interfaces


        #region ctor
        public DecryptorViewModel(IMvxNavigationService mvxNavigationService)
        {
            _navigationService = mvxNavigationService;
            SwitchViewToEncoderCommand = new MvxCommand(SwitchViewToEncoder);
            DecryptMessageToStringCommand = new MvxCommand(() => DecryptMessageToString(new AesCryptoServiceProvider().Key, new AesCryptoServiceProvider().IV));
            LoadImageWithMessageCommand = new MvxCommand(LoadImageWithMessage);
            LoadOriginalImageCommand = new MvxCommand(LoadOriginalImage);
            SaveMessageCommand = new MvxCommand(SaveMessage);
        }
        #endregion ctor

        public override async Task Initialize()
        {
            await base.Initialize();
        }

        #region Binding Properties

        private Bitmap _originalImage;
        public Bitmap OriginalImage
        {
            get
            { return _originalImage; }

            set
            {
                SetProperty(ref _originalImage, value);
                if (OriginalImage != null && ImageWithHiddenMessage != null)
                {
                    DecryptIsEnabled = true;
                    RaisePropertyChanged("DecryptIsEnabled");

                }
                else
                {
                    DecryptIsEnabled = false;
                    RaisePropertyChanged("DecryptIsEnabled");

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
                if (OriginalImage != null && ImageWithHiddenMessage != null)
                {
                    DecryptIsEnabled = true;
                    RaisePropertyChanged("DecryptIsEnabled");

                }
                else
                {
                    DecryptIsEnabled = false;
                    RaisePropertyChanged("DecryptIsEnabled");

                }

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

        private byte[] _messageInBytes;
        public byte[] MessageInBytes
        {
            get
            { return _messageInBytes; }

            set
            {
                SetProperty(ref _messageInBytes, value);
            }
        }

        private string _hiddenMessage;
        public string HiddenMessage
        {
            get
            { return _hiddenMessage; }

            set
           {
                
                SetProperty(ref _hiddenMessage, value);
                LengthOfMessage = _hiddenMessage == null ? 0 : _hiddenMessage.Length;
                RaisePropertyChanged("LengthOfMessage");

                SaveImageIsEnabled = String.IsNullOrEmpty(HiddenMessage) ? false : true;
                RaisePropertyChanged("SaveImageIsEnabled");

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
            }
        }

        private bool _decryptIsEnabled = false;
        public bool DecryptIsEnabled
        {
            get
            { return _decryptIsEnabled; }

            set
            {
                
                SetProperty(ref _decryptIsEnabled, value);

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
        #endregion Binding Properties

        public void ChangePath()
        {
            OriginalImageSourcePath = "krzysiek";
        }

        public void SwitchViewToEncoder()
        {
            _navigationService.Navigate<EncryptorViewModel>();
        }

        public byte[] GetBytesDifference(Bitmap originalImg, Bitmap imgWithMessage, ref byte[] key, ref byte[] iv)
        {

            List<byte> bytesDifferenceMessage = new List<byte>();
            List<byte> bytesDifferenceKey = new List<byte>();
            List<byte> bytesDifferenceIV = new List<byte>();

            //for better tests
            //original image
            int originalImgActualR;
            int originalImgActualG;
            int originalImgActualB;

            //message image
            int imgWithMessageActualR;
            int imgWithMessageActualG;
            int imgWithMessageActualB;


            //for testing purpose
            int counterOfChangingOccurs = 0;

            if (originalImg.Height == imgWithMessage.Height
                && originalImg.Width == imgWithMessage.Width)
            {
                for (int i = 0; i < originalImg.Width; i++)
                {
                    for (int j = 0; j < originalImg.Height; j++)
                    {
                        originalImgActualR = originalImg.GetPixel(i, j).R;
                        originalImgActualG = originalImg.GetPixel(i, j).G;
                        originalImgActualB = originalImg.GetPixel(i, j).B;

                        imgWithMessageActualR = imgWithMessage.GetPixel(i, j).R;
                        imgWithMessageActualG = imgWithMessage.GetPixel(i, j).G;
                        imgWithMessageActualB = imgWithMessage.GetPixel(i, j).B;

                        //check for eventually same R values of pixels of both imgs and B are different
                        if (originalImgActualB != imgWithMessageActualB
                           && originalImgActualR == imgWithMessageActualR
                           && originalImgActualG == imgWithMessageActualG
                           && i != imgWithMessage.Width - 25
                           && j != imgWithMessage.Height - 25)
                        {
                            bytesDifferenceMessage.Add(Convert.ToByte(imgWithMessageActualR));
                            counterOfChangingOccurs++;
                        }
                        //check for eventually same R values of pixels of both imgs and G are different
                        if (originalImgActualB == imgWithMessageActualB
                           && originalImgActualR == imgWithMessageActualR
                           && originalImgActualG != imgWithMessageActualG
                           && i != imgWithMessage.Width - 25
                           && j != imgWithMessage.Height - 25)
                        {
                            int diffG = Math.Abs(originalImgActualG - imgWithMessageActualG);
                            bytesDifferenceMessage.Add(Convert.ToByte(imgWithMessageActualR+diffG));
                            counterOfChangingOccurs++;
                        }
                        //message values
                        if (originalImgActualR != imgWithMessageActualR
                            && i != imgWithMessage.Width - 25 && j != imgWithMessage.Height - 25)
                        {
                            //calculate correct bytes values if original R is higher
                            if (originalImgActualR < imgWithMessageActualR)
                            {
                                int diffR = imgWithMessageActualR - originalImgActualR;
                                int diffRMultipledBy10 = diffR * 10;
                                int r = originalImgActualR + diffRMultipledBy10 + Math.Abs(imgWithMessageActualG - originalImgActualG);

                                if (r > 255 || r < 0)
                                {
                                    return null;
                                }
                                bytesDifferenceMessage.Add(Convert.ToByte(r));
                                counterOfChangingOccurs++;

                            }
                            //calculate correct bytes values if image with message R is higher
                            if (originalImgActualR > imgWithMessageActualR)
                            {
                                int diffG = Math.Abs(imgWithMessageActualG - originalImgActualG);
                                int diffB = Math.Abs(imgWithMessageActualB - originalImgActualB);
                                int diffGMultipliedBy10 = diffG * 10;
                                int r = imgWithMessageActualR - (diffGMultipliedBy10 + diffB);

                                if (r > 255 || r < 0)
                                {
                                    return null;
                                }
                                bytesDifferenceMessage.Add(Convert.ToByte(r));
                                counterOfChangingOccurs++;

                            }

                        }
                        //IV values
                        else if ((originalImgActualR != imgWithMessageActualR
                            || originalImgActualB != imgWithMessageActualB)
                            && i == imgWithMessage.Width - 25 && j < imgWithMessage.Height - 25)
                        {
                            bytesDifferenceIV.Add(Convert.ToByte(imgWithMessageActualR));

                        }
                        //Key values
                        else if ((originalImgActualR != imgWithMessageActualR
                            || originalImgActualB != imgWithMessageActualB)
                            && j == imgWithMessage.Height - 25)
                        {
                            bytesDifferenceKey.Add(Convert.ToByte(imgWithMessageActualR));

                        }
                    }
                }
            }
            else
            {
                return null;

            }
            key = bytesDifferenceKey.ToArray();
            iv = bytesDifferenceIV.ToArray();
            return bytesDifferenceMessage.ToArray();

        }

        public void DecryptMessageToString(byte[] Key, byte[] IV)
        {
            MessageInBytes = GetBytesDifference(OriginalImage, ImageWithHiddenMessage, ref Key, ref IV);
            // Check arguments.
            if (MessageInBytes == null || MessageInBytes.Length <= 0)
            {
                MessageBox.Show("Probably, you aint right person to read that message.", "Message reavel", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.Yes);
                return;
            }

            if (Key == null || Key.Length <= 0)
            {
                MessageBox.Show("Probably, you aint right person to read that message.", "Message reavel", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.Yes); return;
                return;

            }

            if (IV == null || IV.Length <= 0)
            {
                MessageBox.Show("Probably, you aint right person to read that message.", "Message reavel", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.Yes);
                return;

            }


            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an AesCryptoServiceProvider object
            // with the specified key and IV.
            try
            {
                using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
                {
                    aesAlg.Key = Key;
                    aesAlg.IV = IV;
                    //aesAlg.Padding = PaddingMode.Zeros;
                    // Create a decryptor to perform the stream transform.
                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    // Create the streams used for decryption.
                    using (MemoryStream msDecrypt = new MemoryStream(MessageInBytes))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {

                                // Read the decrypted bytes from the decrypting stream
                                // and place them in a string.
                                plaintext = srDecrypt.ReadToEnd();
                                csDecrypt.Close();
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Probably, you aint right person to read that message.", "Message reavel", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.Yes);
                HiddenMessage = String.Empty;
            }
            HiddenMessage = plaintext;
        }

        #region Loading and Saving
        public void LoadImageWithMessage()
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
                ImageWithHiddenMessageSourcePath = ofd.FileName;
                //imageWithHiddenMessage.Source = new BitmapImage(new Uri(ofd.FileName));
                ImageWithHiddenMessage = new Bitmap(Image.FromFile(ofd.FileName));
            }
        }

        public void LoadOriginalImage()
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
                OriginalImageSourcePath = ofd.FileName;
                ///originalImage.Source = new BitmapImage(new Uri(ofd.FileName));
                OriginalImage = new Bitmap(Image.FromFile(ofd.FileName));

            }
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


            if (result == true && !String.IsNullOrEmpty(HiddenMessage))
            {
                File.WriteAllText(sfd.FileName, HiddenMessage);
            }
        }

        #endregion Loading and Saving

    }
}
