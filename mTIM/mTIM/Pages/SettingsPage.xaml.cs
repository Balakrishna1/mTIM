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
