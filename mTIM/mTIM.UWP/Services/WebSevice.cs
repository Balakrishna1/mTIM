using Acr.UserDialogs;
using HeyRed.Mime;
using mTIM.Helpers;
using mTIM.Interfaces;
using mTIM.Models;
using mTIM.UWP.mtimservice;
using mTIM.UWP.Services;
using mTIM.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Xamarin.Essentials;
using Xamarin.Forms;
using static mTIM.UWP.mtimservice.MobileTimClient;

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
                mobileTimClient = new MobileTimClient( EndpointConfiguration.BasicHttpBinding_IMobileTim, GlobalConstants.GetAppURL());
        }

        public async Task ChangeFileComment(int taskId, int fileId, bool fileIdSpecified, string comment)
        {
            Intialize();
            int tasksListID = (int)Application.Current.Properties["GetTaskListIdForDayResult"];
            var result = await mobileTimClient.ChangeFileCommentAsync(GlobalConstants.IMEINumber, GlobalConstants.VersionNumber, fileId, comment);
            if (result)
            {
                await FileInfoHelper.Instance.UpdateFileComment(taskId, fileId, comment);
                FileInfoHelper.Instance.CommentUpdatedCompleted?.Invoke(taskId, fileId);
            }
        }

        public async Task DeleteFile(int taskId, int fileId, bool fileIdSpecified)
        {
            Intialize();
            await mobileTimClient.DeleteFileAsync(GlobalConstants.IMEINumber, GlobalConstants.VersionNumber, fileId);
            await FileInfoHelper.Instance.DeleteFileInList(taskId, fileId);
            FileInfoHelper.Instance.DeleteCompleted?.Invoke(taskId, fileId);
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
                        foreach (var item in fileInfoModel)
                        {
                            if (item.Values.Count() > 0)
                            {
                                foreach (var value in item.Values)
                                {
                                    if (DownloadList.Count > 50)
                                        return;
                                    DownloadList.Add(Task.Run(() => DownloadFile(value.FileId, true)));
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
                MobileTimClient service = new MobileTimClient(EndpointConfiguration.BasicHttpBinding_IMobileTim, GlobalConstants.GetAppURL());
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
                string fileName = String.Format("{0}{1}", fileId, FileInfoHelper.Instance.GetExtension(fileId));
                var mime = MimeTypesMap.GetMimeType(fileName);
                Debug.WriteLine(mime);
            if (FileHelper.IsFileExists(fileName))
            {
                var filepath = FileHelper.GetFilePath(fileName);
                filepath = filepath.Replace('/', '\\');
                await Launcher.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(@filepath, mime)
                });
            }
            else
            {
                DownloadAndOpenFile(fileId, fileIdSpecified);
            }
        }
        /// <summary>
        /// This is used the download the project files data.
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="fileIdSpecified"></param>
        public async void DownloadAndOpenFile(int fileId, bool fileIdSpecified)
        {
            try
            {
                UserDialogs.Instance.ShowLoading("Downloading file.", MaskType.Gradient);
                var service = new MobileTimClient(EndpointConfiguration.BasicHttpBinding_IMobileTim, GlobalConstants.GetAppURL());
                var result = await service.GetFileAsync(GlobalConstants.IMEINumber, GlobalConstants.VersionNumber, fileId);
                if (result != null && result.m_Item2.Length > 0)
                {
                    string fileName = String.Format("{0}{1}", fileId, result.m_Item1);
                    Debug.WriteLine("File saving: " + fileName);
                    Debug.WriteLine("File saving with this extension: " + result.m_Item1);
                    FileInfoHelper.Instance.AddExtesion(fileId, result.m_Item1);
                    await FileHelper.WriteAllBytesAsync(fileName, result.m_Item2);
                    var json = JsonConvert.SerializeObject(FileInfoHelper.Instance.GetExtensionList());
                    FileHelper.WriteTextAsync(GlobalConstants.FileExtesons, json);
                    var mime = MimeTypesMap.GetMimeType(result.m_Item1);
                    Debug.WriteLine(mime);
                    var filepath = FileHelper.GetFilePath(fileName);
                    filepath = filepath.Replace('/', '\\');
                    await Launcher.OpenAsync(new OpenFileRequest
                    {
                        File = new ReadOnlyFile(filepath, mime)
                    });
                }
                UserDialogs.Instance.HideLoading();
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.HideLoading();
                Debug.WriteLine(string.Format("Error in DownloadAndOpenFile: {0}", ex?.Message));
            }
        }

        public async Task QueryAppUpdate(Action<string> actionAppUpdate)
        {
            //try
            //{
            //    Intialize();
            //    var result = mobileTimClient.QueryAppUpdateAsync(GlobalConstants.IMEINumber, GlobalConstants.VersionNumber);
            //    if (result != null)
            //    {
            //        string json = JsonConvert.SerializeObject(result);
            //        Debug.WriteLine("App Data: " + json);
            //        actionAppUpdate?.Invoke(json);
            //    }

            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine(string.Format("Error in QueryAppUpdate: {0}", ex?.Message));
            //}
        }

        public async Task SyncTaskList(string taskListJson, bool isFromAutoSync = false)
        {
            try
            {
                var list = JsonConvert.DeserializeObject<List<TaskResult>>(taskListJson);
                if (list != null && list.Count > 0)
                {
                    mobileTimClient.PostResponsesAsync(GlobalConstants.IMEINumber, GlobalConstants.VersionNumber, list);
                }
                GetTasksListIDsFortheData(isFromAutoSync);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Error in SyncTaskList: {0}", ex?.Message));
            }
        }

        public void UploadFile(bool taskListIdSpecified, int taskId, int posId, bool posIdSpecified, byte[] fileContent, string extension, string gps, string comment, DateTime time, bool timeSpecified, out int UploadFileResult, out bool UploadFileResultSpecified)
        {
            Intialize();
            int tasksListID = (int)Application.Current.Properties["GetTaskListIdForDayResult"];
            var result = mobileTimClient.UploadFileAsync(GlobalConstants.IMEINumber, GlobalConstants.VersionNumber, tasksListID, posId, fileContent, extension, gps, comment, time);
            UploadFileResult = result.Result;
            UploadFileResultSpecified = true;
        }

        public async Task UploadFileAsync(bool taskListIdSpecified, int taskId, int posId, bool posIdSpecified, byte[] fileContent, string extension, string gps, string comment, DateTime time, bool timeSpecified)
        {
            Intialize();
            int tasksListID = (int)Application.Current.Properties["GetTaskListIdForDayResult"];
           var result = await mobileTimClient.UploadFileAsync(GlobalConstants.IMEINumber, GlobalConstants.VersionNumber, tasksListID, posId, fileContent, extension, gps, comment, time);
            if (result > 0)
            {
                await FileInfoHelper.Instance.UpdateFileInfoInList(taskId, posId, result, true);
                FileInfoHelper.Instance.FileUploadCompleted?.Invoke(taskId, posId, result, true);
            }
        }
    }
}
