using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using mTIM.Helpers;
using mTIM.Interfaces;
using mTIM.Models;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace mTIM.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public ObservableCollectionRanged<TimTaskModel> SelectedItemList { get; set; }
        public AndroidMessageModel MessageModel { get; set; } = new AndroidMessageModel();
        public MainViewModel(INavigation navigation)
        {
            this.Navigation = navigation;
            SelectedItemList = new ObservableCollectionRanged<TimTaskModel>();
        }

        private Color listBackgroundColor = Color.White;
        public Color ListBackgroundColor
        {
            get => listBackgroundColor;
            set => SetAndRaisePropertyChanged(ref listBackgroundColor, value);
        }

        private bool isShowBackButton = false;
        public bool IsShowBackButton
        {
            get => isShowBackButton;
            set => SetAndRaisePropertyChanged(ref isShowBackButton, value);
        }

        private string selectedItemText = "";
        public string SelectedItemText
        {
            get => selectedItemText;
            set => SetAndRaisePropertyChanged(ref selectedItemText, value);
        }

        private bool isOpenMenuOptions;
        public bool IsOpenMenuOptions
        {
            get => isOpenMenuOptions;
            set => SetAndRaisePropertyChanged(ref isOpenMenuOptions, value);
        }

        private int seletedIndex;
        public void SelectedItemIndex(int position)
        {
            if (position >= 0)
            {
                ListBackgroundColor = GlobalConstants.IsLandscape? Color.White: Color.DimGray;
                seletedIndex = position;
                var slectedItem = SelectedItemList[position];
                if (slectedItem != null && !slectedItem.Level.Equals("2"))
                {
                    IsShowBackButton = true;
                    SelectedItemText = slectedItem.Name;
                    SelectedItemList.Clear();
                    SelectedItemList.AddRange(TotalListList.Where(x => x.Level.Equals("2") && x.Parent.Equals(slectedItem.Id)));
                }
            }
        }

        public ICommand BackButtonCommand => new Command(BackButtonCommandExecute);

        private void BackButtonCommandExecute()
        {
            RefreshData();
        }

        private bool isScanning;
        public bool IsScanning
        {
            get => isScanning;
            set => SetAndRaisePropertyChanged(ref isScanning, value);
        }

        private bool isOpenBarcodeView;
        public bool IsOpenBarcodeView
        {
            get => isOpenBarcodeView;
            set => SetAndRaisePropertyChanged(ref isOpenBarcodeView, value);
        }

        public ICommand MenuClickCommand => new Command(MenuClickCommandExecute);

        private void MenuClickCommandExecute()
        {
            IsOpenMenuOptions = true;
        }

        public ICommand BarcodeClickCommand => new Command(BarcodeClickCommandExecute);

        private void BarcodeClickCommandExecute()
        {
            IsOpenBarcodeView = true;
            IsScanning = true;
            //#if __ANDROID__
            //            	            // Initialize the scanner first so it can track the current context
            //            	            MobileBarcodeScanner.Initialize (Application);
            //#endif
            //var scanner = new ZXing.Mobile.MobileBarcodeScanner();

            //var result = await scanner.Scan();

            //HandleResult(result);
            //await  Navigation.PushModalAsync(new NavigationPage(new BarcodeContentPage()));
        }

        public ICommand CloseBarcodeCommand => new Command(CloseBarcodeCommandExecute);
        private void CloseBarcodeCommandExecute()
        {
            IsOpenBarcodeView = false;
            IsScanning = false;
        }

        public async void HandleResult(ZXing.Result result)
        {
            IsScanning = false;
            IsOpenBarcodeView = false;
            var msg = "No Barcode!";
            if (result != null)
            {
                msg = "Barcode: " + result.Text + " (" + result.BarcodeFormat + ")";
            }

            Device.BeginInvokeOnMainThread(async () =>
            {
                await Application.Current.MainPage.DisplayAlert("", msg, "Ok");
            });
        }

        public ICommand MenuSettingsItemCommand => new Command(MenuSettingsCommandExecute);
        private async void MenuSettingsCommandExecute()
        {
            IsOpenMenuOptions = false;
            await Navigation.PushModalAsync(new NavigationPage(new SettingsPage()));
        }

        public ICommand MenuInfoItemCommand => new Command(MenuInfoItemCommandExecute);
        private async void MenuInfoItemCommandExecute()
        {
            IsOpenMenuOptions = false;
            var locationstring = string.Format($"{GlobalConstants.LocationDetails?.Longitude} | {GlobalConstants.LocationDetails?.Latitude}");
            await Application.Current.MainPage.DisplayAlert(LabelInfo, string.Format($"IMEI: {GlobalConstants.IMEINumber}\nDeviceID: {GlobalConstants.IMEINumber}\nUniqueID: {GlobalConstants.UniqueID}\nURL: {GlobalConstants.AppBaseURL}\nVersion: {GlobalConstants.VersionNumber}\nLocation: {locationstring}"), "Ok");
        }

        public ICommand MenuRefreshItemCommand => new Command(MenuRefreshItemCommandExecute);
        private void MenuRefreshItemCommandExecute()
        {
            OnSyncCommand(false);
        }

        public override void RefreshData()
        {
            ListBackgroundColor = Color.White;
            IsOpenMenuOptions = false;
            SelectedItemText = string.Empty;
            IsShowBackButton = false;
            SelectedItemList.Clear();
            SelectedItemList.AddRange(TotalListList.Where(x => x.Level.Equals("1") && x.Parent.Equals("0")));
        }

        public async void UpdateList()
        {
            if (!FileHelper.IsFileExists(GlobalConstants.TASKLIST_FILE))
            {
                Webservice.ViewModel = this;
                Webservice.GetTasksListIDsFortheData();
            }
            else
            {
                string json = await FileHelper.ReadTextAsync(GlobalConstants.TASKLIST_FILE);
                var list = JsonConvert.DeserializeObject<List<TimTaskModel>>(json);
                TotalListList.Clear();
                if (list != null)
                {
                    TotalListList.AddRange(list);
                    RefreshData();
                }
            }
        }

        public override void OnSyncCommand(bool isFromAuto = true)
        {
            RefreshData();
            Webservice.ViewModel = this;
            Webservice.GetTasksListIDsFortheData(isFromAuto);
            base.OnSyncCommand();
        }

        public ICommand MenuNewBuildItemCommand => new Command(MenuNewBuildItemCommandExecute);
        private void MenuNewBuildItemCommandExecute()
        {
            IsOpenMenuOptions = false;
        }
    }
}
