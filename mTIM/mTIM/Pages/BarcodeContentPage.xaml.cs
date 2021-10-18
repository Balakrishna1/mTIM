using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;
using ZXing;
using ZXing.Net.Mobile.Forms;

namespace mTIM
{
    public partial class BarcodeContentPage : ZXingScannerPage
    {
        public BarcodeContentPage()
        {
            InitializeComponent();
        }

        private void BtnClose_Clicked(object sender, EventArgs e)
        {
            this.Navigation.PopModalAsync();
        }

        public void Handle_OnScanResult(Result result)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Application.Current.MainPage.DisplayAlert("Scanned result", result.Text, "OK");
                await this.Navigation.PopModalAsync();
            });
        }
    }
}
