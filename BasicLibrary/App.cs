using System;
using System.Collections.Generic;
using System.Text;
using BasicLibrary.ViewModels;
using MvvmCross.ViewModels;

namespace BasicLibrary
{
    public class App:MvxApplication
    {
        public override void Initialize()
        {
            RegisterAppStart<EncryptorViewModel>(); 
        }
    }
}
