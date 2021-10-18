using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using mTIM.Helpers;
using mTIM.Interfaces;
using mTIM.Models;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace mTIM.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private IWebService webservice { get; set; }

        private string entryAppURL;
        public string EntryAppURL
        {
            get => entryAppURL;
            set
            {
                if(GlobalConstants.AppBaseURL != entryAppURL)
                {
                    GlobalConstants.AppBaseURL = entryAppURL;
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
                    IsDecrementIocnVisible = true;
                    if (SyncMinites == 10)
                    {
                        IsIncrementIocnVisible = false;
                    }
                    break;
                case "dc":
                    SyncMinites--;
                    IsIncrementIocnVisible = true;
                    if (SyncMinites == 1)
                    {
                        IsDecrementIocnVisible = false;
                    }
                    break;
            }
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
                StatusSyncTime = SyncTime
            };

            string content = JsonConvert.SerializeObject(settingsModel);
            Console.WriteLine("mTIM Settings JSON:" + content);
            await FileHelper.WriteTextAsync(GlobalConstants.SETTINGS_FILE, content);
        }
    }
}
