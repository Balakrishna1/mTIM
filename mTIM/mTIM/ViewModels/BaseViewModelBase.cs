using mTIM.Helpers;
using mTIM.Resources;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;

namespace mTIM.ViewModels
{
    public class BaseViewModelBase
    {
        private string labelSettings = AppResources.Settings;

        private string labelInfo = AppResources.Info;
        private string labelRefresh = AppResources.Refresh;
        private string labelUrl = AppResources.Url;
        private string labelSyncTime = AppResources.SyncTime;
        private string labelHintURL = AppResources.HintURL;
        private string labelAppUpdates = AppResources.AppUpdates;

        private string selectedLanguage = "";
        public string LabelSettings
        {
            get => labelSettings;
            set => SetAndRaisePropertyChanged(ref labelSettings, value);
        }
        public string LabelInfo
        {
            get => labelInfo;
            set => SetAndRaisePropertyChanged(ref labelInfo, value);
        }
        public string LabelRefresh
        {
            get => labelRefresh;
            set => SetAndRaisePropertyChanged(ref labelRefresh, value);
        }
        public string LabelURL
        {
            get => labelUrl;
            set => SetAndRaisePropertyChanged(ref labelUrl, value);
        }
        public string LabelSyncTime
        {
            get => labelSyncTime;
            set => SetAndRaisePropertyChanged(ref labelSyncTime, value);
        }

        public string LabelHintURL
        {
            get => labelHintURL;
            set => SetAndRaisePropertyChanged(ref labelHintURL, value);
        }
        public string LabelAppUpdates
        {
            get => labelAppUpdates;
            set => SetAndRaisePropertyChanged(ref labelAppUpdates, value);
        }
        public string SelectedLanguage
        {
            get => selectedLanguage;
            set => SetAndRaisePropertyChanged(ref selectedLanguage, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnAppearing()
        {
            UpdateLanguage(string.IsNullOrEmpty(GlobalConstants.SelectedLanguage) ? Thread.CurrentThread.CurrentUICulture?.Name : GlobalConstants.SelectedLanguage);
        }
        public virtual void OnDisAppearing()
        {

        }

        protected void SetAndRaisePropertyChanged<TRef>(
    ref TRef field, TRef value, [CallerMemberName] string propertyName = null)
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void SetAndRaisePropertyChangedIfDifferentValues<TRef>(
            ref TRef field, TRef value, [CallerMemberName] string propertyName = null)
            where TRef : class
        {
            if (field == null || !field.Equals(value))
            {
                SetAndRaisePropertyChanged(ref field, value, propertyName);
            }
        }

        private Color enLangugaeBackground;
        public Color ENLangugaeBackground
        {
            get => enLangugaeBackground;
            set => SetAndRaisePropertyChanged(ref enLangugaeBackground, value);
        }

        private Color duLangugaeBackground;
        public Color DULangugaeBackground
        {
            get => duLangugaeBackground;
            set => SetAndRaisePropertyChanged(ref duLangugaeBackground, value);
        }

        private Color zhLangugaeBackground;
        public Color ZHLangugaeBackground
        {
            get => zhLangugaeBackground;
            set => SetAndRaisePropertyChanged(ref zhLangugaeBackground, value);
        }

        private Color ruLangugaeBackground;
        public Color RULangugaeBackground
        {
            get => ruLangugaeBackground;
            set => SetAndRaisePropertyChanged(ref ruLangugaeBackground, value);
        }

        private Color appUpdateTextColor = Color.Black;
        public Color AppUpdateTextColor
        {
            get => appUpdateTextColor;
            set => SetAndRaisePropertyChanged(ref appUpdateTextColor, value);
        }

        public void UpdateLanguage(string sortName)
        {
            SelectedLanguage = sortName;
            GlobalConstants.SelectedLanguage = sortName;
            var culture = new CultureInfo(sortName);
            AppResources.Culture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            updateLanguageBackground();
            //CrossMultilingual.Current.CurrentCultureInfo = culture;
            LabelSettings = AppResources.Settings;
            LabelAppUpdates = AppResources.AppUpdates;
            LabelHintURL = AppResources.HintURL;
            LabelInfo = AppResources.Info;
            LabelRefresh = AppResources.Refresh;
            LabelSyncTime = AppResources.SyncTime;
            LabelURL = AppResources.Url;
        }

        private void updateLanguageBackground()
        {
            ENLangugaeBackground = Color.White;
            DULangugaeBackground = Color.White;
            ZHLangugaeBackground = Color.White;
            RULangugaeBackground = Color.White;
            switch (SelectedLanguage)
            {
                case "en":
                case "en_US":
                    ENLangugaeBackground = Color.LightGray;
                    break;
                case "de":
                    DULangugaeBackground = Color.LightGray;
                    break;
                case "zh-Hant":
                    ZHLangugaeBackground = Color.LightGray;
                    break;
                case "ru":
                    RULangugaeBackground = Color.LightGray;
                    break;
                default:
                    ENLangugaeBackground = Color.LightGray;
                    break;
            }
        }
    }
}