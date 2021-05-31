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


        #endregion Interfaces


        #region ctor
        public DecryptorViewModel(IMvxNavigationService mvxNavigationService)
        {
            _navigationService = mvxNavigationService;
            SwitchViewToEncoderCommand = new MvxCommand(SwitchViewToEncoder);
            DecryptMessageToStringCommand = new MvxCommand(() => DecryptMessageToString(new AesCryptoServiceProvider().Key, new AesCryptoServiceProvider().IV));
            LoadImageWithMessageCommand = new MvxCommand(LoadImageWithMessage);
            LoadOriginalImageCommand = new MvxCommand(LoadOriginalImage);
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
                    //ImageWithHiddenMessage = new Bitmap(Image.FromFile(_imageWithHiddenMessageSourcePath));
                    //RaisePropertyChanged("ImageWithHiddenMessage");
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
            if (originalImg.Height == imgWithMessage.Height
                && originalImg.Width == imgWithMessage.Width)
            {
                for (int i = 0; i < originalImg.Width; i++)
                {
                    for (int j = 0; j < originalImg.Height; j++)
                    {
                        if (originalImg.GetPixel(i, j).R != imgWithMessage.GetPixel(i, j).R && i != 400 && j != 300)
                        {
                            //bytesDifferenceMessage.Add(Convert.ToByte(Math.Abs(originalImg.GetPixel(i, j).R - imgWithMessage.GetPixel(i, j).R)));
                            bytesDifferenceMessage.Add(Convert.ToByte(imgWithMessage.GetPixel(i, j).R));

                        }
                        else if (originalImg.GetPixel(i, j).R != imgWithMessage.GetPixel(i, j).R && i == 400 && j < 300)
                        {
                            //bytesDifferenceKey.Add(Convert.ToByte(Math.Abs(originalImg.GetPixel(i, j).R - imgWithMessage.GetPixel(i, j).R)));
                            bytesDifferenceKey.Add(Convert.ToByte(imgWithMessage.GetPixel(i, j).R));

                        }
                        else if (originalImg.GetPixel(i, j).R != imgWithMessage.GetPixel(i, j).R && j == 300)
                        {
                            //bytesDifferenceIV.Add(Convert.ToByte(Math.Abs(originalImg.GetPixel(i, j).R - imgWithMessage.GetPixel(i, j).R)));
                            bytesDifferenceIV.Add(Convert.ToByte(imgWithMessage.GetPixel(i, j).R));

                        }
                    }
                }
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
                throw new ArgumentNullException("messageInBytes");
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
                        }
                    }
                }
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

        #endregion Loading and Saving

    }
}
