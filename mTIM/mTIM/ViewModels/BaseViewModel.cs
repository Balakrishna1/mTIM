using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
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
                updateTimer();
                if (value == 10)
                {
                    IsIncrementIocnVisible = false;
                }
                else if (value == 1)
                {
                    IsDecrementIocnVisible = false;
                }
                SetAndRaisePropertyChanged(ref syncMinites, value);
            }
        }

        private void updateTimer()
        {
            StringSyncTime = string.Format(syncTimeFormat, SyncTime);
            TimerHelper.Instance.Dispose();
            TimerHelper.Instance.Create(CallBack);
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
                    item.HasChailds = true;
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
        }
        
        public override void OnDisAppearing()
        {
            //TimerHelper.Instance.Dispose();
            base.OnDisAppearing();
        }

        public void DisplayErrorMessage(string message)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Application.Current.MainPage.DisplayAlert(LabelInfo, message, "Ok");
            });
        }

        public virtual void OnSyncCommand(bool isFromAuto = true)
        {
            SaveTaskList();
            Webservice.ViewModel = this;
            Webservice.GetTasksListIDsFortheData(isFromAuto);
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
