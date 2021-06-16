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

    }
}
