using System;
using System.Collections.Generic;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Xamarin.Forms;

namespace mTIM
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            Xamarin.Essentials.VersionTracking.Track();
            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
            //AppCenter.Start("android={5fab3b86-06fe-4102-8795-96e7e0bac92a};" +
            //      "uwp={2cf2de56-04b5-43df-aba1-ef6c29c0cc80};" +
            //      "ios={c901a770-ef47-47bc-959a-6dc97eb09ad8};",
            //      typeof(Analytics), typeof(Crashes));
            //intializeAppcenter();
        }

        private void intializeAppcenter()
        {
            AppCenter.SetCountryCode("en");
            AppCenter.Configure("android={5fab3b86-06fe-4102-8795-96e7e0bac92a};" +
                  "uwp={2cf2de56-04b5-43df-aba1-ef6c29c0cc80};" +
                  "ios={c901a770-ef47-47bc-959a-6dc97eb09ad8};");
            if (AppCenter.Configured)
            {
                AppCenter.Start(typeof(Analytics));
                AppCenter.Start(typeof(Crashes));
                Analytics.SetEnabledAsync(true);
                Analytics.StartSession();
            }
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
