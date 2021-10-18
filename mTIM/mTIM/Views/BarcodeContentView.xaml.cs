using System;
using System.Collections.Generic;
using mTIM.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing;

namespace mTIM
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BarcodeContentView 
    {
        public BarcodeContentView()
        {
            InitializeComponent();
        }

        MainViewModel ViewModel;
        public void SetBindingViewModel(MainViewModel viewModel)
        {
            ViewModel = viewModel;
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            btnClose.Clicked += BtnClose_Clicked;
        }

        private void BtnClose_Clicked(object sender, EventArgs e)
        {
            ViewModel.IsOpenBarcodeView = false;
            ViewModel.IsScanning = false;
        }

        public void Handle_OnScanResult(Result result)
        {
            ViewModel.HandleResult(result);
            //Device.BeginInvokeOnMainThread(() =>
            //{
            //     Application.Current.MainPage.DisplayAlert("Scanned result", result.Text, "OK");
            //});
        }
    }
}
