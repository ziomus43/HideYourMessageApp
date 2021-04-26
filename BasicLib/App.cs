using System;
using System.Collections.Generic;
using System.Text;
using BasicLib.ViewModels;
using MvvmCross.ViewModels;

namespace BasicLib
{
    public class App:MvxApplication
    {
        public override void Initialize()
        {
            RegisterAppStart<EncryptorViewModel>(); 
        }
    }
}
