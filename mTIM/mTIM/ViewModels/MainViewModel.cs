using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using mTIM.Helpers;
using mTIM.Models;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace mTIM.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public ObservableCollectionRanged<TimTaskModel> SelectedItemList { get; set; }
        public ObservableCollectionRanged<int> LstValues { get; set; } = new ObservableCollectionRanged<int>();

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

        private string subText = "";
        public string SubText
        {
            get => subText;
            set => SetAndRaisePropertyChanged(ref subText, value);
        }

        private bool isOpenMenuOptions;
        public bool IsOpenMenuOptions
        {
            get => isOpenMenuOptions;
            set => SetAndRaisePropertyChanged(ref isOpenMenuOptions, value);
        }

        private bool isValueListVisible;
        public bool IsValueListVisible
        {
            get => isValueListVisible;
            set => SetAndRaisePropertyChanged(ref isValueListVisible, value);
        }

        private object selectedValue;
        public object SelectedValue
        {
            get => selectedValue;
            set => SetAndRaisePropertyChanged(ref selectedValue, value);
        }

        List<string> headerStrings = new List<string>();
        private int previousIndex;
        private int seletedIndex;
        public void SelectedItemIndex(int position)
        {
            if (position >= 0)
            {
                ListBackgroundColor = GlobalConstants.IsLandscape? Color.White: Color.DimGray;
                seletedIndex = position;
                var selectedItem = SelectedItemList[position];
                if (selectedItem != null && selectedItem.HasChailds)
                {
                    previousIndex = selectedItem.Parent;
                    IsShowBackButton = true;
                    //SelectedItemText = selectedItem.Name;
                    headerStrings.Add(selectedItem.Name);
                    updateHeaderTexts();
                    SelectedItemList.Clear();
                    SelectedItemList.AddRange(TotalListList.Where(x => x.Level.Equals(selectedItem.Level+1) && x.Parent.Equals(selectedItem.Id)));
                }
            }
        }

        public void SelectedValueItemIndex(int position)
        {
            setValues(position);
        }

        public ICommand BackButtonCommand => new Command(BackButtonCommandExecute);

        private void BackButtonCommandExecute()
        {
            if(IsValueListVisible)
            {
                IsValueListVisible = false;
                return;
            }
            if (previousIndex >= 0)
            {
                ListBackgroundColor = GlobalConstants.IsLandscape ? Color.White : Color.DimGray;
                seletedIndex = previousIndex;
                var selectedItem = TotalListList.Where(x => x.Level.Equals(previousIndex)).FirstOrDefault();
                if (selectedItem != null)
                {
                    previousIndex = selectedItem.Parent;
                    IsShowBackButton = true;
                    headerStrings.RemoveAt(headerStrings.Count - 1);
                    updateHeaderTexts();
                    //SelectedItemText = selectedItem.Name;
                    SelectedItemList.Clear();
                    SelectedItemList.AddRange(TotalListList.Where(x => x.Level.Equals(selectedItem.Level + 1) && x.Parent.Equals(selectedItem.Id)));
                }
            }
            //RefreshData();
        }

        TimTaskModel SelectedModel = new TimTaskModel();
        private void setValues(int position)
        {
            LstValues?.Clear();
            List<int> valuesList = new List<int>();
            SelectedModel = SelectedItemList[position];
            if(SelectedModel != null)
            {
                IsValueListVisible = true;
                if (!string.IsNullOrEmpty(SelectedModel.Range))
                {
                    var result = SelectedModel.Range.Split(':', '-');
                    if (result != null)
                    {
                        int startvalue;
                        int endValue;
                        int splitValue;
                        switch (result.Length)
                        {
                            case 1:
                                valuesList.Add(Convert.ToInt32(result[0]));
                                break;
                            case 2:
                                startvalue = Convert.ToInt32(result[0]);
                                endValue = Convert.ToInt32(result[1]);
                                for (int i = startvalue; i <= endValue; i++)
                                {
                                    valuesList.Add(i);
                                }
                                break;
                            case 3:
                                startvalue = Convert.ToInt32(result[0]);
                                endValue = Convert.ToInt32(result[1]);
                                splitValue = Convert.ToInt32(result[2]);
                                for (int i = startvalue; i <= endValue; i++)
                                {
                                    valuesList.Add(i);
                                }
                                break;
                        }
                        LstValues.AddRange(valuesList);
                        if (SelectedModel.Value != null)
                            SelectedValue = Convert.ToInt32(SelectedModel.Value);
                    }
                }
            }
        }

        public void SelectedValueIndex(int position)
        {
            IsValueListVisible = false;
            SelectedModel.Value = LstValues[position];
            SelectedItemList.Update();
        }

        private void updateHeaderTexts()
        {
            SelectedItemText = headerStrings.LastOrDefault();
            if(headerStrings.Count > 1)
            {
                SubText = String.Join("/", headerStrings.SkipLast(1));
                IsShowBackButton = true;
            }else if(headerStrings.Count == 0)
            {
                IsShowBackButton = false;
                SelectedItemText =  TotalListList.Where(x => x.Level.Equals(0)).FirstOrDefault().Name;
            }
            else
            {
                IsShowBackButton = true;
                SubText = string.Empty;
            }
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
            headerStrings = new List<string>();
            updateHeaderTexts();
            //SelectedItemText =  TotalListList.Where(x => x.Level.Equals(0)).FirstOrDefault().Name;
            ListBackgroundColor = Color.White;
            IsOpenMenuOptions = false;
            IsShowBackButton = false;
            SelectedItemList.Clear();
            SelectedItemList.AddRange(TotalListList.Where(x => x.Level.Equals(1) && x.Parent.Equals(0)));
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
                    UpdateChaildValues(list);
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
