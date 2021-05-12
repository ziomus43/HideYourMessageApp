using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using MvvmCross.ViewModels;
using System.Drawing;


namespace BasicLib.ViewModels
{
    public class EncryptorViewModel:MvxViewModel
    {

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
            { return _isEnabled=(!String.IsNullOrEmpty(MessageToHide))&&(String.IsNullOrEmpty(EncryptedMessage)); }

            set
            {
                SetProperty(ref _isEnabled, value);
            }
        }


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

                using(MemoryStream msEncrypt=new MemoryStream())
                {
                    using(CryptoStream csEncrypt=new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using(StreamWriter swEncrypt=new StreamWriter(csEncrypt))
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
            foreach(byte bajt in encrypted)
            {
                for(int i=0; i<encrypted.Length; i++)
                {
                    if (bajt == encrypted[i]&& Array.IndexOf(encrypted, bajt)!=i)
                    {
                        counter += 1;
                        catched.Add(bajt);
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
        public Bitmap ChangePixelColor(byte[] tab)
        {
            Bitmap bmp;
            string fileName = "C:\\Users\\Endrju\\Desktop\\Red_Image.png";
            bmp = (Bitmap)Image.FromFile(fileName);
            bmp = ChangeColor(bmp, tab);
            bmp.Save("C:\\Users\\Endrju\\Desktop\\Saved_Image.png");
            return bmp;
        }

        //Get pixel color
        public Bitmap ChangeColor(Bitmap sourceBitmap, byte[] tab)
        {
            Color newColor = Color.Green;
            Color actualColor;
            int a = newColor.A;
            int r = newColor.R;
            int g = newColor.G;
            int b = newColor.B;
            int[] tab1 = new int[] { a, r, g, b };
            Bitmap newBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);
            for(int i=0; i<sourceBitmap.Width/2; i++)
            {
                for(int j=0; j<sourceBitmap.Height/2; j++)
                {
                    actualColor = sourceBitmap.GetPixel(i, j);
                    if (i < tab.Length)
                    {
                        newColor = Color.FromArgb(tab[i], newColor.G, newColor.B);
                    }
                    if (actualColor.A >150)
                    {
                        newBitmap.SetPixel(i, j, newColor);
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
    }
}
