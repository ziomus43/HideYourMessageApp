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
using BasicLib.ViewModels;
using System.IO;
using System.Security.Cryptography;

namespace ApplicationStarter.Views
{
    /// <summary>
    /// Interaction logic for EncryptDecryptView.xaml
    /// </summary>
    public partial class EncryptorView : MvxWpfView
    {
        public EncryptorView()
        {
            InitializeComponent();
        }

        readonly EncryptorViewModel encryptorViewModel = new EncryptorViewModel();

        #region Methods

        #region Loading and Saving
        private void LoadTextButton_Click(object sender, RoutedEventArgs e)
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
                messageToHide.Text = streamReader.ReadToEnd();

                //close the stream
                streamReader.Dispose();

            }

        }

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


        private void SaveTextButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Save your text message to txt file";
            sfd.FileName = "";
            sfd.DefaultExt = ".txt";
            sfd.Filter = "Text File (.txt)|*.txt";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = sfd.ShowDialog();


            if (result == true && !String.IsNullOrEmpty(messageToHide.Text))
            {
                File.WriteAllText(sfd.FileName, messageToHide.Text);
            }
        }






        #endregion Loading and Saving

        private void EncryptMessageButton_Click(object sender, RoutedEventArgs e)
        {
            string encryptedMessageToShow=null;
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();

                if (!String.IsNullOrEmpty(messageToHide.Text))
                {
                    byte[] encryptedMessage = encryptorViewModel.EncryptMessageToBytes(messageToHide.Text,aes.Key,aes.IV);
                    encryptedMessageToShow = Convert.ToBase64String(encryptedMessage);
                }

            
            messageToHide.Text = encryptedMessageToShow;


        }
        #endregion Methods
    }
}
