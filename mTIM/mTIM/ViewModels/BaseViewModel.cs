using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using mTIM.Helpers;
using mTIM.Interfaces;
using mTIM.Models;
using mTIM.Models.D;
using mTIM.Resources;
using Newtonsoft.Json;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace mTIM.ViewModels
{
    public class BaseViewModel : BaseViewModelBase, INotifyPropertyChanged
    {
        public List<TimTaskModel> TotalListList = new List<TimTaskModel>();
        public IWebService Webservice { get; set; }
        public IImageCompressionService ImageCompressionService;
        public byte[] ImageSource { get; set; }
        public INavigation Navigation { get; set; }
        public bool IsNetworkConnected { get; set; }

        private const string syncTimeFormat = "({0}s)";

        private string stringSyncTime = string.Empty;
        public string StringSyncTime
        {
            get => stringSyncTime;
            set
            {
                SetAndRaisePropertyChanged(ref stringSyncTime, value);
            }
        }


        private bool isIncrementIocnVisible = true;
        public bool IsIncrementIocnVisible
        {
            get => isIncrementIocnVisible;
            set
            {
                SetAndRaisePropertyChanged(ref isIncrementIocnVisible, value);
            }
        }

        private bool isDecrementIocnVisible = true;
        public bool IsDecrementIocnVisible
        {
            get => isDecrementIocnVisible;
            set
            {
                SetAndRaisePropertyChanged(ref isDecrementIocnVisible, value);
            }
        }

        private int syncMinites = GlobalConstants.DefaultSyncMinites;
        public int SyncMinites
        {
            get => syncMinites;
            set
            {
                GlobalConstants.SyncMinutes = value;
                updateTimer();
                IsIncrementIocnVisible = value == 10 ? false : true;
                IsDecrementIocnVisible = value == 1 ? false : true;
                SetAndRaisePropertyChanged(ref syncMinites, value);
            }
        }

        private bool isRefreshBadgeVisible = false;
        public bool IsRefreshBadgeVisible
        {
            get => isRefreshBadgeVisible;
            set => SetAndRaisePropertyChanged(ref isRefreshBadgeVisible, value);
        }

        private void updateTimer()
        {
            StringSyncTime = string.Format(syncTimeFormat, SyncTime);
            TimerHelper.Instance.Dispose();
            TimerHelper.Instance.Create(CallBack);
            TimerHelper.Instance.StartTimer();
        }

        private int syncTime = 0;
        public int SyncTime
        {
            get => syncTime;
            set
            {
                GlobalConstants.StatusSyncTime = value;
                SetAndRaisePropertyChanged(ref syncTime, value);
            }
        }

        public BaseViewModel()
        {
            Webservice = DependencyService.Get<IWebService>();
            ImageCompressionService = DependencyService.Get<IImageCompressionService>();
            Webservice.ActionRefreshCallBack -= ShowBadge;
            Webservice.ActionRefreshCallBack += ShowBadge;
            SyncMinites = GlobalConstants.SyncMinutes > 0 ? GlobalConstants.SyncMinutes : GlobalConstants.DefaultSyncMinites;
            if (GlobalConstants.StatusSyncTime > 0)
            {
                SyncTime = GlobalConstants.StatusSyncTime;
            }
            else
            {
                SyncTime = SyncMinites * 60;
            }
            StringSyncTime = string.Format(syncTimeFormat, SyncTime);
        }

        public void ShowBadge(bool flag)
        {
            IsRefreshBadgeVisible = flag;
        }

        public void CallBack(object state)
        {
            SyncTime--;
            StringSyncTime = string.Format(syncTimeFormat, SyncTime == 0 ? "Sync" : SyncTime.ToString());
            Debug.WriteLine("{0} seconds left to sync.\n", SyncTime);
            if (SyncTime == 0)
            {
                Refresh();
            }
        }

        public void Refresh()
        {
            OnSyncCommand();
            SyncTime = SyncMinites * 60;
        }

        public async void UpdateList(string json)
        {
            //var json = CsvToJsonConverter.ConvertCsvToJson(csvList);
            if (!string.IsNullOrEmpty(json))
            {
                await FileHelper.WriteTextAsync(GlobalConstants.TASKLIST_FILE, json);
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

        public void UpdateChaildValues(List<TimTaskModel> list)
        {
            foreach (var item in list)
            {
                if (list.Where(x => x.Parent.Equals(item.Id)).ToList()?.Count > 0)
                {
                    item.HasChilds = true;
                }
            }
        }

        public ICommand CloseCommand => new Command(CloseCommandExecute);
        private async void CloseCommandExecute()
        {
            await Navigation?.PopModalAsync();
        }

        public override void OnAppearing()
        {
            if (GlobalConstants.StatusSyncTime > 0)
            {
                SyncTime = GlobalConstants.StatusSyncTime;
            }
            TimerHelper.Instance.Create(CallBack);
            base.OnAppearing();
            CheckNetworkConnection();
            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
        }

        public override void OnDisAppearing()
        {
            //TimerHelper.Instance.Dispose();
            base.OnDisAppearing();
            Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;
        }

        private void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            CheckNetworkConnection();
            UploadOfflineFilesIntoServer();
        }

        private void CheckNetworkConnection()
        {
            var networkStatus = Connectivity.NetworkAccess;
            if (networkStatus == NetworkAccess.Internet)
            {
                IsNetworkConnected = true;
            }
            else
            {
                IsNetworkConnected = false;
            }
        }

        public void UploadOfflineFilesIntoServer()
        {
            if (IsNetworkConnected)
            {
                var list = FileInfoHelper.Instance.TotalFilesList;
                if (list?.Count > 0)
                {
                    foreach (var item in list)
                    {
                        var offlineItems = item.Values.Where(x => x.IsOffline.Equals(true)).ToList();
                        var editedItems = item.Values.Where(x => x.IsCommentEdited.Equals(true)).ToList();
                        var deletedItems = item.Values.Where(x => x.IsDeleted.Equals(true)).ToList();
                        if (offlineItems != null && offlineItems.Count > 0)
                        {
                            UploadOfflineFiles(item.Key, offlineItems);
                        }
                        if (editedItems != null && editedItems.Count > 0)
                        {
                            UploadOfflineCommentFiles(item.Key, editedItems);
                        }
                        if (deletedItems != null && deletedItems.Count > 0)
                        {
                            UploadOfflineDeletedFiles(item.Key, deletedItems);
                        }
                    }
                }
            }
        }

        private void UploadOfflineFiles(int taskId, List<FileInfo> items)
        {
            foreach (var file in items)
            {
                Task.Run(() => Webservice.UploadFileAsync(true, taskId, file.FileID, file.FileIDSpecified, GetBytesFromPath(file.OfflineFilePath), System.IO.Path.GetExtension(file.OfflineFilePath), null, file.Comment, DateTime.Now, true));
            }
        }

        private void UploadOfflineCommentFiles(int taskId, List<FileInfo> items)
        {
            foreach (var file in items)
            {
                Task.Run(() => Webservice.ChangeFileComment(taskId, file.FileID, file.FileIDSpecified, file.Comment));
            }
        }

        private void UploadOfflineDeletedFiles(int taskId, List<FileInfo> items)
        {
            foreach (var file in items)
            {
                Task.Run(() => Webservice.DeleteFile(taskId, file.FileID, file.FileIDSpecified));
            }
        }

        private byte[] GetBytesFromPath(string photoPath)
        {
            byte[] photoBytes = null;
            // canceled
            if (!string.IsNullOrEmpty(photoPath))
            {
                System.IO.FileStream stream1 = System.IO.File.OpenRead(photoPath);

                var b = new byte[stream1.Length];

                stream1.Read(b, 0, b.Length);
                photoBytes = ImageCompressionService.CompressImageBytes(b, 30);

            }
            return photoBytes;
        }

        public void DisplayErrorMessage(string message)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Application.Current.MainPage.DisplayAlert(LabelInfo, message, AppResources.Ok);
            });
        }

        public virtual void OnSyncCommand(bool isFromAuto = true)
        {
            SaveTaskList();
            Webservice.ViewModel = this;
            Webservice.SyncTaskList(JsonConvert.SerializeObject(TotalListList), isFromAuto);
        }

        public void SaveTaskList()
        {
            if (TotalListList != null && TotalListList.Count > 0)
            {
                var json = JsonConvert.SerializeObject(TotalListList);
                FileHelper.WriteTextAsync(GlobalConstants.TASKLIST_FILE, json);
            }
        }

        public virtual void RefreshData()
        {

        }
    }
}
