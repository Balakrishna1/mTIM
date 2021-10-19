using System;
using System.Collections.Generic;
using System.Diagnostics;
using Acr.UserDialogs;
using mTIM.Droid.mtimtest.precast_software.com;
using mTIM.Droid.Services;
using mTIM.Helpers;
using mTIM.Interfaces;
using mTIM.Models;
using mTIM.ViewModels;
using Newtonsoft.Json;
using PCLStorage;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(WebSevice))]
namespace mTIM.Droid.Services
{
    public class WebSevice : IWebService
    {
        MobileTimService timService = new MobileTimService();
        public BaseViewModel ViewModel { get; set; }

        private bool fromAutoSync;
        public void GetTasksListIDsFortheData(bool isFromAutoSync = false)
        {
            fromAutoSync = isFromAutoSync;
            if(!fromAutoSync)
            UserDialogs.Instance.ShowLoading(string.Empty, MaskType.Gradient);
            Console.WriteLine(string.Format("Method Executed: GetTasksListIDsFortheData"));
            timService.GetTaskListIdForDayCompleted -= TimService_GetTaskListIdForDayCompleted;
            timService.GetTaskListIdForDayCompleted += TimService_GetTaskListIdForDayCompleted;
            timService.GetTaskListIdForDayAsync(GlobalConstants.IMEINumber, GlobalConstants.VersionNumber, DateTime.Now.Year, true, DateTime.Now.Month, true, DateTime.Now.Day, true);
        }

        private async void TimService_GetTaskListIdForDayCompleted(object sender, GetTaskListIdForDayCompletedEventArgs e)
        {
            if (e.Error == null && !e.Cancelled)
            {
                if (!fromAutoSync)
                    UserDialogs.Instance.ShowLoading(string.Empty, MaskType.Gradient);
                if (Application.Current.Properties.ContainsKey("GetTasksListIDsFortheData"))
                {
                    int tasksListID = (int)Application.Current.Properties["GetTasksListIDsFortheData"];
                    if(tasksListID == e.GetTaskListIdForDayResult)
                    {
                        if (!fromAutoSync)
                            UserDialogs.Instance.HideLoading();
                        return;
                    }
                }
                Application.Current.Properties["GetTasksListIDsFortheData"] = e.GetTaskListIdForDayResult;
                await Application.Current.SavePropertiesAsync();
                //timService.GetTaskListAsCsvCompleted -= TimService_GetTaskListAsCsvCompleted;
                //timService.GetTaskListAsCsvCompleted += TimService_GetTaskListAsCsvCompleted;
                timService.GetTaskListCompleted -= TimService_GetTaskListCompleted;
                timService.GetTaskListCompleted += TimService_GetTaskListCompleted;
                timService.GetTaskListAsync(GlobalConstants.IMEINumber, GlobalConstants.VersionNumber, e.GetTaskListIdForDayResult, e.GetTaskListIdForDayResultSpecified);
                timService.GetGraphicsBlobProtobufGZippedCompleted -= TimService_GetGraphicsBlobProtobufGZippedCompleted;
                timService.GetGraphicsBlobProtobufGZippedCompleted += TimService_GetGraphicsBlobProtobufGZippedCompleted;
                timService.GetGraphicsBlobProtobufGZippedAsync(GlobalConstants.IMEINumber, GlobalConstants.VersionNumber, e.GetTaskListIdForDayResult, e.GetTaskListIdForDayResultSpecified);
            }
            else if (e.Error != null)
            {
                if (!fromAutoSync)
                {
                    UserDialogs.Instance.HideLoading();
                    ViewModel.DisplayErrorMessage(e.Error.Message);
                }
            }
        }

        private async void TimService_GetGraphicsBlobProtobufGZippedCompleted(object sender, GetGraphicsBlobProtobufGZippedCompletedEventArgs e)
        {
            UserDialogs.Instance.HideLoading();
            if (e.Error == null && !e.Cancelled)
            {
                var path = string.Format(GlobalConstants.GraphicsBlob_FILE, 198);
                if (!FileHelper.IsFileExists(path))
                {
                    if(e.Result != null && e.Result.Length >= 0)
                    {
                        await FileHelper.WriteAllBytesAsync(path, e.Result);
                    }
                }
                ViewModel.ImageSource = e.Result;
                    //await FileHelper.ReadAllBytesAsync(path);
                Console.WriteLine(e.Result);
            }
            else if (e.Error != null && !fromAutoSync)
            {
                ViewModel.DisplayErrorMessage(e.Error.Message);
            }
        }

        private void TimService_GetTaskListAsCsvCompleted(object sender, GetTaskListAsCsvCompletedEventArgs e)
        {
            UserDialogs.Instance.HideLoading();
            if (e.Error == null && !e.Cancelled)
            {
                ViewModel.UpdateList(e.Result);
            }
            else if (e.Error != null && !fromAutoSync)
            {
                ViewModel.DisplayErrorMessage(e.Error.Message);
            }
        }

        private void TimService_GetTaskListCompleted(object sender, GetTaskListCompletedEventArgs e)
        {
            UserDialogs.Instance.HideLoading();
            if (e.Error == null && !e.Cancelled)
            {
                string json = JsonConvert.SerializeObject(e.Result);
                Debug.WriteLine("Task Data:" + json);
                ViewModel.UpdateList(json);
            }
            else if (e.Error != null && !fromAutoSync)
            {
                ViewModel.DisplayErrorMessage(e.Error.Message);
            }
        }
    }
}
