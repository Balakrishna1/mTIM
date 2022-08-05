using Acr.UserDialogs;
using mTIM.Helpers;
using mTIM.Interfaces;
using mTIM.Models;
using mTIM.UWP.mtimservices;
using mTIM.UWP.Services;
using mTIM.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using static mTIM.UWP.mtimservices.MobileTimClient;

[assembly: Xamarin.Forms.Dependency(typeof(WebSevice))]
namespace mTIM.UWP.Services
{
    class WebSevice : IWebService
    {
        public BaseViewModel ViewModel { get; set; }
        public Action<bool> ActionRefreshCallBack { get; set; }
        public Action<bool> GraphicsDownloadedCallBack { get; set; }
        public List<Task> DownloadList { get; set; }
        private MobileTimClient mobileTimClient { get; set; }


        private void Intialize()
        {
            if (mobileTimClient == null)
                mobileTimClient = new MobileTimClient();
        }

        public async Task ChangeFileComment(int taskId, int fileId, bool fileIdSpecified, string comment)
        {
        }

        public async Task DeleteFile(int taskId, int fileId, bool fileIdSpecified)
        {
        }

        private bool fromAutoSync;
        public async Task GetTasksListIDsFortheData(bool isFromAutoSync = false)
        {
            try
            {
                Intialize();
                fromAutoSync = isFromAutoSync;
                if (!fromAutoSync)
                    UserDialogs.Instance.ShowLoading("Loading list.", MaskType.Gradient);
                Debug.WriteLine(string.Format("Method Executed: GetTasksListIDsFortheData"));
                var taskListId = await mobileTimClient.GetTaskListIdForDayAsync(GlobalConstants.IMEINumber, GlobalConstants.VersionNumber, DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                if (taskListId > 0)
                {
                    if (Application.Current.Properties.ContainsKey("GetTaskListIdForDayResult"))
                    {
                        int tasksListID = (int)Application.Current.Properties["GetTaskListIdForDayResult"];
                        if (tasksListID == taskListId)
                        {
                            if (!fromAutoSync)
                                UserDialogs.Instance.HideLoading();
                            return;
                        }
                        else
                        {
                            if (fromAutoSync)
                            {
                                ActionRefreshCallBack?.Invoke(true);
                                return;
                            }
                        }
                    }
                    ActionRefreshCallBack?.Invoke(false);
                    FileHelper.DeleteAppDirectory();
                    FileInfoHelper.Instance.Clear();
                    Application.Current.Properties["GetTaskListIdForDayResult"] = taskListId;
                    await Application.Current.SavePropertiesAsync();
                    var timTasks = await mobileTimClient.GetTaskListAsync(GlobalConstants.IMEINumber, GlobalConstants.VersionNumber, taskListId);//To get the task list.
                    if (timTasks != null && timTasks.Any())
                    {
                        string json = JsonConvert.SerializeObject(timTasks);
                        Debug.WriteLine("Task Data:" + json);
                        //Passing the list json to BaseViewModel.
                        ViewModel.UpdateList(json);
                    }
                    else
                    {
                        ViewModel.DisplayErrorMessage("No tasks list for the day.");
                    }
                    var gzipBytes = await mobileTimClient.GetGraphicsBlobProtobufGZippedAsync(GlobalConstants.IMEINumber, GlobalConstants.VersionNumber, taskListId);//To get the 3D drawing.
                    if (gzipBytes != null && gzipBytes.Length >= 0)
                    {
                        if (!FileHelper.IsFileExists(GlobalConstants.GraphicsBlob_FILE))
                        {
                            await FileHelper.WriteAllBytesAsync(GlobalConstants.GraphicsBlob_FILE, gzipBytes);
                            GraphicsDownloadedCallBack?.Invoke(true);
                        }
                    }
                    var files = await mobileTimClient.GetFilesInformationAsync(GlobalConstants.IMEINumber, GlobalConstants.VersionNumber, taskListId);//To get the files.
                    if (files != null && files.Count > 0)
                    {
                        List<FileInfoModel> fileInfoModel = new List<FileInfoModel>();
                        foreach(var file in files)
                        {
                            fileInfoModel.Add(new FileInfoModel() { Key = file.Key, Values = file.Value.Select(v => new FileInfo(v.Comment, v.FileID)).ToList() });
                        }
                        var json = JsonConvert.SerializeObject(fileInfoModel);
                        await FileHelper.WriteTextAsync(GlobalConstants.FILES_INFO, json);
                        FileInfoHelper.Instance.SaveFileInfo(json);
                        Debug.WriteLine(String.Format("File information: {0}", json));
                        DownloadList = new List<Task>();
                        UserDialogs.Instance.ShowLoading("Downloading files.", MaskType.Gradient);
                        foreach (var item in files)
                        {
                            if (item.Value.Count > 0)
                            {
                                foreach (var value in item.Value)
                                {
                                    if (DownloadList.Count > 50)
                                        return;
                                    DownloadList.Add(Task.Run(() => DownloadFile(value.FileID, true)));
                                }
                            }
                        }
                    }
                    else
                    {
                        UserDialogs.Instance.HideLoading();
                    }
                }
                else
                {
                    ViewModel.DisplayErrorMessage("No tasks list for the day.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Error in GetTasksListIDsFortheData: {0}", ex?.Message)); 
                UserDialogs.Instance.HideLoading();
                ViewModel.DisplayErrorMessage(ex.Message);
            }
        }

        /// <summary>
        ///This method is used to download the file.
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="fileIdSpecified"></param>
        /// <returns></returns>
        public async Task DownloadFile(int fileId, bool fileIdSpecified)
        {
            try
            {
                MobileTimClient service = new MobileTimClient();
                var result = await service.GetFileAsync(GlobalConstants.IMEINumber, GlobalConstants.VersionNumber, fileId);
                FileCompletedEvent(result, fileId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Error in DownloadFile: {0}", ex?.Message));
                UserDialogs.Instance.HideLoading();
            }
        }

        /// <summary>
        /// This is used the save the file in local FileSystem after getting the information from service.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="fileId"></param>
        /// <returns></returns>
        private async Task FileCompletedEvent(TupleOfstringbase64Binary result, int fileId)
        {
            if (result.m_Item2.Length > 0)
            {
                string fileName = String.Format("{0}{1}", fileId, result.m_Item1);
                Debug.WriteLine("File saving: " + fileName);
                Debug.WriteLine("File saving with this extension: " + result.m_Item1);
                FileInfoHelper.Instance.AddExtesion(fileId, result.m_Item1);
                await FileHelper.WriteAllBytesAsync(fileName, result.m_Item2);
                if (DownloadList.Count == FileInfoHelper.Instance.GetExtensionsCount())
                {
                    var json = JsonConvert.SerializeObject(FileInfoHelper.Instance.GetExtensionList());
                    FileHelper.WriteTextAsync(GlobalConstants.FileExtesons, json);
                    UserDialogs.Instance.HideLoading();
                }
            }
        }
    

        public async Task OpenFile(int fileId, bool fileIdSpecified)
        {
        }

        public async Task QueryAppUpdate(Action<string> actionAppUpdate)
        {
        }

        public async Task SyncTaskList(string taskListJson, bool isFromAutoSync = false)
        {
        }

        public void UploadFile(bool taskListIdSpecified, int taskId, int posId, bool posIdSpecified, byte[] fileContent, string extension, string gps, string comment, DateTime time, bool timeSpecified, out int UploadFileResult, out bool UploadFileResultSpecified)
        {
            UploadFileResult = 0;
            UploadFileResultSpecified = true;
        }

        public async Task UploadFileAsync(bool taskListIdSpecified, int taskId, int posId, bool posIdSpecified, byte[] fileContent, string extension, string gps, string comment, DateTime time, bool timeSpecified)
        {
        }
    }
}
