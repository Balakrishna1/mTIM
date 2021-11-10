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
        public MainPage()
        {
            InitializeComponent();
            ViewModel = new MainViewModel(Navigation);
            BindingContext = ViewModel;
            BarcodeView.IsVisible = false;
            BarcodeView.SetBindingViewModel(ViewModel);
            var customCell = new DataTemplate(typeof(ElementViewCell));
            customCell.SetBinding(ElementViewCell.IdProperty, "Id");
            customCell.SetBinding(ElementViewCell.NameProperty, "Name");
            customCell.SetBinding(ElementViewCell.TypeProperty, "Type");
            customCell.SetBinding(ElementViewCell.ColorProperty, "Color");
            customCell.SetBinding(ElementViewCell.LevelProperty, "Level");
            customCell.SetBinding(ElementViewCell.ValueProperty, "Value");
            customCell.SetBinding(ElementViewCell.HasChaildsProperty, "HasChailds");
            ElementViewCell.ActionArrowClicked -= ArrowClicked;
            ElementViewCell.ActionArrowClicked += ArrowClicked;
            ElementViewCell.ActionValueClicked -= ValueClicked;
            ElementViewCell.ActionValueClicked += ValueClicked;
            listView.SelectionMode = ListViewSelectionMode.Single;
            listView.ItemTemplate = customCell;
            listView.ItemsSource = ViewModel.SelectedItemList;
            ViewModel.SelectedItemList.CollectionChanged += SelectedItemList_CollectionChanged;
            ViewModel.LstValues.CollectionChanged += LstValues_CollectionChanged;
            frameHeader.SizeChanged -= FrameHeader_SizeChanged;
            frameHeader.SizeChanged += FrameHeader_SizeChanged;

            Task.Run(async () =>
            {
                if (!await GetPermissions())
                {
                    //TODO:Need to write application kill process
                }
                else
                {
                    updateInfo();
                }
            });
            listView.ItemSelected -= ListView_ItemSelected;
            listView.ItemSelected += ListView_ItemSelected;
            lstValues.ItemSelected -= LstValues_ItemSelected;
            lstValues.ItemSelected += LstValues_ItemSelected;
            lstDocuments.ItemSelected -= LstDocuments_ItemSelected;
            lstDocuments.ItemSelected += LstDocuments_ItemSelected;
        }

        private void LstValues_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            lstValues.SelectedItem = null;
        }

        private void LstDocuments_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItemIndex < 0)
            {
                return;
            }
            lstDocuments.SelectedItem = null;
            ViewModel.SelectedDocument(e.SelectedItemIndex);
        }


        private void LstValues_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItemIndex < 0)
            {
                return;
            }
            ViewModel.SelectedValueIndex(e.SelectedItemIndex);
        }

        private void ArrowClicked(int id)
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
            if (!FileHelper.IsFileExists(GlobalConstants.IMEI_FILE))
            {
                getVersionInfo();
                getDeviceInfo();
                SaveAppMessage();
            }else
            {
                string jsonIMEI = FileHelper.ReadText(GlobalConstants.IMEI_FILE);
                Debug.WriteLine("mTIM Device JSON:" + jsonIMEI);
                AndroidMessageModel messageModel = JsonConvert.DeserializeObject<AndroidMessageModel>(jsonIMEI);
                if (messageModel != null)
                {
                    ViewModel.MessageModel = messageModel;
                    GlobalConstants.DeviceID = messageModel.DeviceId;
                    GlobalConstants.IMEINumber = messageModel.IMEI;
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
#if DEBUG
            GlobalConstants.AppBaseURL = "http://mtimtest.precast-software.com:7778";
#else
            GlobalConstants.AppURL = string.Empty;
#endif
            ViewModel.UpdateList();
        }


        public async void SaveAppMessage()
        {
            AndroidMessageModel androidMessage = new AndroidMessageModel();
            androidMessage.Brand = Location.DeviceInfo.Manufacturer;
            androidMessage.Device = Location.DeviceInfo.Model;
            androidMessage.DeviceId = GlobalConstants.DeviceID;
            androidMessage.IMEI = GlobalConstants.IMEINumber;
            androidMessage.PseudoID = "";
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

        private void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (!GlobalConstants.IsLandscape)
            {
                if (e.SelectedItemIndex < 0)
                {
                    return;
                }
                ViewModel.SelectedItemIndex(e.SelectedItemIndex);
            }
            loadTexturedMesh();
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
            if (!BarcodeView.IsInitiated)
            {
                BarcodeView.Init(height, width);
            }
            if (!CustomBottomSheet.IsInitiated)
            {
                CustomBottomSheet.Init(height, width);
            }
            CustomBottomSheet.InvokeView(height, width);
            if (height > width)
            {
                ViewModel.IsOpenBarcodeView = false;
                GlobalConstants.IsLandscape = false;
                stackHeader.Orientation = StackOrientation.Vertical;
                stackHeader.HorizontalOptions = LayoutOptions.EndAndExpand;
                stackHeader.FlowDirection = FlowDirection.LeftToRight;
                stackMenuOptions.FlowDirection= FlowDirection.LeftToRight;
                stackList.Orientation = StackOrientation.Vertical;
                listView.WidthRequest = lstValues.WidthRequest = lstDocuments.WidthRequest = stackStringType.WidthRequest = width;
                listView.HeightRequest = lstValues.HeightRequest = lstDocuments.HeightRequest = stackStringType.HeightRequest = height - frameHeader.Height;
                glBuilding.IsVisible = false;
            }
            else
            {
                ViewModel.IsOpenBarcodeView = false;
                GlobalConstants.IsLandscape = true;
                glBuilding.IsVisible = true;
                stackHeader.Orientation = StackOrientation.Horizontal;
                stackHeader.HorizontalOptions = LayoutOptions.EndAndExpand;
                stackHeader.FlowDirection = FlowDirection.RightToLeft;
                stackMenuOptions.FlowDirection = FlowDirection.LeftToRight;
                stackList.Orientation = StackOrientation.Horizontal;
                listView.WidthRequest = lstValues.WidthRequest = lstDocuments.WidthRequest = stackStringType.WidthRequest = 250;
                listView.HeightRequest = lstValues.HeightRequest = lstDocuments.HeightRequest = stackStringType.HeightRequest = height - frameHeader.Height;
                glBuilding.WidthRequest = width - 250;
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

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
        }



        protected override void OnAppearing()
        {
            ViewModel.OnAppearing();
            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
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
                App.RootNode.Position = Vector3.Zero;
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

            foreach (var permission in permissionsStartList)
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
                        status = await CrossPermissions.Current.RequestPermissionAsync<LocationWhenInUsePermission>();
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
                }else
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
            ViewModel.IsOpenBarcodeView = false;
        }

        public bool isLoaded = false;
        private void loadUrhoView()
        {
            if (!isLoaded)
            {
                Device.StartTimer(TimeSpan.FromMilliseconds(300), () =>
                {
                    glBuilding.StartUrhoApp();
                    isLoaded = true;
                    return false;
                });
            }
        }
    }
}
