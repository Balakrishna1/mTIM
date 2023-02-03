using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using mTIM.Helpers;
using mTIM.Interfaces;
using mTIM.Managers;
using mTIM.Models;
using mTIM.Models.D;
using mTIM.ViewModels;
using mTIM.Views;
using Newtonsoft.Json;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using Location = Xamarin.Essentials;

namespace mTIM
{
    public partial class MainPage : ContentPage
    {
        protected UrhoApp App => glBuilding.App;
        MainViewModel ViewModel;
        public const double ListWidthInLandscape = 300;
        private double projectFontSize = 0;
        private double projectSubtextFontSize = 0;
        private TimMesh CurrentMesh = new TimMesh();

        public MainPage()
        {
            InitializeComponent();
            ViewModel = new MainViewModel(Navigation);
            BindingContext = ViewModel;
            //BarcodeView.SetBindingViewModel(ViewModel);
            projectFontSize = lblTittle.FontSize;
            projectSubtextFontSize = lblSubtext.FontSize;

            TimBaseViewCell.ActionRightIconClicked -= RightIconClicked;
            TimBaseViewCell.ActionRightIconClicked += RightIconClicked;
            TimBaseViewCell.ActionValueClicked -= ValueClicked;
            TimBaseViewCell.ActionValueClicked += ValueClicked;
            TimBaseViewCell.ActionItemClicked -= ItemClicked;
            TimBaseViewCell.ActionItemClicked += ItemClicked;
            ViewModel.SelectedItemList.CollectionChanged += SelectedItemList_CollectionChanged;
            ViewModel.LstValues.CollectionChanged += LstValues_CollectionChanged;
            frameHeader.SizeChanged -= FrameHeader_SizeChanged;
            frameHeader.SizeChanged += FrameHeader_SizeChanged;
            ViewModel.ActionSelectedItemText -= updateTextInGameWindow;
            ViewModel.ActionSelectedItemText += updateTextInGameWindow;
            ViewModel.UpdateDrawing -= UpdateDrawing;
            ViewModel.UpdateDrawing += UpdateDrawing;
            ViewModel.UpdateListSelection -= UpdateListSelection;
            ViewModel.UpdateListSelection += UpdateListSelection;
            ScaleTo1x(imgBadge);
            ViewModel.Intialize();
        }

        private void UpdateDrawing(int id)
        {
            if (!GlobalConstants.IsLandscape)
            {
                return;
            }
            Device.BeginInvokeOnMainThread(() =>
            {
                listView.ScrollTo(ViewModel.SelectedItemList?.Where(x => x.Id == id).FirstOrDefault(), ScrollToPosition.Center, true);
            });
            glBuilding.Update3dDrawing(id, CurrentMesh);
        }

        /// <summary>
        /// Update the selected item in list.
        /// </summary>
        /// <param name="id"></param>
        private void UpdateListSelection(int id)
        {
            if (!GlobalConstants.IsLandscape)
            {
                return;
            }
            Device.BeginInvokeOnMainThread(() =>
            {
                listView.ScrollTo(ViewModel.SelectedItemList?.Where(x => x.Id == id).FirstOrDefault(), ScrollToPosition.Center, true);
            });
        }

        /// <summary>
        /// Update the text in model window.
        /// </summary>
        /// <param name="value"></param>
        private void updateTextInGameWindow(string value)
        {
            App?.UpdateText(value);
        }

        /// <summary>
        /// Item tapped in the portrait mode.
        /// </summary>
        /// <param name="id"></param>
        private void ItemClicked(int id)
        {
            try
            {
                AnalyticsManager.TrackEvent(System.Reflection.MethodBase.GetCurrentMethod().Name,  AnalyticsType.ClickorSelected);
                var item = ViewModel.SelectedItemList.Where(x => x.Id.Equals(id)).FirstOrDefault();
                if (GlobalConstants.IsLandscape)
                {
                    if (item.HasChilds)
                    {
                        if (glBuilding != null && glBuilding.App.IsElementAvailable(id) && !TimTaskListHelper.IsParent(id))
                        {
                            glBuilding.UpdateElements(id.ToString());
                        }
                        else
                        {
                            var mesh = ViewModel.Meshes.Where(x => x.ProjectId == item.ProjectId).FirstOrDefault();
                            if (CurrentMesh.ProjectId != mesh.ProjectId)
                            {
                                CurrentMesh.IsLoaded = false;
                            }
                            CurrentMesh = mesh;
                            glBuilding.Update3dDrawing(id, CurrentMesh);
                        }
                        ViewModel.UpdateIndexSelection(id);
                    }
                    ViewModel?.ActionSelectedItemText?.Invoke(item.Name);
                }
                else
                {
                    if (item != null)
                        ViewModel.SelectedItemIndex(ViewModel.SelectedItemList.IndexOf(item));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                CrashReportManager.ReportError(ex, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Values collection changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LstValues_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            lstValues.SelectedItem = null;
        }

        /// <summary>
        /// Comment clicked in the document cell.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void CommentClicked(object sender, System.EventArgs e)
        {
            AnalyticsManager.TrackEvent(System.Reflection.MethodBase.GetCurrentMethod().Name, AnalyticsType.ClickorSelected);
            object itemArgs = ((Button)sender).CommandParameter;
            var selectedItem = itemArgs as FileInfo;
            await TouchHelper.Instance.TouchEffectsWithCommand((Button)sender, 0.9, 100, ViewModel.OnCommentClickedCommand, selectedItem);
        }

        /// <summary>
        /// Delete clicked in the document cell.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void OnDeleteClicked(object sender, EventArgs e)
        {
            AnalyticsManager.TrackEvent(System.Reflection.MethodBase.GetCurrentMethod().Name, AnalyticsType.ClickorSelected);
            object itemArgs = ((ImageButton)sender).CommandParameter;
            var selectedItem = itemArgs as FileInfo;
            await TouchHelper.Instance.TouchEffectsWithCommand((ImageButton)sender, 0.9, 100, ViewModel.OnDeleteClickedCommand, selectedItem);
        }

        /// <summary>
        /// Eye icon clicked in the picture cell.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void OnEyeClicked(object sender, EventArgs e)
        {
            AnalyticsManager.TrackEvent(System.Reflection.MethodBase.GetCurrentMethod().Name, AnalyticsType.ClickorSelected);
            object itemArgs = ((ImageButton)sender).CommandParameter;
            var selectedItem = itemArgs as FileInfo;
            await TouchHelper.Instance.TouchEffectsWithCommand((ImageButton)sender, 0.9, 100, ViewModel.OnViewClickedCommand, selectedItem);
        }

        /// <summary>
        /// Value item tapped in the cell.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnValueTapped(object sender, EventArgs e)
        {
            AnalyticsManager.TrackEvent(System.Reflection.MethodBase.GetCurrentMethod().Name, AnalyticsType.ClickorSelected);
            TappedEventArgs itemArgs = e as TappedEventArgs;
            var selectedItem = itemArgs.Parameter as Value;
            ViewModel.SelectedValue(selectedItem);
        }

        /// <summary>
        /// Right icon clicked in the cell.
        /// </summary>
        /// <param name="id"></param>
        private void RightIconClicked(int id)
        {
            AnalyticsManager.TrackEvent(System.Reflection.MethodBase.GetCurrentMethod().Name, AnalyticsType.ClickorSelected);
            var item = ViewModel.SelectedItemList.Where(x => x.Id.Equals(id)).FirstOrDefault();
            if (item != null)
                ViewModel.SelectedItemIndex(ViewModel.SelectedItemList.IndexOf(item));
        }

        /// <summary>
        /// Value clicked in the cell
        /// </summary>
        /// <param name="id"></param>
        private void ValueClicked(int id)
        {
            AnalyticsManager.TrackEvent(System.Reflection.MethodBase.GetCurrentMethod().Name, AnalyticsType.ClickorSelected);
            var item = ViewModel.SelectedItemList.Where(x => x.Id.Equals(id)).FirstOrDefault();
            if (item != null)
                ViewModel.SelectedValueItem(item);
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
            AnalyticsManager.TrackEvent(System.Reflection.MethodBase.GetCurrentMethod().Name, AnalyticsType.ClickorSelected);
            if (this.width != width || this.height != height)
            {
                this.width = width;
                this.height = height;
                //reconfigure layout
            }
            ViewModel.IsScanning = false;
            ViewModel.IsOpenBarcodeView = false;
            if (!CustomBottomSheet.IsInitiated)
            {
                CustomBottomSheet.Init(height, width);
            }
            CustomBottomSheet.InvokeView(height, width);
            if (!AppUpdateBottomSheet.IsInitiated)
            {
                AppUpdateBottomSheet.Init(height, width);
            }
            AppUpdateBottomSheet.InvokeView(height, width);
            if (height > width)
            {
                TimTaskListHelper.GetTotalList()?.ToList()?.ForEach(x => x.IsSelected = false);
                //Xamarin.Forms.PlatformConfiguration.iOSSpecific.Page.SetUseSafeArea(On<Xamarin.Forms.PlatformConfiguration.iOS>(), true);
                GlobalConstants.IsLandscape = false;
                stackHeader.Orientation = StackOrientation.Vertical;
                stackHeader.HorizontalOptions = LayoutOptions.EndAndExpand;
                stackHeader.FlowDirection = FlowDirection.LeftToRight;
                stackMenuOptions.FlowDirection = FlowDirection.LeftToRight;
                stackList.Orientation = StackOrientation.Vertical;
                listView.WidthRequest = lstValues.WidthRequest = lstDocuments.WidthRequest = stackStringType.WidthRequest = width;
                listView.HeightRequest = lstValues.HeightRequest = lstDocuments.HeightRequest = stackStringType.HeightRequest = height - frameHeader.Height;
                stopUrhoView();
                glBuilding.IsVisible = false;
                isLoaded = false;
            }
            else
            {
                //Xamarin.Forms.PlatformConfiguration.iOSSpecific.Page.SetUseSafeArea(On<Xamarin.Forms.PlatformConfiguration.iOS>(), true);
                //ListWidthInLandscape = (width / 4) * 1.5;
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
                //glBuilding.BackgroundColor = Color.White;
                loadUrhoView();
            }

#if __iOS__
            var safeInsets = On<iOS>().SafeAreaInsets();
            safeInsets.Right = 0;
            safeInsets.Left = GlobalConstants.IsLandscape ? 40 : 0;
            safeInsets.Top = GlobalConstants.IsLandscape ? 0 : 40;
            safeInsets.Bottom = 0;
            Padding = safeInsets;
#endif
        }

        /// <summary>
        /// Update the header text size.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
            base.OnAppearing();

            if (GlobalConstants.IsLandscape && Device.RuntimePlatform.Equals(Device.iOS))
            {
                loadUrhoView();
            }
        }

        protected override void OnDisappearing()
        {
            ViewModel.OnDisAppearing();
            base.OnDisappearing();
        }

        private void TapGestureRecognizer(object sender, EventArgs e)
        {
            ViewModel.IsOpenMenuOptions = false;
            ViewModel.IsOpenUpdateOptions = false;
            ViewModel.IsOpenBarcodeView = false;
        }

        /// <summary>
        /// Load the urho sharp app.
        /// </summary>
        public bool isLoaded = false;
        private async Task loadUrhoView()
        {
            if (!isLoaded)
            {
                await glBuilding?.StartUrhoApp();
                isLoaded = true;
            }
        }

        /// <summary>
        /// Stop the urho sharp app.
        /// </summary>
        /// <returns></returns>
        private async Task stopUrhoView()
        {
            if (isLoaded)
            {
                await glBuilding.ResetUrhoApp();
                isLoaded = false;
            }
        }

        /// <summary>
        /// Refresh icon animation
        /// </summary>
        /// <param name="image"></param>
        public void ScaleTo1x(Image image)
        {
            var a = new Xamarin.Forms.Animation();
            a.Add(0, 0.5, new Xamarin.Forms.Animation(v => image.Scale = v, 0.7, 1.2, Easing.CubicInOut));
            a.Add(0.5, 1, new Xamarin.Forms.Animation(v => image.Scale = v, 1.2, 0.7, Easing.CubicIn));
            a.Commit(this, "animation", length: 1000,
                finished: (v, c) => image.Scale = 0.7, repeat: () => true);
        }

        /// <summary>
        /// To update the text size.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// To update the sub text size.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Camera icon click in the document cell.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnCamera_Clicked(System.Object sender, System.EventArgs e)
        {
            AnalyticsManager.TrackEvent(System.Reflection.MethodBase.GetCurrentMethod().Name, AnalyticsType.ClickorSelected);
            ViewModel.CapturePhotoAsync();
        }

        /// <summary>
        /// Galary icon click in the document cell.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnGalary_Clicked(System.Object sender, System.EventArgs e)
        {
            AnalyticsManager.TrackEvent(System.Reflection.MethodBase.GetCurrentMethod().Name, AnalyticsType.ClickorSelected);
            ViewModel.PickPhotoAsync();
        }
    }
}
