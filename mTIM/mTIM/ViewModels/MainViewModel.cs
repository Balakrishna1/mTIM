using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using mTIM.Enums;
using mTIM.Helpers;
using mTIM.Models;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace mTIM.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public ObservableCollectionRanged<TimTaskModel> SelectedItemList { get; set; }
        public ObservableCollectionRanged<Value> LstValues { get; set; } = new ObservableCollectionRanged<Value>();
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

        private bool isTaskListVisible;
        public bool IsTaskListVisible
        {
            get => isTaskListVisible;
            set => SetAndRaisePropertyChanged(ref isTaskListVisible, value);
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

        List<string> headerStrings = new List<string>();
        private int previousId;
        public void SelectedItemIndex(int position)
        {
            if (position >= 0)
            {
                ListBackgroundColor = GlobalConstants.IsLandscape ? Color.White : Color.DimGray;
                var selectedItem = SelectedItemList[position];
                if (selectedItem != null && selectedItem.HasChailds)
                {
                    previousId = selectedItem.Id;
                    IsShowBackButton = true;
                    //SelectedItemText = selectedItem.Name;
                    headerStrings.Add(selectedItem.Name);
                    updateHeaderTexts();
                    SelectedItemList.Clear();
                    SelectedItemList.AddRange(TotalListList.Where(x => x.Level.Equals(selectedItem.Level + 1) && x.Parent.Equals(selectedItem.Id)));
                }
                else
                {
                    RightIconCommand(selectedItem);
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
                updateTaskListVisibility();
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
            if(IsValueListVisible || IsDocumentListVisible || IsEditTextVisible)
            {
                IsDocumentListVisible = IsValueListVisible = IsEditTextVisible = false;
                updateTaskListVisibility();
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
            List<Value> valuesList = new List<Value>();
            SelectedModel = model;
            if (SelectedModel != null)
            {
                switch (SelectedModel.Type)
                {
                    case DataType.Int:
                        if (!string.IsNullOrEmpty(SelectedModel.Range))
                        {
                            IsValueListVisible = true;
                            updateTaskListVisibility();
                            string v = SelectedModel.Range;
                            int step = 1;
                            int indexStep = v.IndexOf(':');
                            if (indexStep == -1)
                                indexStep = v.IndexOf('|');
                            if (indexStep != -1)
                            {
                                string stepString = v.Substring(indexStep + 1);
                                v = v.Substring(0, indexStep);
                                int.TryParse(stepString, out step);
                            }
                            Debug.WriteLine(step <= 0 ? "Step has to be greater than 0!" : "");
                            int indexTo = v.IndexOf('-');
                            if (indexTo != -1)
                            {
                                string fromString = v.Substring(0, indexTo);
                                string toString = v.Substring(indexTo + 1);
                                int valueFrom = 0;
                                int.TryParse(fromString, out valueFrom);

                                int valueTo = valueFrom;
                                int.TryParse(toString, out valueTo);
                                Debug.WriteLine(valueFrom > valueTo ? "To value is smaller than from value" : "");

                                for (int i = valueFrom; i <= valueTo; i += step)
                                    valuesList.Add(new Value() { Data = i });
                            }
                            if (SelectedModel.Range.Contains(","))
                            {
                                var result = SelectedModel.Range.Split(',');
                                if (result.Length > 0)
                                {
                                    foreach (var value in result)
                                    {
                                        valuesList.Add(new Value() { Data = value });
                                    }
                                }
                            }
                            if (SelectedModel.Value != null)
                            {
                                var item = valuesList.Where(x => x.Data.Equals(SelectedModel.Value)).FirstOrDefault();
                                if (item != null)
                                {
                                    item.BackgroundColor = Color.LightGray;
                                }
                            }
                            LstValues.AddRange(valuesList);
                        }
                        else
                        {
                            InputText = SelectedModel.Value?.ToString(); ;
                            IsEditTextVisible = true;
                            updateTaskListVisibility();
                        }
                        break;
                    case DataType.Float:
                        if (!string.IsNullOrEmpty(SelectedModel.Range))
                        {
                            IsValueListVisible = true;
                            updateTaskListVisibility();
                            string v = SelectedModel.Range;
                            float step = 1;
                            int indexStep = v.IndexOf(':');
                            if (indexStep == -1)
                                indexStep = v.IndexOf('|');
                            if (indexStep != -1)
                            {
                                string stepString = v.Substring(indexStep + 1);
                                v = v.Substring(0, indexStep);
                                float.TryParse(stepString, out step);
                            }
                            Debug.WriteLine(step <= 0 ? "Step has to be greater than 0!" : "");
                            int indexTo = v.IndexOf('-');
                            if (indexTo != -1)
                            {
                                string fromString = v.Substring(0, indexTo);
                                string toString = v.Substring(indexTo + 1);
                                float valueFrom = 0;
                                float.TryParse(fromString, out valueFrom);

                                float valueTo = valueFrom;
                                float.TryParse(toString, out valueTo);
                                Debug.WriteLine(valueFrom > valueTo ? "To value is smaller than from value" : "");

                                for (float i = valueFrom; i <= valueTo; i += step)
                                    valuesList.Add(new Value() { Data = Convert.ToDecimal(i).ToString("0.00") });
                            }
                            if (SelectedModel.Range.Contains(","))
                            {
                                var result = SelectedModel.Range.Split(',');
                                if (result.Length > 0)
                                {
                                    foreach (var value in result)
                                    {
                                        valuesList.Add(new Value() { Data = Convert.ToDecimal(value).ToString("0.00") });
                                    }
                                }
                            }
                            if (SelectedModel.Value != null)
                            {
                                var item = valuesList.Where(x => x.Data.Equals(SelectedModel.Value)).FirstOrDefault();
                                if (item != null)
                                {
                                    item.BackgroundColor = Color.LightGray;
                                }
                            }
                            LstValues.AddRange(valuesList);
                        }
                        else
                        {
                            InputText = SelectedModel.Value?.ToString(); ;
                            IsEditTextVisible = true;
                            updateTaskListVisibility();
                        }
                        break;
                    case DataType.Bool:
                        SelectedModel.Value = !Convert.ToBoolean(SelectedModel.Value);
                        SelectedItemList.Update();
                        break;
                    case DataType.Doc:
                        SelectedDocumentItem(SelectedModel.Id);
                        break;
                    case DataType.String:
                        InputText = SelectedModel.Value?.ToString(); ;
                        IsEditTextVisible = true;
                        updateTaskListVisibility();
                        break;
                    default:
                        break;
                }
            }
        }

        public void SelectedValueIndex(int position)
        {
            IsValueListVisible = false;
            SelectedModel.Value = LstValues[position].Data;
            SelectedItemList.Update();
            updateTaskListVisibility();
            SaveTaskList();
        }

        private void updateTaskListVisibility()
        {
            if (IsValueListVisible || IsDocumentListVisible || IsEditTextVisible)
            {
                IsTaskListVisible = false;
            }
            else
            {
                IsTaskListVisible = true;
            }
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
            //         // Initialize the scanner first so it can track the current context
            //            MobileBarcodeScanner.Initialize (Application);
            //#endif
            //            var scanner = new ZXing.Mobile.MobileBarcodeScanner();

            //            var result = await scanner.Scan();

            //            HandleResult(result);
            //            await Navigation.PushModalAsync(new NavigationPage(new BarcodeContentPage()));
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
            IsDocumentListVisible = IsValueListVisible = IsEditTextVisible = false;
            updateTaskListVisibility();
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
                Debug.WriteLine("Task List: " + json);
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
        
        private async void RightIconCommand(TimTaskModel item)
        {
            if (item != null)
            {
                switch(item.Type)
                {
                    case DataType.Prjladen:
                    case DataType.Prjladen2:
                        await Application.Current.MainPage.DisplayAlert("Load task list?", item.Value.ToString(), "Ok", "Cancel");
                        break;
                    case DataType.Aktion:
                    case DataType.Aktion2:
                        item.Value= DateTime.Now.ToString("HH:mm:ss");
                        SelectedItemList?.Update();
                        break;
                    default:
                        break;
                }
            }
        }

        public ICommand CancelButtonCommand => new Command(CancelButtonCommandExecute);
        private void CancelButtonCommandExecute()
        {
            IsEditTextVisible = false;
            updateTaskListVisibility();
        }

        public ICommand OkButtonCommand => new Command(OkButtonCommandExecute);
        private void OkButtonCommandExecute()
        {
            SelectedModel.Value = InputText;
            SelectedItemList.Update();
            IsEditTextVisible = false;
            updateTaskListVisibility();
        }
    }

    public class Value
    {
        public object Data { get; set; }
        public Color BackgroundColor { get; set; } = Color.Transparent;
    }
}
