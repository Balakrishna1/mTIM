using Acr.UserDialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using TouchEffect.UWP;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace mTIM.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            ZXing.Net.Mobile.Forms.WindowsUniversal.Platform.Init();
            UserDialogs.Init();
            TouchEffectPreserver.Preserve();
            this.InitializeComponent();
            _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                LoadApplication(new mTIM.App());
              });
        }
    }
}
