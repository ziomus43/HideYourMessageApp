using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using MvvmCross.Platforms.Wpf.Views;
using BasicLibrary.ViewModels;
using System.IO;
using System.Security.Cryptography;
using MvvmCross.Navigation;

namespace ApplicationStarter.Views
{
    /// <summary>
    /// Interaction logic for EncryptDecryptView.xaml
    /// </summary>
    public partial class EncryptorView : MvxWpfView
    {

        private IMvxNavigationService _mvxNavigationService;

        public EncryptorView()
        {
            InitializeComponent();
            encryptorViewModel = new EncryptorViewModel(_mvxNavigationService);
        }

        EncryptorViewModel encryptorViewModel;

        #region Methods

        #region Loading and Saving


        private void LoadImageButton_Click(object sender, RoutedEventArgs e)
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
                imageToHideMessageInside.Source = new BitmapImage(new Uri(ofd.FileName));
            }
        }


        #endregion Loading and Saving

        private void EncryptMessageButton_Click(object sender, RoutedEventArgs e)
        {
            string encryptedMessageToShow=null;
            byte[] encryptedMessage = null;
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();

                if (!String.IsNullOrEmpty(messageToHide.Text))
                {
                    encryptedMessage = encryptorViewModel.EncryptMessageToBytes(messageToHide.Text,aes.Key,aes.IV);
                    encryptedMessageToShow = Convert.ToBase64String(encryptedMessage);
                }

            
            messageToHide.Text = encryptedMessageToShow;
            //encryptorViewModel.ChangePixelColor(encryptedMessage);

        }
        #endregion Methods

        private void HideMessageButton_Click(object sender, RoutedEventArgs e)
        {
            //encryptorViewModel.ChangePixelColor();
        }

        //For Check before making whole Decryption page
        private void Load_Click(object sender, RoutedEventArgs e)
        {
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            //messageToHide.Text = encryptorViewModel.DecryptMessageToString(messageToHide.Text, aes.Key, aes.IV);
        }

        private void SaveImageButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
