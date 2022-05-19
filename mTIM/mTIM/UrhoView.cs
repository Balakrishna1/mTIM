using System;
using System.Threading.Tasks;
using Urho;
using Urho.Forms;
using Xamarin.Forms;

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

        public UrhoView()
        {
            LoadingUrhoTask = new TaskCompletionSource<bool>();
            _urhoSurface = new UrhoSurface();

            Content = new Grid
            {
                Children =
                {
                    _urhoSurface
                }
            };
        }

        protected override async void OnParentSet()
        {
            base.OnParentSet();

            if (this.Parent == null)
            {
                await StopUrhoApp();
            }
            else
            {
                //await StartUrhoApp();
            }
        }

        /// <summary>
        /// To Intialize the game window. 
        /// </summary>
        /// <returns></returns>
        public async Task StartUrhoApp()
        {
            if (_urhoSurface == null)
                throw new System.Exception("Urho Surface not defined");

            if (_urhoApp == null)
            {
                //This will fail if called twice within an application
                _urhoApp = await _urhoSurface.Show<UrhoApp>(
                    new ApplicationOptions(assetsFolder: "Data")
                    {
                        Orientation = ApplicationOptions.OrientationType.LandscapeAndPortrait,
                        TouchEmulation = true,
                    });
                LoadingUrhoTask.SetResult(true);
            }
            else if(_urhoApp.IsInitialized)
            {
                _urhoApp.AddStuff();
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
    }
}
