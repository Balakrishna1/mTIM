using mTIM.Helpers;
using mTIM.ViewModels;
using Xamarin.Forms;

namespace mTIM
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsViewModel ViewModel;
        public SettingsPage()
        {
            InitializeComponent();
            ViewModel = new SettingsViewModel(Navigation);
            BindingContext = ViewModel;
            btnClose.Clicked -= BtnClose_Clicked;
            btnClose.Clicked += BtnClose_Clicked;
        }

        private async void BtnClose_Clicked(object sender, System.EventArgs e)
        {
            await TouchHelper.Instance.TouchEffectsWithCommand<object>(btnClose, 0.7, 50, ViewModel.CloseCommand);
        }

        protected override void OnAppearing()
        {
            ViewModel.OnAppearing();
            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            ViewModel.OnDisAppearing();
            base.OnDisappearing();
        }
    }
}
