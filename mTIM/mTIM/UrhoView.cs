using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using mTIM.Models.D;
using mTIM.Helpers;
using Urho;
using Urho.Forms;
using Xamarin.Essentials;
using Xamarin.Forms;
using mTIM.Managers;

namespace mTIM
{
    public class UrhoView : ContentView
    {
        public UrhoApp App => _urhoApp;
        public TaskCompletionSource<bool> LoadingUrhoTask;

        public Action<int, double, double> MouseWheelPressed;
        public Action<double, double> RightMouseClickMoved;

        private UrhoSurface _urhoSurface;
        private UrhoApp _urhoApp;
        private readonly string tag = "UrhoView";

        public UrhoView()
        {
            LoadingUrhoTask = new TaskCompletionSource<bool>();
            this.BackgroundColor = Xamarin.Forms.Color.LightCoral;
            _urhoSurface = new UrhoSurface()
            {
                BackgroundColor = Xamarin.Forms.Color.LightCoral,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand
            };
            _urhoSurface.HorizontalOptions = LayoutOptions.FillAndExpand;
            _urhoSurface.VerticalOptions = LayoutOptions.FillAndExpand;
            Content = new Grid
            {
                Children =
                {
                    _urhoSurface
                }
            };
            Content.HorizontalOptions = LayoutOptions.FillAndExpand;
            Content.VerticalOptions = LayoutOptions.FillAndExpand;
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            //Debug.WriteLine($"width: {width} height: {height}");
            if (!Device.RuntimePlatform.Equals(Device.iOS) && DeviceDisplay.MainDisplayInfo.Orientation != DisplayOrientation.Landscape) return;
            _urhoSurface.LayoutTo(new Rectangle(new Point(0, 0), new Size(width, height)));
        }

        /// <summary>
        /// To Intialize the game window. 
        /// </summary>
        /// <returns></returns>
        public async Task StartUrhoApp()
        {
            try
            {
                AnalyticsManager.TrackEvent(System.Reflection.MethodBase.GetCurrentMethod().Name);
                if (_urhoSurface == null)
                    throw new System.Exception("Urho Surface not defined");

                if (_urhoApp == null)
                {
                    //This will fail if called twice within an application
                    _urhoApp = await _urhoSurface.Show<UrhoApp>(
                        new ApplicationOptions(assetsFolder: "Data")
                        {
                            Orientation = ApplicationOptions.OrientationType.LandscapeAndPortrait,
                            TouchEmulation = true
                        });
                    LoadingUrhoTask.SetResult(true);
                }
                else if (_urhoApp.IsInitialized)
                {
                    _urhoApp.AddStuff();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                CrashReportManager.ReportError(ex, System.Reflection.MethodBase.GetCurrentMethod().Name, tag);
            }
        }

        /// <summary>
        /// To stop the game window.
        /// </summary>
        /// <returns></returns>
        public async Task StopUrhoApp()
        {
            if (_urhoApp != null)
            {
                AnalyticsManager.TrackEvent(System.Reflection.MethodBase.GetCurrentMethod().Name);
                await _urhoApp.Exit();
                _urhoApp = null;
                Content = null;
                _urhoSurface = null;
            }
        }

        /// <summary>
        /// To reset the window.
        /// </summary>
        /// <returns></returns>
        public async Task ResetUrhoApp()
        {
            if (_urhoApp != null)
            {
                await _urhoApp.Exit();
                _urhoApp = null;
            }
        }

        /// <summary>
        /// This is used to update the 3D drawing.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="mesh"></param>
        internal void Update3dDrawing(int id, TimMesh mesh)
        {
            AnalyticsManager.TrackEvent(System.Reflection.MethodBase.GetCurrentMethod().Name);
            Urho.Application.InvokeOnMain(() =>
            {
                if (App != null && mesh != null)
                {
                    App?.UpdateCameraPosition();
                    if (Helpers.TimTaskListHelper.IsParent(id))
                    {
                        if (!mesh.IsLoaded)
                        {
                            App.Reset();
                            App.AddStuff();
                        }
                        App.LoadLinesDrawing(mesh);
                        App.LoadEelementsDrawing(mesh, true, isParent: true);
                        App.HideButtonsWindow();
                    }
                    else
                    {
                        App.LoadLinesDrawing(mesh);
                        App.LoadEelementsDrawing(mesh, false);
                        TimElementMesh elementsMesh = mesh.elementMeshes.Where(x => x.listId == id).FirstOrDefault();
                        if (!elementsMesh.Equals(default(TimElementMesh)) && elementsMesh.triangleBatch.numVertices > 0)
                        {
                            App.ShowButtonsWindow();
                            App.LoadActiveDrawing(mesh, elementsMesh.triangleBatch.startIndex, elementsMesh.triangleBatch.primitiveCount);
                        }
                        else
                        {
                            App.LoadEelementsDrawing(mesh, true, 1);
                            App.HideButtonsWindow();
                        }
                    }
                }
            });
        }

        internal void UpdateElements(string id)
        {
            AnalyticsManager.TrackEvent(System.Reflection.MethodBase.GetCurrentMethod().Name);
            App?.UpdateCameraPosition();
            App?.UpdateSelectedElement(id);
        }
    }
}
