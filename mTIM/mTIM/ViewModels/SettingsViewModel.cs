using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using mTIM.Helpers;
using mTIM.Models;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace mTIM.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private string entryAppURL;
        public string EntryAppURL
        {
            get => entryAppURL;
            set
            {
                if(GlobalConstants.AppBaseURL != value)
                {
                    GlobalConstants.AppBaseURL = value.Trim();
                    SaveSettings();
                }
                SetAndRaisePropertyChanged(ref entryAppURL, value);
            }
        }

        private int statusSyncTime;
        public int StatusSyncTime
        {
            get => statusSyncTime;
            set
            {
                SetAndRaisePropertyChanged(ref statusSyncTime, value);
            }
        }

        public ObservableCollection<Language> Languages;
        public SettingsViewModel(INavigation navigation)
        {
            this.Navigation = navigation;
            Languages = new ObservableCollection<Language>
            {
                new Language() { DisplayName= "english", ShortName="en" },
                new Language() { DisplayName= "deutsch", ShortName="de" },
                new Language() { DisplayName= "中文", ShortName="zh-Hant" },
                new Language() { DisplayName= "русский", ShortName="ru" }
            };
            EntryAppURL = GlobalConstants.AppBaseURL;
            StatusSyncTime = GlobalConstants.StatusSyncTime;
        }

        public override void OnAppearing()
        {
            TimerHelper.Instance.StartTimer();
            base.OnAppearing();
        }

        public ICommand SelectedLanguageItemCommand => new Command(SelectedLanguageItemExecute);

        private void SelectedLanguageItemExecute(object parameter)
        {
            SelectedLanguage = parameter.ToString();
            UpdateLanguage(parameter.ToString());
            SaveSettings();
        }

        public ICommand IncrementORDecrementCommand => new Command(SlectedButtonClcik);

        private void SlectedButtonClcik(object parameter)
        {
            switch (parameter.ToString())
            {
                case "in":
                    SyncMinites++;
                    SyncTime = SyncMinites * 60;
                    break;
                case "dc":
                    SyncMinites--;
                    SyncTime = SyncMinites * 60;
                    break;
            }
            SaveSettings();
        }

        public async Task SaveSettings()
        {
            if (FileHelper.IsFileExists(GlobalConstants.SETTINGS_FILE))
            {
                FileHelper.DeleteDirectory(GlobalConstants.SETTINGS_FILE);
            }
            SettingsModel settingsModel = new SettingsModel()
            {
                BaseUrl = GlobalConstants.AppBaseURL,
                Language = SelectedLanguage,
                StatusSyncTime = SyncTime,
                StatusSyncMinutes = SyncMinites
            };

            string content = JsonConvert.SerializeObject(settingsModel);
            Debug.WriteLine("mTIM Settings JSON:" + content);
            await FileHelper.WriteTextAsync(GlobalConstants.SETTINGS_FILE, content);
        }

        public override async Task OnSyncCommand(bool isFromAuto = true)
        {
            base.OnSyncCommand(isFromAuto);
        }
    }
}
