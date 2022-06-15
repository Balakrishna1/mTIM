using System;
using mTIM.ViewModels;
using Xamarin.Forms;
using ZXing;
using ZXing.Net.Mobile.Forms;

namespace mTIM
{
    public partial class BarcodeContentPage : ZXingScannerPage
    {
        MainViewModel _viewModel;
        public BarcodeContentPage(MainViewModel viewModel)
        {
            _viewModel = viewModel;
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
                await this.Navigation.PopModalAsync();
                _viewModel.HandleResult(result);
            });
        }
    }
}
