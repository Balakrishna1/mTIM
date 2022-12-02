using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using HeyRed.Mime;
using Microsoft.AppCenter.Crashes;
using mTIM.Enums;
using mTIM.Helpers;
using mTIM.Interfaces;
using mTIM.Managers;
using mTIM.Models;
using mTIM.Models.D;
using mTIM.Resources;
using Newtonsoft.Json;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace mTIM.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public ObservableCollectionRanged<TimTaskModel> SelectedItemList { get; set; }
        public ObservableCollectionRanged<Value> LstValues { get; set; } = new ObservableCollectionRanged<Value>();
        public ObservableCollectionRanged<FileInfo> LstFiles { get; set; } = new ObservableCollectionRanged<FileInfo>();

        public AndroidMessageModel MessageModel { get; set; } = new AndroidMessageModel();
        public AppVersionUpdateInfo VersionInfo { get; set; } = new AppVersionUpdateInfo();
        public IDevice Device;
        public const string installAppFormat = "Install Update{0}";

        public IList<TimMesh> Meshes { get; set; } = new List<TimMesh>();
        public Result ProbufResult { get; set; } = new Result();

        public Action<string> ActionSelectedItemText;
        public Action<int> UpdateDrawing;
        public Action<int> UpdateListSelection;
        public MainViewModel(INavigation navigation)
        {
            this.Navigation = navigation;
            Device = DependencyService.Get<IDevice>();
            SelectedItemList = new ObservableCollectionRanged<TimTaskModel>();
            FileInfoHelper.Instance.LoadFileList();
            FileInfoHelper.Instance.LoadExtensions();
            FileInfoHelper.Instance.FileUploadCompleted -= FileUploadCompleted;
            FileInfoHelper.Instance.CommentUpdatedCompleted -= EditCommentCompleted;
            Webservice.GraphicsDownloadedCallBack -= GraphicsDownloadedCallBack;
            FileInfoHelper.Instance.FileUploadCompleted += FileUploadCompleted;
            FileInfoHelper.Instance.CommentUpdatedCompleted += EditCommentCompleted;
            Webservice.GraphicsDownloadedCallBack += GraphicsDownloadedCallBack;
            UploadOfflineFilesIntoServer();
            LoadMesh();
        }

        private void GraphicsDownloadedCallBack(bool isDownloaded)
        {
            Task.Run(() => LoadMesh());
        }

        /// <summary>
        /// Load the mesh in background when download the data from service.
        /// </summary>
        public async void LoadMesh()
        {
            try
            {
                if (!FileHelper.IsFileExists(GlobalConstants.GraphicsBlob_FILE))
                {
                    return;
                }
                var compressedData = await FileHelper.ReadAllBytesAsync(GlobalConstants.GraphicsBlob_FILE);
                if (compressedData != null && compressedData.Length > 0)
                {
                    ProbufResult = GZipHelper.DeserializeResult(compressedData);
                    Meshes = CreateMesh(ProbufResult);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                CrashReportManager.ReportError(ex, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// This is used to create the mesh.
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public IList<TimMesh> CreateMesh(Result result)
        {
            TimMeshLoader meshLoader = new TimMeshLoader();
            var lst = TimTaskListHelper.GetPrecastElements();
            var meshes = meshLoader.Load(result, lst);
            return meshes;
        }

        #region File related code
        /// <summary>
        /// File upload completed callback.
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="postId"></param>
        /// <param name="UploadFileId"></param>
        /// <param name="UploadFileSpecified"></param>
        private void FileUploadCompleted(int taskId, int postId, int UploadFileId, bool UploadFileSpecified)
        {
            var item = LstFiles.Where(x => x.FileID.Equals(UploadFileId)).FirstOrDefault();
            if (item != null)
            {
                var index = LstFiles.IndexOf(item);
                item.FileID = UploadFileId;
                item.FileIDSpecified = UploadFileSpecified;
                item.IsOffline = false;
                if (index >= 0)
                    LstFiles.ReplaceItem(index, item);
            }
        }

        /// <summary>
        /// Edit comment completed callback.
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="fileId"></param>
        private void EditCommentCompleted(int taskId, int fileId)
        {
            var item = LstFiles.Where(x => x.FileID.Equals(fileId)).FirstOrDefault();
            if (item != null)
            {
                var index = LstFiles.IndexOf(item);
                item.IsCommentEdited = false;
                if (index >= 0)
                    LstFiles.ReplaceItem(index, item);
            }
        }


        /// <summary>
        /// To capture picture from Camera.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public void CapturePhotoAsync(FileInfo file)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    MediaPickerOptions option = new MediaPickerOptions();
                    option.Title = "mTIM";
                    var fileinfo = await MediaPicker.CapturePhotoAsync(option);
                    var bytes = LoadPhotoAsync(fileinfo);
                    var extension = System.IO.Path.GetExtension(fileinfo.FullPath);
                    await Task.Delay(500);

                    PromptConfig config = new PromptConfig();
                    config.Title = "Datei hinzufuegen?";
                    config.InputType = InputType.Name;
                    config.Message = "Wollen Sie die Datei hinzufuegen?";
                    config.Placeholder = "Kommentar";
                    config.OkText = "Ja";
                    config.CancelText = "Nein";
                    var result = await UserDialogs.Instance.PromptAsync(config);
                    if (result.Ok)
                    {
                        int postId = FileHelper.GenerateAndGetOfflineID();
                        int uploadFileResult;
                        bool uploadFileResultSpecified = false;
                        FileInfo info = new FileInfo();
                        List<FileInfo> infos = new List<FileInfo>();
                        info.FileID = postId;
                        info.Comment = string.IsNullOrEmpty(result.Text) ? postId.ToString() : result.Text;
                        info.IsOffline = true;
                        info.OfflineFilePath = fileinfo.FullPath;
                        info.FileIDSpecified = uploadFileResultSpecified;
                        int taskId = SelectedModel.Id;
                        FileInfoHelper.Instance.UpdateValueInList(taskId, info);
                        infos = GetValues(taskId);
                        int index = SelectedItemList.IndexOf(SelectedModel);
                        SelectedItemList.ReplaceItem(index, SelectedModel);
                        LstFiles.Clear();
                        LstFiles.AddRange(infos);
                        Task task = new Task(async () =>
                        {
                            if (IsNetworkConnected)
                            {
                                Webservice.UploadFile(true, SelectedModel.Id, postId, true, bytes, extension, "", result.Text, DateTime.Now, true, out uploadFileResult, out uploadFileResultSpecified);
                                if (uploadFileResult > 0)
                                {
                                    var index = LstFiles.IndexOf(info);
                                    info.FileID = uploadFileResult;
                                    info.FileIDSpecified = uploadFileResultSpecified;
                                    info.IsOffline = false;
                                    if (index >= 0)
                                        LstFiles.ReplaceItem(index, info);
                                    updateSelectedItem();
                                }
                            }
                        });
                        task.Start();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    CrashReportManager.ReportError(ex, System.Reflection.MethodBase.GetCurrentMethod().Name);
                }
            });
        }

        /// <summary>
        /// To get the bytes from image.
        /// </summary>
        /// <param name="photo"></param>
        /// <returns></returns>
        byte[] LoadPhotoAsync(FileResult photo)
        {
            byte[] photoBytes = null;
            // canceled
            if (photo != null)
            {
                System.IO.FileStream stream1 = System.IO.File.OpenRead(photo.FullPath);

                var b = new byte[stream1.Length];

                stream1.Read(b, 0, b.Length);
                photoBytes = ImageCompressionService.CompressImageBytes(b, 30);

            }
            return photoBytes;
        }

        /// <summary>
        /// To select the picture from galary.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public void PickPhotoAsync(FileInfo file)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                MediaPickerOptions option = new MediaPickerOptions();
                option.Title = "mTIM";
                var fileinfo = await MediaPicker.PickPhotoAsync(option);
                var bytes = LoadPhotoAsync(fileinfo);
                var extension = System.IO.Path.GetExtension(fileinfo.FullPath);
                await Task.Delay(500);
                PromptConfig config = new PromptConfig();
                config.Title = "Datei hinzufuegen?";
                config.InputType = InputType.Name;
                config.Message = "Wollen Sie die Datei hinzufuegen?";
                config.Placeholder = "Kommentar";
                config.OkText = "Ja";
                config.CancelText = "Nein";
                var result = await UserDialogs.Instance.PromptAsync(config);
                if (result.Ok)
                {
                    int postId = FileHelper.GenerateAndGetOfflineID();
                    int uploadFileResult;
                    bool uploadFileResultSpecified = true;

                    FileInfo info = new FileInfo();
                    List<FileInfo> infos = new List<FileInfo>();
                    info.FileID = postId;
                    info.Comment = string.IsNullOrEmpty(result.Text) ? postId.ToString() : result.Text;
                    info.IsOffline = true;
                    info.OfflineFilePath = fileinfo.FullPath;
                    info.FileIDSpecified = uploadFileResultSpecified;
                    int taskId = SelectedModel.Id;
                    FileInfoHelper.Instance.UpdateValueInList(taskId, info);
                    infos = GetValues(taskId);
                    LstFiles.Clear();
                    LstFiles.AddRange(infos);
                    Task task = new Task(async () =>
                    {
                        if (IsNetworkConnected)
                        {
                            Webservice.UploadFile(true, SelectedModel.Id, postId, true, bytes, extension, "", result.Text, DateTime.Now, true, out uploadFileResult, out uploadFileResultSpecified);
                            if (uploadFileResult > 0)
                            {
                                var index = LstFiles.IndexOf(info);
                                info.FileID = uploadFileResult;
                                info.FileIDSpecified = uploadFileResultSpecified;
                                info.IsOffline = false;
                                if (index >= 0)
                                    LstFiles.ReplaceItem(index, info);
                                updateSelectedItem();
                            }
                        }
                    });
                    task.Start();
                }
            });
        }

        /// <summary>
        /// To update the selected model.
        /// </summary>
        private void updateSelectedItem()
        {
            int index = SelectedItemList.IndexOf(SelectedModel);
            SelectedItemList.ReplaceItem(index, SelectedModel);
        }

        #endregion

        public override void OnAppearing()
        {
            base.OnAppearing();
            TimerHelper.Instance.StartTimer();
        }

        public override void OnDisAppearing()
        {
            base.OnDisAppearing();
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

        private bool isOpenUpdateOptions;
        public bool IsOpenUpdateOptions
        {
            get => isOpenUpdateOptions;
            set => SetAndRaisePropertyChanged(ref isOpenUpdateOptions, value);
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

        private string notificationCount = "0";
        public string NotificationCount
        {
            get => notificationCount;
            set => SetAndRaisePropertyChanged(ref notificationCount, value);
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

        private bool isNotificationVisible = false;
        public bool IsNotificationVisible
        {
            get => isNotificationVisible;
            set => SetAndRaisePropertyChanged(ref isNotificationVisible, value);
        }

        private string updateVersionText = "";
        public string UpdateVersionText
        {
            get => updateVersionText;
            set => SetAndRaisePropertyChanged(ref updateVersionText, value);
        }

        private bool isShowGalaryIcon = false;
        public bool IsShowGalaryIcon
        {
            get => isShowGalaryIcon;
            set => SetAndRaisePropertyChanged(ref isShowGalaryIcon, value);
        }

        /// <summary>
        /// Update the selected element in List.
        /// </summary>
        /// <param name="id"></param>
        public void SlectedElementPositionIn3D(int id)
        {
            if (id <= 1)
            {
                return;
            }
            var selectedElement = TimTaskListHelper.GetItem(id);
            if (selectedElement != null)
            {
                SelectedItemList.Clear();
                SelectedItemList.AddRange(TimTaskListHelper.GetParentsFromChildren(selectedElement.Parent, selectedElement.Level));
                UpdateIndexSelection(id);
                UpdateListSelection?.Invoke(id);
                previousId = selectedElement.Parent;
                headerStrings = new List<string>();
                AddHeaders(selectedElement);
                headerStrings.Reverse();
                updateHeaderTexts();
                if (GlobalConstants.IsLandscape)
                    ActionSelectedItemText?.Invoke(selectedElement.Name);
            }
        }

        /// <summary>
        ///To add the header strings.
        /// </summary>
        /// <param name="selectedTask"></param>
        private void AddHeaders(TimTaskModel selectedTask)
        {
            int parentId = selectedTask.Parent;
            while (parentId > 0)
            {
                var item = TimTaskListHelper.GetItem(parentId);
                item.IsSelected = true;
                headerStrings.Add(item.Name);
                parentId = item.Parent;
            }
        }

        /// <summary>
        /// To update the header data and list based on selection.
        /// </summary>
        List<string> headerStrings = new List<string>();
        private int previousId;
        public void SelectedItemIndex(int position)
        {
            if (position >= 0)
            {
                ListBackgroundColor = GlobalConstants.IsLandscape ? Color.White : Color.DimGray;
                var selectedItem = SelectedItemList[position];
                if (selectedItem != null && selectedItem.HasChilds)
                {
                    previousId = selectedItem.Id;
                    IsShowBackButton = true;
                    //SelectedItemText = selectedItem.Name;
                    headerStrings.Add(selectedItem.Name);
                    updateHeaderTexts();
                    SelectedItemList.Clear();
                    SelectedItemList.AddRange(selectedItem.Childrens);
                }
                else
                {
                    RightIconCommand(selectedItem);
                }
            }
        }

        private TimTaskModel selectedItem;
        /// <summary>
        /// To Update item selection in the list.
        /// </summary>
        /// <param name="id"></param>
        public void UpdateIndexSelection(int id)
        {
            if (selectedItem != null)
            {
                var pindex = SelectedItemList.IndexOf(selectedItem);
                if (pindex >= 0)
                {
                    selectedItem.IsSelected = false;
                    SelectedItemList.ReplaceItem(pindex, selectedItem);
                }
                else
                {
                    SelectedItemList.ToList().ForEach(x => x.IsSelected = false);
                }
            }
            selectedItem = SelectedItemList.Where(x => x.Id.Equals(id)).FirstOrDefault();
            var index = SelectedItemList.IndexOf(selectedItem);
            selectedItem.IsSelected = true;
            SelectedItemList.ReplaceItem(index, selectedItem);
        }

        /// <summary>
        /// Value click in list.
        /// </summary>
        /// <param name="model"></param>
        public void SelectedValueItem(TimTaskModel model)
        {
            openValues(model);
        }

        /// <summary>
        /// Document click in list.
        /// </summary>
        /// <param name="id"></param>
        private void SelectedDocumentItem(int id)
        {
            openDocumentValues(id);
        }

        /// <summary>
        /// To open the seleceted document.
        /// </summary>
        /// <param name="id"></param>
        private void openDocumentValues(int id)
        {
            var values = GetValues(id);
            LstFiles?.Clear();
            if (values?.Count >= 0)
            {
                IsDocumentListVisible = true;
                updateTaskListVisibility();
                LstFiles.AddRange(values);
            }
        }

        /// <summary>
        /// To get the values of selected item.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private List<FileInfo> GetValues(int id)
        {
            var values = FileInfoHelper.Instance.GetValues(id);
            values?.ForEach(x => x.IsShowDeleteIcon = IsShowGalaryIcon);
            return values;
        }

        /// <summary>
        /// This is used to navigate back to the previsous sate.
        /// </summary>
        public ICommand BackButtonCommand => new Command(BackButtonCommandExecute);
        private void BackButtonCommandExecute()
        {
            if (IsValueListVisible || IsDocumentListVisible || IsEditTextVisible)
            {
                IsDocumentListVisible = IsValueListVisible = IsEditTextVisible = false;
                updateTaskListVisibility();
                return;
            }
            if (previousId >= 0)
            {
                ListBackgroundColor = GlobalConstants.IsLandscape ? Color.White : Color.DimGray;
                var selectedItem = TimTaskListHelper.GetItem(previousId);
                if (selectedItem != null)
                {
                    previousId = selectedItem.Parent;
                    IsShowBackButton = true;
                    headerStrings.RemoveAt(headerStrings.Count - 1);
                    updateHeaderTexts();
                    //SelectedItemText = selectedItem.Name;
                    SelectedItemList.ToList().ForEach(x => x.IsSelected = false);
                    SelectedItemList.Clear();
                    SelectedItemList.AddRange(TimTaskListHelper.GetParentsFromChildren(selectedItem.Parent, selectedItem.Level));
                    var item = SelectedItemList.Where(x => x.IsSelected == true).FirstOrDefault();
                    if (item != null && GlobalConstants.IsLandscape)
                    {
                        UpdateDrawing?.Invoke(item.Id);
                        ActionSelectedItemText?.Invoke(selectedItem.Name);
                    }
                }
            }
            //RefreshData();
        }

        /// <summary>
        /// This is used to show the respective data based on selection.
        /// </summary>
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
                    case DataType.Float:
                        if (!string.IsNullOrEmpty(SelectedModel.Range))
                        {
                            IsValueListVisible = true;
                            updateTaskListVisibility();
                            LstValues.AddRange(SelectedModel.Values);
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
                        break;
                    case DataType.Doc:
                        IsShowGalaryIcon = false;
                        if (!string.IsNullOrEmpty(SelectedModel.Range))
                        {
                            if (SelectedModel.Range.ToLower().Equals("mtim"))
                            {
                                IsShowGalaryIcon = true;
                            }
                        }
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

        /// <summary>
        /// This is used to update the selected value in main list.
        /// </summary>
        public void SelectedValue(Value item)
        {
            IsValueListVisible = false;
            if (item != null)
            {
                SelectedModel.UpdateValue(item.Data);
            }
            updateTaskListVisibility();
            SaveTaskList();
        }


        /// <summary>
        /// This is used to show the selected document.
        /// </summary>
        public async void OpenDocument(FileInfo file)
        {
            if (file != null)
            {
                if (!string.IsNullOrEmpty(file.OfflineFilePath))
                {
                    var mime = MimeTypesMap.GetMimeType(file.OfflineFilePath);
                    UserDialogs.Instance.ShowLoading(string.Empty, MaskType.Gradient);
                    await Launcher.OpenAsync(new OpenFileRequest
                    {
                        File = new ReadOnlyFile(file.OfflineFilePath, mime)
                    });
                    UserDialogs.Instance.HideLoading();
                }
                else
                {
                    Webservice.ViewModel = this;
                    Webservice.OpenFile(Math.Abs(file.FileID), file.FileIDSpecified);
                }
            }
        }

        /// <summary>
        /// To update the task list visibility.
        /// </summary>
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

        /// <summary>
        /// Update the header texts.
        /// </summary>
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
                SelectedItemText = TimTaskListHelper.GetItem(0)?.Name;
                SubText = string.Empty;
            }
            else
            {
                IsShowBackButton = true;
                SubText = string.Empty;
            }
            ActionSelectedItemText?.Invoke(SelectedItemText);
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
            AnalyticsManager.TrackEvent(System.Reflection.MethodBase.GetCurrentMethod().Name, AnalyticsType.ClickorSelected);
            IsOpenMenuOptions = true;
        }

        public ICommand BarcodeClickCommand => new Command(BarcodeClickCommandExecute);

        private async void BarcodeClickCommandExecute()
        {
            AnalyticsManager.TrackEvent(System.Reflection.MethodBase.GetCurrentMethod().Name, AnalyticsType.ClickorSelected);
            //IsOpenBarcodeView = true;
            IsScanning = true;
            //#if __ANDROID__
            //            // Initialize the scanner first so it can track the current context
            //            MobileBarcodeScanner.Initialize (Application);
            //#endif
            //            var scanner = new ZXing.Mobile.MobileBarcodeScanner();

            //            var result = await scanner.Scan();

            //            HandleResult(result);
            await Navigation.PushModalAsync(new NavigationPage(new BarcodeContentPage(this)), true);
        }

        public ICommand MessageClickCommand => new Command(MessageClickCommandExecute);
        private void MessageClickCommandExecute()
        {
            AnalyticsManager.TrackEvent(System.Reflection.MethodBase.GetCurrentMethod().Name, AnalyticsType.ClickorSelected);
            IsOpenUpdateOptions = true;
            UpdateVersionText = string.Format(AppResources.InstallUpdate, VersionInfo?.Version);
        }

        public ICommand MenuUpdateAppCommand => new Command(MenuUpdateAppCommandExecute);
        private void MenuUpdateAppCommandExecute()
        {
            AnalyticsManager.TrackEvent(System.Reflection.MethodBase.GetCurrentMethod().Name, AnalyticsType.ClickorSelected);
            IsOpenUpdateOptions = false;
            Device.DownloadAndInstallAPK(VersionInfo);
        }

        public ICommand CloseBarcodeCommand => new Command(CloseBarcodeCommandExecute);
        private void CloseBarcodeCommandExecute()
        {
            AnalyticsManager.TrackEvent(System.Reflection.MethodBase.GetCurrentMethod().Name, AnalyticsType.ClickorSelected);
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

            Xamarin.Forms.Device.BeginInvokeOnMainThread(async () =>
            {
                await Application.Current.MainPage.DisplayAlert("", msg, AppResources.Ok);
            });
        }

        public ICommand MenuSettingsItemCommand => new Command(MenuSettingsCommandExecute);
        private async void MenuSettingsCommandExecute()
        {
            AnalyticsManager.TrackEvent(System.Reflection.MethodBase.GetCurrentMethod().Name, AnalyticsType.ClickorSelected);
            IsOpenMenuOptions = false;
            await Navigation.PushModalAsync(new NavigationPage(new SettingsPage()));
        }

        public ICommand MenuInfoItemCommand => new Command(MenuInfoItemCommandExecute);
        private async void MenuInfoItemCommandExecute()
        {
            AnalyticsManager.TrackEvent(System.Reflection.MethodBase.GetCurrentMethod().Name, AnalyticsType.ClickorSelected);
            IsOpenMenuOptions = false;
            var locationstring = string.Format($"{GlobalConstants.LocationDetails?.Longitude} | {GlobalConstants.LocationDetails?.Latitude}");
            await Application.Current.MainPage.DisplayAlert(LabelInfo, string.Format($"IMEI: {GlobalConstants.IMEINumber}\nDeviceID: {GlobalConstants.IMEINumber}\nUniqueID: {GlobalConstants.UniqueID}\nURL: {GlobalConstants.AppBaseURL}\nVersion: {GlobalConstants.VersionNumber}\nLocation: {locationstring}"), AppResources.Ok);
        }

        public ICommand MenuRefreshItemCommand => new Command(MenuRefreshItemCommandExecute);
        private void MenuRefreshItemCommandExecute()
        {
            AnalyticsManager.TrackEvent(System.Reflection.MethodBase.GetCurrentMethod().Name, AnalyticsType.ClickorSelected);
            IsOpenMenuOptions = false;
            OnSyncCommand(false);
        }

        /// <summary>
        /// This is used to refersh complete data and switch back to first level.
        /// </summary>
        public override void RefreshData()
        {
            AnalyticsManager.TrackEvent(System.Reflection.MethodBase.GetCurrentMethod().Name);
            headerStrings = new List<string>();
            updateHeaderTexts();
            IsDocumentListVisible = IsValueListVisible = IsEditTextVisible = false;
            updateTaskListVisibility();
            ListBackgroundColor = Color.White;
            IsOpenMenuOptions = false;
            IsOpenUpdateOptions = false;
            IsShowBackButton = false;
            SelectedItemList.Clear();
            SelectedItemList.AddRange(TimTaskListHelper.GetParentsFromChildren(0, 1));
        }

        public async void UpdateList()
        {
            try
            {
                Crashes.GenerateTestCrash();
                SearchForNewApp();
                if (!FileHelper.IsFileExists(GlobalConstants.TASKLIST_FILE))
                {
                    Webservice.ViewModel = this;
                    Webservice.GetTasksListIDsFortheData();
                }
                else
                {
                    OnSyncCommand(true);
                    string json = await FileHelper.ReadTextAsync(GlobalConstants.TASKLIST_FILE);
                    Debug.WriteLine("Task List: " + json);
                    var list = JsonConvert.DeserializeObject<List<TimTaskModel>>(json);
                    if (list != null)
                    {
                        list.UpdateList();
                        RefreshData();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                CrashReportManager.ReportError(ex, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void AppData(string json)
        {
            if (!string.IsNullOrEmpty(json))
            {
                AnalyticsManager.TrackEvent(System.Reflection.MethodBase.GetCurrentMethod().Name);
                VersionInfo = JsonConvert.DeserializeObject<AppVersionUpdateInfo>(json);
                IsNotificationVisible = true;
                NotificationCount = "1";
                LabelAppUpdates = AppResources.AppUpdates;
                AppUpdateTextColor = Color.Black;
            }
        }

        /// <summary>
        /// To sync the data to server after time interval completed.
        /// </summary>
        /// <param name="isFromAuto"></param>
        public override void OnSyncCommand(bool isFromAuto = true)
        {
            SearchForNewApp();
            base.OnSyncCommand(isFromAuto);
        }

        public ICommand MenuNewBuildItemCommand => new Command(MenuNewBuildItemCommandExecute);
        private void MenuNewBuildItemCommandExecute()
        {
            SearchForNewApp();
        }

        public ICommand OnCommentClickedCommand => new Command(CommentClickExecute);
        private async void CommentClickExecute(object parameter)
        {
            if(!IsShowGalaryIcon)
            {
                var document = parameter as FileInfo;
                OpenDocument(document);
                return;
            }
            var selectedItem = parameter as FileInfo;
            PromptConfig config = new PromptConfig();
            config.Title = "Comment";
            config.InputType = InputType.Name;
            config.Message = "Please edit you comment.";
            config.Placeholder = "Kommentar";
            config.Text = selectedItem.Comment;
            config.OkText = "Ok";
            config.CancelText = "Cancel";
            var result = await UserDialogs.Instance.PromptAsync(config);
            if (result.Ok)
            {
                var index = LstFiles.IndexOf(selectedItem);
                selectedItem.Comment = string.IsNullOrEmpty(result.Text) ? selectedItem.FileID.ToString() : result.Text;
                if (selectedItem.IsOffline)
                {
                    FileInfoHelper.Instance.UpdateFileInfo(SelectedModel.Id, index, selectedItem);
                    LstFiles.ReplaceItem(index, selectedItem);
                }
                else
                {
                    selectedItem.IsCommentEdited = true;
                    FileInfoHelper.Instance.UpdateFileInfo(SelectedModel.Id, index, selectedItem);
                    LstFiles.ReplaceItem(index, selectedItem);
                    if (IsNetworkConnected && !selectedItem.IsOffline)
                    {
                        Webservice.ChangeFileComment(SelectedModel.Id, selectedItem.FileID, selectedItem.FileIDSpecified, selectedItem.Comment);
                    }
                }
            }
        }

        public ICommand OnDeleteClickedCommand => new Command(DeleteClickExecute);
        private void DeleteClickExecute(object parameter)
        {
            var selectedItem = parameter as FileInfo;
            if (selectedItem != null && selectedItem.IsOffline)
            {
                FileInfoHelper.Instance.DeleteValueInList(SelectedModel.Id, selectedItem);
                LstFiles.Remove(selectedItem);
                updateSelectedItem();
            }
            else
            {
                if (IsNetworkConnected)
                {
                    Webservice.DeleteFile(SelectedModel.Id, selectedItem.FileID, selectedItem.FileIDSpecified);
                    FileInfoHelper.Instance.DeleteValueInList(SelectedModel.Id, selectedItem);
                }
                else
                {
                    var index = LstFiles.IndexOf(selectedItem);
                    selectedItem.IsDeleted = true;
                    FileInfoHelper.Instance.UpdateFileInfo(SelectedModel.Id, index, selectedItem);
                }
                var files = GetValues(SelectedModel.Id);
                LstFiles.Clear();
                LstFiles.AddRange(files);
                updateSelectedItem();
            }
        }

        public ICommand OnViewClickedCommand => new Command(ViewClickExecute);
        private void ViewClickExecute(object parameter)
        {
            var selectedItem = parameter as FileInfo;
            OpenDocument(selectedItem);
        }

        private void SearchForNewApp()
        {
            IsNotificationVisible = false;
            LabelAppUpdates = AppResources.CheckNewVersion; // "Checking new version";
            AppUpdateTextColor = Color.LightGray;
            Webservice.QueryAppUpdate(AppData);
        }

        private async void RightIconCommand(TimTaskModel item)
        {
            if (item != null)
            {
                switch (item.Type)
                {
                    case DataType.Prjladen:
                    case DataType.Prjladen2:
                        await Application.Current.MainPage.DisplayAlert(AppResources.LoadTaskList, item.Value?.ToString(), AppResources.Ok, AppResources.Cancel);
                        break;
                    case DataType.Aktion:
                    case DataType.Aktion2:
                        item.Value = DateTime.Now.ToString("HH:mm:ss");
                        break;
                    default:
                        break;
                }
                SaveTaskList();
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
            IsEditTextVisible = false;
            updateTaskListVisibility();
            SaveTaskList();
        }
    }
}
