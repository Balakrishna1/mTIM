using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using HeyRed.Mime;
using mTIM.Enums;
using mTIM.Helpers;
using mTIM.Interfaces;
using mTIM.Managers;
using mTIM.Models;
using mTIM.Models.D;
using mTIM.Resources;
using Newtonsoft.Json;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Xamarin.Essentials;
using Xamarin.Forms;
using PermissionStatus = Plugin.Permissions.Abstractions.PermissionStatus;

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
            LoadMesh();
        }



        #region Intialze the data.

        public void Intialize()
        {
            Task.Run(async () =>
            {
                if (!await GetPermissions())
                {
                    Device.CloseApplication();
                }
                else
                {
                    InitializeData();
                }
            });
        }

        /// <summary>
        /// Intialize the data
        /// </summary>
        /// <returns></returns>
        private async Task InitializeData()
        {
            try
            {
                Task task = Task.Run(async () =>
                {
#if DEBUG
                    GlobalConstants.AppBaseURL = "http://mtimtest.precast-software.com:7778";
#else
                    GlobalConstants.AppBaseURL = string.Empty;
#endif
                    getLocation();
                    if (!FileHelper.IsFileExists(GlobalConstants.IMEI_FILE))
                    {
                        getVersionInfo();
                        getDeviceInfo();
                        SaveAppMessage();
                    }
                    else
                    {
                        string jsonIMEI = FileHelper.ReadText(GlobalConstants.IMEI_FILE);
                        Debug.WriteLine("mTIM Device JSON:" + jsonIMEI);
                        AndroidMessageModel messageModel = JsonConvert.DeserializeObject<AndroidMessageModel>(jsonIMEI);
                        if (messageModel != null)
                        {
                            MessageModel = messageModel;
                            GlobalConstants.DeviceID = messageModel.DeviceId;
                            GlobalConstants.IMEINumber = messageModel.IMEI;
                            GlobalConstants.UniqueID = messageModel.PseudoID;
                            GlobalConstants.VersionCode = messageModel.VersionCode;
                            GlobalConstants.VersionNumber = messageModel.VersionName;
                        }
                    }

                    if (FileHelper.IsFileExists(GlobalConstants.SETTINGS_FILE))
                    {
                        string jsonSettings = FileHelper.ReadText(GlobalConstants.SETTINGS_FILE);
                        Debug.WriteLine("mTIM Device JSON:" + jsonSettings);
                        SettingsModel messageModel = JsonConvert.DeserializeObject<SettingsModel>(jsonSettings);
                        if (messageModel != null)
                        {
                            GlobalConstants.SelectedLanguage = messageModel.Language;
                            SelectedLanguage = messageModel.Language;
                            UpdateLanguage(SelectedLanguage);
                            GlobalConstants.AppBaseURL = messageModel.BaseUrl;
                            GlobalConstants.StatusSyncTime = SyncTime = messageModel.StatusSyncTime;
                            GlobalConstants.SyncMinutes = SyncMinites = messageModel.StatusSyncMinutes;
                        }
                    }

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
                });
                UserDialogs.Instance.ShowLoading("Intializing..", MaskType.Gradient);
                await Task.WhenAll(task);
                UserDialogs.Instance.HideLoading();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                UserDialogs.Instance.HideLoading();
                CrashReportManager.ReportError(ex, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// This method is used to update the app version.
        /// </summary>
        private void getVersionInfo()
        {
            GlobalConstants.VersionNumber = VersionTracking.CurrentVersion;
            GlobalConstants.VersionCode = VersionTracking.CurrentBuild;
        }

        /// <summary>
        /// this method is used to update the device information.
        /// </summary>
        private void getDeviceInfo()
        {
            IDevice device = DependencyService.Get<IDevice>();
            GlobalConstants.DeviceID = device.GetDeviceID();
            GlobalConstants.IMEINumber = device.GetImeiNumeber();
            GlobalConstants.UniqueID = device.GetUniqueID();
        }

        /// <summary>
        /// To Save device information
        /// </summary>
        public async void SaveAppMessage()
        {
            AnalyticsManager.TrackEvent(System.Reflection.MethodBase.GetCurrentMethod().Name);
            AndroidMessageModel androidMessage = new AndroidMessageModel();
            androidMessage.Brand = DeviceInfo.Manufacturer;
            androidMessage.Device = DeviceInfo.Model;
            androidMessage.DeviceId = GlobalConstants.DeviceID;
            androidMessage.IMEI = GlobalConstants.IMEINumber;
            androidMessage.PseudoID = GlobalConstants.UniqueID;
            androidMessage.VersionCode = GlobalConstants.VersionCode;
            androidMessage.VersionName = GlobalConstants.VersionNumber;
            string content = JsonConvert.SerializeObject(androidMessage);
            Debug.WriteLine("mTIM Device JSON:" + content);
            await FileHelper.WriteTextAsync(GlobalConstants.IMEI_FILE, content);
        }

        #endregion

        #region To get location information

        /// <summary>
        /// To get the location.
        /// </summary>
        private async void getLocation()
        {
            try
            {
                var location = await Geolocation.GetLastKnownLocationAsync();

                if (location != null)
                {
                    fillLocationInfo(location);
                    Debug.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
                }
                else
                {
                    await GetCurrentLocation();
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Handle not supported on device exception
            }
            catch (FeatureNotEnabledException fneEx)
            {
                // Handle not enabled on device exception
            }
            catch (PermissionException pEx)
            {
                // Handle permission exception
            }
            catch (Exception ex)
            {
                // Unable to get location
                CrashReportManager.ReportError(ex, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// To get the current location.
        /// </summary>
        CancellationTokenSource cts;
        private async Task GetCurrentLocation()
        {
            try
            {
                AnalyticsManager.TrackEvent(System.Reflection.MethodBase.GetCurrentMethod().Name);
                var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                cts = new CancellationTokenSource();
                var location = await Geolocation.GetLocationAsync(request, cts.Token);

                if (location != null)
                {
                    fillLocationInfo(location);
                    Debug.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Handle not supported on device exception
            }
            catch (FeatureNotEnabledException fneEx)
            {
                // Handle not enabled on device exception
            }
            catch (PermissionException pEx)
            {
                // Handle permission exception
            }
            catch (Exception ex)
            {
                // Unable to get location
                CrashReportManager.ReportError(ex, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// This is used to update the location information.
        /// </summary>
        /// <param name="location"></param>
        private void fillLocationInfo(Xamarin.Essentials.Location location)
        {
            GlobalConstants.LocationDetails = new Models.Location()
            {
                Longitude = location.Longitude,
                Latitude = location.Latitude
            };
        }
        #endregion


        #region Permissions

        /// <summary>
        /// This is used to get the list of permission required for the app.
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> GetPermissions()
        {
            bool permissionsGranted = true;

            var permissionsStartList = new List<Permission>()
            {
                Permission.Phone,
                Permission.Camera,
                Permission.LocationWhenInUse,
                Permission.Storage,
            };

            var permissionsNeededList = new List<Permission>();
            foreach (var permission in permissionsStartList)
            {
                PermissionStatus status;
                switch (permission)
                {
                    case Permission.Phone:
                        status = await CrossPermissions.Current.CheckPermissionStatusAsync<PhonePermission>();
                        break;
                    case Permission.Camera:
                        status = await CrossPermissions.Current.CheckPermissionStatusAsync<CameraPermission>();
                        break;
                    case Permission.Storage:
                        status = await CrossPermissions.Current.CheckPermissionStatusAsync<StoragePermission>();
                        break;
                    case Permission.LocationWhenInUse:
                        status = await CrossPermissions.Current.CheckPermissionStatusAsync<LocationWhenInUsePermission>();
                        break;
                    default:
                        status = PermissionStatus.Unknown;
                        break;

                }
                if (status != PermissionStatus.Granted)
                {
                    permissionsNeededList.Add(permission);
                }
            }

            List<PermissionStatus> permissionsResult = new List<PermissionStatus>();

            foreach (var permission in permissionsNeededList)
            {
                PermissionStatus status;
                switch (permission)
                {
                    case Permission.Phone:
                        status = await CrossPermissions.Current.RequestPermissionAsync<PhonePermission>();
                        break;
                    case Permission.Camera:
                        status = await CrossPermissions.Current.RequestPermissionAsync<CameraPermission>();
                        break;
                    case Permission.Storage:
                        status = await CrossPermissions.Current.RequestPermissionAsync<StoragePermission>();
                        break;
                    case Permission.LocationWhenInUse:
                        //var results = await CrossPermissions.Current.RequestPermissionsAsync(Permission.LocationWhenInUse);
                        //status = results[Permission.LocationWhenInUse];
                        status = await CrossPermissions.Current.RequestPermissionAsync<LocationAlwaysPermission>();
                        break;
                    default:
                        status = PermissionStatus.Unknown;
                        break;

                }
                permissionsResult.Add(status);
            }
            try
            {
                foreach (var permission in permissionsResult)
                {
                    if (permission == PermissionStatus.Granted || permission == PermissionStatus.Unknown)
                    {
                        permissionsGranted = true;
                    }
                    else
                    {
                        permissionsGranted = false;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                CrashReportManager.ReportError(ex, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
            return permissionsGranted;
        }
        #endregion

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
        /// <param name="postId"></param>
        /// <param name="fileId"></param>
        /// <param name="UploadFileId"></param>
        /// <param name="UploadFileSpecified"></param>
        private void FileUploadCompleted(int postId, int fileId, int UploadFileId, bool UploadFileSpecified)
        {
            var item = LstFiles.Where(x => x.FileID.Equals(fileId)).FirstOrDefault();
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
        /// <param name="postId"></param>
        /// <param name="fileId"></param>
        private void EditCommentCompleted(int postId, int fileId)
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
        public void CapturePhotoAsync()
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    MediaPickerOptions option = new MediaPickerOptions();
                    option.Title = "mTIM";
                    var fileinfo = await MediaPicker.CapturePhotoAsync(option);
                    if (fileinfo != null)
                    {
                        AddFile(fileinfo);
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
        /// To select the picture from galary.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public void PickPhotoAsync()
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    MediaPickerOptions option = new MediaPickerOptions();
                    option.Title = "mTIM";
                    var fileinfo = await MediaPicker.PickPhotoAsync(option);
                    if (fileinfo != null)
                    {
                        AddFile(fileinfo);
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
        /// To get the bytes from image path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        byte[] LoadPhotoFromPathAsync(string path)
        {
            byte[] photoBytes = null;
            // canceled
            if (path != null)
            {
                System.IO.FileStream stream1 = System.IO.File.OpenRead(path);

                var b = new byte[stream1.Length];

                stream1.Read(b, 0, b.Length);
                photoBytes = ImageCompressionService.CompressImageBytes(b, 30);

            }
            return photoBytes;
        }

        private void AddFile(FileResult fileinfo)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(500);
                PromptConfig config = new PromptConfig();
                config.Title = AppResources.AddFile;
                config.InputType = InputType.Name;
                config.Message = AppResources.WantToAddFile;
                config.Placeholder = AppResources.Comment;
                config.OkText = AppResources.Yes;
                config.CancelText = AppResources.No;
                var result = await UserDialogs.Instance.PromptAsync(config);
                if (result.Ok)
                {
                    int fileId = FileHelper.GenerateAndGetOfflineID();
                    FileInfo info = new FileInfo();
                    info.FileID = fileId;
                    info.Comment = string.IsNullOrEmpty(result.Text) ? fileId.ToString() : result.Text;
                    info.IsOffline = true;
                    info.OfflineFilePath = fileinfo.FullPath;
                    info.FileIDSpecified = false;
                    info.ShouldBeDelete = true;
                    var addfileTask = new Task(async () => await AddOrUpdateFile(info, SelectedModel.Id, string.IsNullOrEmpty(result.Text) ? fileId.ToString() : result.Text, FileType.Add));
                    addfileTask.Start();
                }
            });
        }

        /// <summary>
        /// This is used for add or update or delete the file.
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="postId"></param>
        /// <param name="comment"></param>
        /// <param name="fileType"></param>
        /// <returns></returns>
        private async Task AddOrUpdateFile(FileInfo fileInfo, int postId, string comment, FileType fileType)
        {
            switch (fileType)
            {
                case FileType.Add:
                    var bytes = LoadPhotoFromPathAsync(fileInfo.OfflineFilePath);
                    var extension = System.IO.Path.GetExtension(fileInfo.OfflineFilePath);
                    int uploadFileResult;
                    bool uploadFileResultSpecified;
                    await FileInfoHelper.Instance.UpdateValueInList(postId, fileInfo);
                    var infos = GetValues(postId);
                    LstFiles.Clear();
                    LstFiles.AddRange(infos ?? new List<FileInfo>());
                    updateSelectedItem();
                    if (IsNetworkConnected)
                    {
                        Webservice.UploadFile(true, SelectedModel.Id, fileInfo.FileID, true, bytes, extension, string.Format("{0}|{1}", GlobalConstants.LocationDetails.Latitude, GlobalConstants.LocationDetails.Longitude), fileInfo.Comment, DateTime.Now, true, out uploadFileResult, out uploadFileResultSpecified);
                        if (uploadFileResult > 0)
                        {
                            var position = LstFiles.IndexOf(fileInfo);
                            fileInfo.FileID = uploadFileResult;
                            fileInfo.FileIDSpecified = uploadFileResultSpecified;
                            fileInfo.IsOffline = false;
                            if (position >= 0)
                                LstFiles.ReplaceItem(position, fileInfo);
                            await FileInfoHelper.Instance.UpdateFileInfoInList(postId, fileInfo.FileID, uploadFileResult, uploadFileResultSpecified);
                        }
                    }
                    break;
                case FileType.Update:
                    var index = LstFiles.IndexOf(fileInfo);
                    fileInfo.Comment = comment;
                    if (fileInfo.IsOffline)
                    {
                        await FileInfoHelper.Instance.UpdateFileInfo(postId, index, fileInfo);
                        LstFiles.ReplaceItem(index, fileInfo);
                    }
                    else
                    {
                        fileInfo.IsCommentEdited = true;
                        await FileInfoHelper.Instance.UpdateFileInfo(postId, index, fileInfo);
                        LstFiles.ReplaceItem(index, fileInfo);
                        if (IsNetworkConnected && !fileInfo.IsOffline)
                        {
                            Webservice.ChangeFileComment(SelectedModel.Id, fileInfo.FileID, fileInfo.FileIDSpecified, fileInfo.Comment);
                        }
                    }
                    break;
                case FileType.Delete:
                    if (fileInfo != null && fileInfo.IsOffline)
                    {
                        await FileInfoHelper.Instance.DeleteValueInList(postId, fileInfo);
                        LstFiles.Remove(fileInfo);
                    }
                    else
                    {
                        if (IsNetworkConnected)
                        {
                            Webservice.DeleteFile(postId, fileInfo.FileID, fileInfo.FileIDSpecified);
                            await FileInfoHelper.Instance.DeleteValueInList(SelectedModel.Id, fileInfo);
                        }
                        else
                        {
                            var fileIndex = LstFiles.IndexOf(fileInfo);
                            fileInfo.IsDeleted = true;
                            await FileInfoHelper.Instance.UpdateFileInfo(postId, fileIndex, fileInfo);
                        }
                        var files = GetValues(postId);
                        LstFiles.Clear();
                        LstFiles.AddRange(files);
                    }
                    updateSelectedItem();
                    break;
                default:
                    break;
            }
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
            if (cts != null && !cts.IsCancellationRequested)
                cts.Cancel();
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
            return FileInfoHelper.Instance.GetValues(id);
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
            Task.Run(() => PostResult(SelectedModel));
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
        public override async Task OnSyncCommand(bool isFromAuto = true)
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
            if (!IsShowGalaryIcon)
            {
                var document = parameter as FileInfo;
                OpenDocument(document);
                return;
            }
            var selectedItem = parameter as FileInfo;
            PromptConfig config = new PromptConfig();
            config.Title = AppResources.FileCommentHeader;
            config.InputType = InputType.Name;
            config.Message = AppResources.FileComment;
            config.Placeholder = AppResources.Comment;
            config.Text = selectedItem.Comment;
            config.OkText = AppResources.Ok;
            config.CancelText = AppResources.Cancel;
            var result = await UserDialogs.Instance.PromptAsync(config);
            if (result.Ok)
            {
                var editcommenTask = new Task(async () => await AddOrUpdateFile(selectedItem, SelectedModel.Id, string.IsNullOrEmpty(result.Text) ? selectedItem.FileID.ToString() : result.Text, FileType.Update));
                editcommenTask.Start();
            }
        }

        public ICommand OnDeleteClickedCommand => new Command(DeleteClickExecute);
        private void DeleteClickExecute(object parameter)
        {
            var selectedItem = parameter as FileInfo;
            var deleteFileTask = new Task(async ()=> await AddOrUpdateFile(selectedItem, SelectedModel.Id, string.Empty, FileType.Delete));
            deleteFileTask.Start();
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
                Task.Run(() => PostResult(SelectedModel));
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
            Task.Run(() => PostResult(SelectedModel));
        }

        private async Task PostResult(TimTaskModel item)
        {
            var list = await PostResultHelper.Instance.GetOfflineResults();
            var data = new TaskResult()
            {
                Gps = string.Format("{0}|{1}",
                GlobalConstants.LocationDetails.Latitude,
                GlobalConstants.LocationDetails.Longitude),
                PosId = item.Id,
                PosIdSpecified = true,
                TaskId = GlobalConstants.GetTaskListId(),
                TaskIdSpecified = true,
                Value = Convert.ToString(item.Value),
                Time = GlobalConstants.GetISODateTime()
            };
            list.Add(data);
            if (IsNetworkConnected)
            {
                FileHelper.DeleteFile(GlobalConstants.POST_RESULT);
                Webservice.PostReultAsync(JsonConvert.SerializeObject(list));
            }
            else
            {
                PostResultHelper.Instance.SaveTaskResults(list.ToArray());
            }
        }
    }
}
