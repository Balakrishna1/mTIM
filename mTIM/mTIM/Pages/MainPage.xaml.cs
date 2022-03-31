using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using mTIM.Components;
using mTIM.Helpers;
using mTIM.Interfaces;
using mTIM.Models;
using mTIM.ViewModels;
using Newtonsoft.Json;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Urho;
using Xamarin.Forms;
using Location = Xamarin.Essentials;

namespace mTIM
{
    public partial class MainPage : ContentPage
    {
        protected UrhoApp App => glBuilding.App;
        protected WorldInputHandler inputs;
        MainViewModel ViewModel;
        public const int ListWidthInLandscape = 300;
        private double projectFontSize = 0;
        private double projectSubtextFontSize = 0;

        public MainPage()
        {
            InitializeComponent();
            ViewModel = new MainViewModel(Navigation);
            BindingContext = ViewModel;
            //BarcodeView.SetBindingViewModel(ViewModel);
            projectFontSize = lblTittle.FontSize;
            projectSubtextFontSize = lblSubtext.FontSize;
            var customCell = new DataTemplate(typeof(ElementViewCell));
            customCell.SetBinding(ElementViewCell.IdProperty, "Id");
            customCell.SetBinding(ElementViewCell.NameProperty, "Name");
            customCell.SetBinding(ElementViewCell.TypeProperty, "Type");
            customCell.SetBinding(ElementViewCell.ColorProperty, "Color");
            customCell.SetBinding(ElementViewCell.LevelProperty, "Level");
            customCell.SetBinding(ElementViewCell.ValueProperty, "Value");
            customCell.SetBinding(ElementViewCell.HasChaildsProperty, "HasChailds");
            ElementViewCell.ActionRightIconClicked -= RightIconClicked;
            ElementViewCell.ActionRightIconClicked += RightIconClicked;
            ElementViewCell.ActionValueClicked -= ValueClicked;
            ElementViewCell.ActionValueClicked += ValueClicked;
            ElementViewCell.ActionItemClicked -= ItemClicked;
            ElementViewCell.ActionItemClicked += ItemClicked;
            listView.SelectionMode = ListViewSelectionMode.Single;
            listView.ItemTemplate = customCell;
            listView.ItemsSource = ViewModel.SelectedItemList;
            ViewModel.SelectedItemList.CollectionChanged += SelectedItemList_CollectionChanged;
            ViewModel.LstValues.CollectionChanged += LstValues_CollectionChanged;
            frameHeader.SizeChanged -= FrameHeader_SizeChanged;
            frameHeader.SizeChanged += FrameHeader_SizeChanged;
            ViewModel.ActionSelectedItemText -= updateTextInGameWindow;
            ViewModel.ActionSelectedItemText += updateTextInGameWindow;

            Task.Run(async () =>
            {
                if (!await GetPermissions())
                {
                    ViewModel.Device.CloseApplication();
                }
                else
                {
                    updateInfo();
                }
            });
        }

        private void updateTextInGameWindow(string value)
        {
            App?.UpdateText(value);
        }

        private void ItemClicked(int id)
        {
            var item = ViewModel.SelectedItemList.Where(x => x.Id.Equals(id)).FirstOrDefault();
            if (item != null)
                ViewModel.SelectedItemIndex(ViewModel.SelectedItemList.IndexOf(item));
        }

        private void LstValues_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            lstValues.SelectedItem = null;
        }

        async void CommentClicked(object sender, System.EventArgs e)
        {
            object itemArgs = ((Button)sender).CommandParameter;
            var selectedItem = itemArgs as FileInfo;
            await TouchHelper.Instance.TouchEffectsWithCommand((Button)sender, 0.9, 100, ViewModel.OnCommentClickedCommand, selectedItem);
        }

        async void OnDeleteClicked(object sender, EventArgs e)
        {
            object itemArgs = ((ImageButton)sender).CommandParameter;
            var selectedItem = itemArgs as FileInfo;
            await TouchHelper.Instance.TouchEffectsWithCommand((ImageButton)sender, 0.9, 100, ViewModel.OnDeleteClickedCommand, selectedItem);
        }

        async void OnEyeClicked(object sender, EventArgs e)
        {
            object itemArgs = ((ImageButton)sender).CommandParameter;
            var selectedItem = itemArgs as FileInfo;
            await TouchHelper.Instance.TouchEffectsWithCommand((ImageButton)sender, 0.9, 100, ViewModel.OnViewClickedCommand, selectedItem);
        }

        void OnValueTapped(object sender, EventArgs e)
        {
            TappedEventArgs itemArgs = e as TappedEventArgs;
            var selectedItem = itemArgs.Parameter as Value;
            ViewModel.SelectedValue(selectedItem);
        }

        private void RightIconClicked(int id)
        {
            var item = ViewModel.SelectedItemList.Where(x => x.Id.Equals(id)).FirstOrDefault();
            if (item != null)
                ViewModel.SelectedItemIndex(ViewModel.SelectedItemList.IndexOf(item));
        }

        private void ValueClicked(int id)
        {
            var item = ViewModel.SelectedItemList.Where(x => x.Id.Equals(id)).FirstOrDefault();
            if (item != null)
                ViewModel.SelectedValueItem(item);
        }

        private void updateInfo()
        {
            ScaleTo1x(imgBadge);
#if DEBUG
            GlobalConstants.AppBaseURL = "http://mtimtest.precast-software.com:7778";
#else
            GlobalConstants.AppBaseURL = string.Empty;
#endif
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
                    ViewModel.MessageModel = messageModel;
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
                    ViewModel.SelectedLanguage = messageModel.Language;
                    ViewModel.UpdateLanguage(ViewModel.SelectedLanguage);
                    GlobalConstants.AppBaseURL = messageModel.BaseUrl;
                    GlobalConstants.StatusSyncTime = ViewModel.SyncTime = messageModel.StatusSyncTime;
                    GlobalConstants.SyncMinutes = ViewModel.SyncMinites = messageModel.StatusSyncMinutes;
                }
            }

            getLocation();
            ViewModel.UpdateList();
        }


        public async void SaveAppMessage()
        {
            AndroidMessageModel androidMessage = new AndroidMessageModel();
            androidMessage.Brand = Location.DeviceInfo.Manufacturer;
            androidMessage.Device = Location.DeviceInfo.Model;
            androidMessage.DeviceId = GlobalConstants.DeviceID;
            androidMessage.IMEI = GlobalConstants.IMEINumber;
            androidMessage.PseudoID = GlobalConstants.UniqueID;
            androidMessage.VersionCode = GlobalConstants.VersionCode;
            androidMessage.VersionName = GlobalConstants.VersionNumber;
            string content = JsonConvert.SerializeObject(androidMessage);
            Debug.WriteLine("mTIM Device JSON:" + content);
            await FileHelper.WriteTextAsync(GlobalConstants.IMEI_FILE, content);
        }

        private void SelectedItemList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            listView.SelectedItem = null;
        }

        private double width = 0;
        private double height = 0;
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height); //must be called
            if (this.width != width || this.height != height)
            {
                this.width = width;
                this.height = height;
                //reconfigure layout
            }
            //BarcodeView.Init(height, width);
            //BarcodeView.IsVisible = true;
            ViewModel.IsScanning = false;
            ViewModel.IsOpenBarcodeView = false;
            if (!CustomBottomSheet.IsInitiated)
            {
                CustomBottomSheet.Init(height, width);
            }
            CustomBottomSheet.InvokeView(height, width);
            if(!AppUpdateBottomSheet.IsInitiated)
            {
                AppUpdateBottomSheet.Init(height, width);
            }
            AppUpdateBottomSheet.InvokeView(height, width);
            if (height > width)
            {
                //Xamarin.Forms.PlatformConfiguration.iOSSpecific.Page.SetUseSafeArea(On<Xamarin.Forms.PlatformConfiguration.iOS>(), true);
                GlobalConstants.IsLandscape = false;
                stackHeader.Orientation = StackOrientation.Vertical;
                stackHeader.HorizontalOptions = LayoutOptions.EndAndExpand;
                stackHeader.FlowDirection = FlowDirection.LeftToRight;
                stackMenuOptions.FlowDirection = FlowDirection.LeftToRight;
                stackList.Orientation = StackOrientation.Vertical;
                listView.WidthRequest = lstValues.WidthRequest = lstDocuments.WidthRequest = stackStringType.WidthRequest = width;
                listView.HeightRequest = lstValues.HeightRequest = lstDocuments.HeightRequest = stackStringType.HeightRequest = height - frameHeader.Height;
                glBuilding.IsVisible = false;
                isLoaded = false;
            }
            else
            {
                //Xamarin.Forms.PlatformConfiguration.iOSSpecific.Page.SetUseSafeArea(On<Xamarin.Forms.PlatformConfiguration.iOS>(), true);
                GlobalConstants.IsLandscape = true;
                glBuilding.IsVisible = true;
                stackHeader.Orientation = StackOrientation.Horizontal;
                stackHeader.HorizontalOptions = LayoutOptions.EndAndExpand;
                stackHeader.FlowDirection = FlowDirection.RightToLeft;
                stackMenuOptions.FlowDirection = FlowDirection.LeftToRight;
                stackList.Orientation = StackOrientation.Horizontal;
                listView.WidthRequest = lstValues.WidthRequest = lstDocuments.WidthRequest = stackStringType.WidthRequest = ListWidthInLandscape;
                listView.HeightRequest = lstValues.HeightRequest = lstDocuments.HeightRequest = stackStringType.HeightRequest = height - frameHeader.Height;
                glBuilding.WidthRequest = width - ListWidthInLandscape;
                glBuilding.HeightRequest = height - frameHeader.Height;
                loadUrhoView();
            }
        }


        private void FrameHeader_SizeChanged(object sender, EventArgs e)
        {
            listView.HeightRequest = lstValues.HeightRequest = lstDocuments.HeightRequest = stackStringType.HeightRequest = height - frameHeader.Height;
            if (GlobalConstants.IsLandscape)
            {
                glBuilding.HeightRequest = height - frameHeader.Height;
            }
        }

        protected override void OnAppearing()
        {
            ViewModel.OnAppearing();
            //glBuilding?.StopUrhoApp();
            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            if (cts != null && !cts.IsCancellationRequested)
                cts.Cancel();
            ViewModel.OnDisAppearing();
            base.OnDisappearing();
        }

        private async Task loadTexturedMesh()
        {
            await InitializeUrho();

            var model = App.AddChild<ObjectModel>("model");

            //await model.LoadMesh(ViewModel.ImageSource);

            //var model = App.AddChild<ObjectModel>("model");
            var path = string.Format(GlobalConstants.GraphicsBlob_FILE, 198);
            await model.LoadTexturedMesh(path, "mTIM.Meshes.Model2.png", false);
        }

        private async Task InitializeUrho()
        {
            try
            {
                // have to wait for the loading task to complete before adding components
                await glBuilding.LoadingUrhoTask.Task;

                App.RootNode.RemoveAllChildren();
                App.RootNode.SetWorldRotation(Quaternion.Identity);
                App.RootNode.Position = Urho.Vector3.Zero;
                App.Camera.Zoom = 1f;

                inputs = App.AddChild<WorldInputHandler>("inputs");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

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
                Permission.Storage
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
            }
            return permissionsGranted;
        }

        private async void getLocation()
        {
            try
            {
                var location = await Location.Geolocation.GetLastKnownLocationAsync();

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
            catch (Location.FeatureNotSupportedException fnsEx)
            {
                // Handle not supported on device exception
            }
            catch (Location.FeatureNotEnabledException fneEx)
            {
                // Handle not enabled on device exception
            }
            catch (Location.PermissionException pEx)
            {
                // Handle permission exception
            }
            catch (Exception ex)
            {
                // Unable to get location
            }
        }

        CancellationTokenSource cts;
        async Task GetCurrentLocation()
        {
            try
            {
                var request = new Location.GeolocationRequest(Location.GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                cts = new CancellationTokenSource();
                var location = await Location.Geolocation.GetLocationAsync(request, cts.Token);

                if (location != null)
                {
                    fillLocationInfo(location);
                    Debug.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
                }
            }
            catch (Location.FeatureNotSupportedException fnsEx)
            {
                // Handle not supported on device exception
            }
            catch (Location.FeatureNotEnabledException fneEx)
            {
                // Handle not enabled on device exception
            }
            catch (Location.PermissionException pEx)
            {
                // Handle permission exception
            }
            catch (Exception ex)
            {
                // Unable to get location
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
        /// This method is used to update the app version.
        /// </summary>
        private void getVersionInfo()
        {
            GlobalConstants.VersionNumber = Location.VersionTracking.CurrentVersion;
            GlobalConstants.VersionCode = Location.VersionTracking.CurrentBuild;
        }

        private void TapGestureRecognizer(object sender, EventArgs e)
        {
            ViewModel.IsOpenMenuOptions = false;
            ViewModel.IsOpenUpdateOptions = false;
            ViewModel.IsOpenBarcodeView = false;
        }

        public bool isLoaded = false;
        private void loadUrhoView()
        {
            if (!isLoaded)
            {
                //Device.StartTimer(TimeSpan.FromMilliseconds(300), () =>
                //{
                    glBuilding.StartUrhoApp();
                    isLoaded = true;
                //    return true;
                //});
            }
        }

        public void ScaleTo1x(Image image)
        {
            var a = new Xamarin.Forms.Animation();
            a.Add(0, 0.5, new Xamarin.Forms.Animation(v => image.Scale = v, 0.7, 1.2, Easing.CubicInOut));
            a.Add(0.5, 1, new Xamarin.Forms.Animation(v => image.Scale = v, 1.2, 0.7, Easing.CubicIn));
            a.Commit(this, "animation", length: 1000,
                finished: (v, c) => image.Scale = 0.7, repeat: () => true);
        }

        void Label_SizeChanged(System.Object sender, System.EventArgs e)
        {
            var lbl = sender as Label;
            if (lbl != null && lbl.Text.Length > 20)
            {
                lblTittle.FontSize = projectFontSize - 4;
            }
            else
            {
                lblTittle.FontSize = projectFontSize;
            }
        }

        void LabelSubText_SizeChanged(System.Object sender, System.EventArgs e)
        {
            var lbl = sender as Label;
            if (lbl != null && lbl.Text.Length > 60)
            {
                lblSubtext.FontSize = projectSubtextFontSize - 2;
            }
            else
            {
                lblSubtext.FontSize = projectSubtextFontSize;
            }
        }

        void btnCamera_Clicked(System.Object sender, System.EventArgs e)
        {
           ViewModel.CapturePhotoAsync(null);
        }

        void btnGalary_Clicked(System.Object sender, System.EventArgs e)
        {
            ViewModel.PickPhotoAsync(null);
        }
    }
}
