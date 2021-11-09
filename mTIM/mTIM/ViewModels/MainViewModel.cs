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
        public ObservableCollectionRanged<object> LstValues { get; set; } = new ObservableCollectionRanged<object>();
        public ObservableCollectionRanged<FileInfo> LstFiles { get; set; } = new ObservableCollectionRanged<FileInfo>();

        public AndroidMessageModel MessageModel { get; set; } = new AndroidMessageModel();
        public MainViewModel(INavigation navigation)
        {
            this.Navigation = navigation;
            SelectedItemList = new ObservableCollectionRanged<TimTaskModel>();
            FileInfoHelper.Instance.LoadFileList();
            FileInfoHelper.Instance.LoadExtensions();
        }

        public override void OnAppearing()
        {
            base.OnAppearing();
            TimerHelper.Instance.StartTimer();
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

        private string inputText = "";
        public string InputText
        {
            get => inputText;
            set => SetAndRaisePropertyChanged(ref inputText, value);
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

        private bool isDocumentListVisible;
        public bool IsDocumentListVisible
        {
            get => isDocumentListVisible;
            set => SetAndRaisePropertyChanged(ref isDocumentListVisible, value);
        }

        private bool isEditTextVisible;
        public bool IsEditTextVisible
        {
            get => isEditTextVisible;
            set => SetAndRaisePropertyChanged(ref isEditTextVisible, value);
        }

        private object selectedValue;
        public object SelectedValue
        {
            get => selectedValue;
            set => SetAndRaisePropertyChanged(ref selectedValue, value);
        }

        List<string> headerStrings = new List<string>();
        private int previousId;
        public void SelectedItemIndex(int position)
        {
            if (position >= 0)
            {
                ListBackgroundColor = GlobalConstants.IsLandscape? Color.White: Color.DimGray;
                var selectedItem = SelectedItemList[position];
                if (selectedItem != null && selectedItem.HasChailds)
                {
                    previousId = selectedItem.Id;
                    IsShowBackButton = true;
                    //SelectedItemText = selectedItem.Name;
                    headerStrings.Add(selectedItem.Name);
                    updateHeaderTexts();
                    SelectedItemList.Clear();
                    SelectedItemList.AddRange(TotalListList.Where(x => x.Level.Equals(selectedItem.Level+1) && x.Parent.Equals(selectedItem.Id)));
                }
            }
        }

        public void SelectedValueItem(TimTaskModel model)
        {
            openValues(model);
        }

        public void SelectedDocumentItem(int id)
        {
            openDocumentValues(id);
        }

        private void openDocumentValues(int id)
        {
            var values = FileInfoHelper.Instance.GetValues(id);
            LstFiles?.Clear();
            if (values?.Count > 0)
            {
                IsDocumentListVisible = true;
                LstFiles.AddRange(values);
            }
        }

        public void SelectedDocument(int position)
        {
            var file = LstFiles[position];
            if(file!= null)
            {
                Webservice.ViewModel = this;
                Webservice.OpenPdfFile(Math.Abs(file.FileID), file.FileIDSpecified);
            }
        }

        public ICommand BackButtonCommand => new Command(BackButtonCommandExecute);

        private void BackButtonCommandExecute()
        {
            if(IsValueListVisible || IsDocumentListVisible)
            {
                IsDocumentListVisible = false;
                IsValueListVisible = false;
                return;
            }
            if (previousId >= 0)
            {
                ListBackgroundColor = GlobalConstants.IsLandscape ? Color.White : Color.DimGray;
                var selectedItem = TotalListList.Where(x => x.Id.Equals(previousId)).FirstOrDefault();
                if (selectedItem != null)
                {
                    previousId = selectedItem.Parent;
                    IsShowBackButton = true;
                    headerStrings.RemoveAt(headerStrings.Count - 1);
                    updateHeaderTexts();
                    //SelectedItemText = selectedItem.Name;
                    SelectedItemList.Clear();
                    SelectedItemList.AddRange(TotalListList.Where(x => x.Level.Equals(selectedItem.Level) && x.Parent.Equals(selectedItem.Parent)));
                }
            }
            //RefreshData();
        }

        TimTaskModel SelectedModel = new TimTaskModel();
        private void openValues(TimTaskModel model)
        {
            LstValues?.Clear();
            List<object> valuesList = new List<object>();
            SelectedModel = model;
            if (SelectedModel != null)
            {
                if (SelectedModel.Type != null)
                {
                    switch (SelectedModel.Type)
                    {
                        case "int":
                            IsValueListVisible = true;
                            if (!string.IsNullOrEmpty(SelectedModel.Range))
                            {
                                var result = SelectedModel.Range.Split(',', ':', '-');
                                if (result!=null)
                                {
                                    decimal startvalue;
                                    decimal endValue;
                                    int splitValue;
                                    if (SelectedModel.Range.Contains(","))
                                    {
                                        if(result.Length>0)
                                        {
                                            valuesList.AddRange(result);
                                        }
                                    }
                                    else
                                    {
                                        switch (result.Length)
                                        {
                                            case 1:
                                                valuesList.Add(Convert.ToInt32(result[0]));
                                                break;
                                            case 2:
                                                startvalue = Convert.ToInt32(result[0]);
                                                endValue = Convert.ToInt32(result[1]);
                                                for (decimal i = startvalue; i <= endValue; i++)
                                                {
                                                    valuesList.Add(i);
                                                }
                                                break;
                                            case 3:
                                                startvalue = Convert.ToDecimal(result[0]);
                                                endValue = Convert.ToDecimal(result[1]);
                                                splitValue = Convert.ToInt32(result[2]);
                                                var res = Math.Round(endValue / splitValue);
                                                int value = (int)(startvalue + res);
                                                valuesList.Add(startvalue);
                                                for (int i = 1; i < splitValue; i++)
                                                {
                                                    valuesList.Add(value * i);
                                                }

                                                break;
                                        }
                                    }
                                    LstValues.AddRange(valuesList);
                                    if (SelectedModel.Value != null)
                                        SelectedValue = Convert.ToInt32(SelectedModel.Value);
                                }
                            }
                            break;
                        case "bool":
                            SelectedModel.Value = !Convert.ToBoolean(SelectedModel.Value);
                            SelectedItemList.Update();
                            break;
                        case "Doc":
                            SelectedDocumentItem(SelectedModel.Id);
                            break;
                        case "string":
                            InputText = SelectedModel.Value?.ToString(); ;
                            IsEditTextVisible = true;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public void SelectedValueIndex(int position)
        {
            IsValueListVisible = false;
            SelectedModel.Value = LstValues[position];
            SelectedItemList.Update();
            SaveTaskList();
        }

        private void updateHeaderTexts()
        {
            SelectedItemText = headerStrings.LastOrDefault();
            if (headerStrings.Count > 1)
            {
                SubText = String.Join("/", headerStrings.SkipLast(1));
                IsShowBackButton = true;
            }
            else if (headerStrings.Count == 0)
            {
                IsShowBackButton = false;
                SelectedItemText = TotalListList?.Where(x => x.Level.Equals(0)).FirstOrDefault()?.Name;
                SubText = string.Empty;
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
            IsDocumentListVisible = false;
            IsValueListVisible = false;
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
            base.OnSyncCommand(isFromAuto);
        }

        public ICommand MenuNewBuildItemCommand => new Command(MenuNewBuildItemCommandExecute);
        private void MenuNewBuildItemCommandExecute()
        {
            IsOpenMenuOptions = false;
        }

        public ICommand CancelButtonCommand => new Command(CancelButtonCommandExecute);
        private void CancelButtonCommandExecute()
        {
            IsEditTextVisible = false;
        }

        public ICommand OkButtonCommand => new Command(OkButtonCommandExecute);
        private void OkButtonCommandExecute()
        {
            SelectedModel.Value = InputText;
            SelectedItemList.Update();
            IsEditTextVisible = false;
        }
    }
}
