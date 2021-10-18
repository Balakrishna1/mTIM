using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using mTIM.Helpers;
using mTIM.Interfaces;
using mTIM.Models;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace mTIM.ViewModels
{
    public class BaseViewModel : BaseViewModelBase, INotifyPropertyChanged
    {
        public List<TimTaskModel> TotalListList = new List<TimTaskModel>();
        public IWebService Webservice { get; set; }
        public byte[] ImageSource { get; set; }
        public INavigation Navigation { get; set; }

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
                SyncTime = value * 60;
                updateTimer();
                SetAndRaisePropertyChanged(ref syncMinites, value);
            }
        }

        private void updateTimer()
        {
            StringSyncTime = string.Format(syncTimeFormat, SyncTime);
            TimerHelper.Instance.Dispose();
            TimerHelper.Instance.Create(CallBack);
        }

        private int syncTime = GlobalConstants.DefaultSyncMinites * 60;
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
            if (GlobalConstants.StatusSyncTime > 0)
            {
                SyncTime = GlobalConstants.StatusSyncTime;
            }
            StringSyncTime = string.Format(syncTimeFormat, SyncTime);
        }

        public void CallBack(object state)
        {
            SyncTime--;
            StringSyncTime = string.Format(syncTimeFormat, SyncTime == 0 ? "Sync" : SyncTime.ToString());
            Console.WriteLine("{0:h:mm:ss.fff} Updating timer.\n", DateTime.Now);
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

        public async void UpdateList(string csvList)
        {
            var json = CsvToJsonConverter.ConvertCsvToJson(csvList);
            if (!string.IsNullOrEmpty(json))
            {
                if (!FileHelper.IsFileExists(GlobalConstants.TASKLIST_FILE))
                {
                    await FileHelper.WriteTextAsync(GlobalConstants.TASKLIST_FILE, json);
                }
                var list = JsonConvert.DeserializeObject<List<TimTaskModel>>(json);
                TotalListList.Clear();
                if (list != null)
                {
                    TotalListList.AddRange(list);
                    RefreshData();
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
            TimerHelper.Instance.Create(CallBack);
            base.OnAppearing();
        }
        
        public override void OnDisAppearing()
        {
            TimerHelper.Instance.Dispose();
            base.OnDisAppearing();
        }

        public void DisplayErrorMessage(string message)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Application.Current.MainPage.DisplayAlert(LabelInfo, message, "Ok");
            });
        }

        /// <summary>
        /// This is used to get the baseURL.
        /// </summary>
        /// <returns></returns>
        public string GetAppURL()
        {
            var baseUrl = GlobalConstants.AppBaseURL;
            if (baseUrl.Length > 0 && !baseUrl.EndsWith('/'))
                baseUrl = baseUrl + '/';
            baseUrl = baseUrl + "External/" + GlobalConstants.VERSION + "/";
            return baseUrl;
        }

        public virtual void OnSyncCommand(bool isFromAuto = true)
        {

        }

        public virtual void RefreshData()
        {

        }
    }
}
