using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Acr.UserDialogs;
using HeyRed.Mime;
using mTIM.Droid.mtimtest.precast_software.com;
using mTIM.Droid.Services;
using mTIM.Helpers;
using mTIM.Interfaces;
using mTIM.ViewModels;
using Newtonsoft.Json;
using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: Dependency(typeof(WebSevice))]
namespace mTIM.Droid.Services
{
    public class WebSevice : IWebService
    {

        MobileTimService timService = GlobalConstants.GetAppURL() == string.Empty ? new MobileTimService() : new MobileTimService(GlobalConstants.GetAppURL());
        public BaseViewModel ViewModel { get; set; }
        public Action<bool> ActionRefreshCallBack { get; set; }

        private bool fromAutoSync;

        /// <summary>
        /// This method is used the Tasks list for the day.
        /// </summary>
        /// <param name="isFromAutoSync"></param>
        public void GetTasksListIDsFortheData(bool isFromAutoSync = false)
        {
            try
            {
                timService = new MobileTimService(GlobalConstants.GetAppURL());
                fromAutoSync = isFromAutoSync;
                if (!fromAutoSync)
                    UserDialogs.Instance.ShowLoading("Loading list.", MaskType.Gradient);
                Debug.WriteLine(string.Format("Method Executed: GetTasksListIDsFortheData"));
                timService.GetTaskListIdForDayCompleted -= TimService_GetTaskListIdForDayCompleted;
                timService.GetTaskListIdForDayCompleted += TimService_GetTaskListIdForDayCompleted;
                timService.GetTaskListIdForDayAsync(GlobalConstants.IMEINumber, GlobalConstants.VersionNumber, DateTime.Now.Year, true, DateTime.Now.Month, true, DateTime.Now.Day, true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Error in GetTasksListIDsFortheData: {0}", ex?.Message));
            }
        }

        private async void TimService_GetTaskListIdForDayCompleted(object sender, GetTaskListIdForDayCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null && !e.Cancelled)
                {
                    if (Application.Current.Properties.ContainsKey("GetTaskListIdForDayResult"))
                    {
                        int tasksListID = (int)Application.Current.Properties["GetTaskListIdForDayResult"];
                        if (tasksListID == e.GetTaskListIdForDayResult)
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
                    Application.Current.Properties["GetTaskListIdForDayResult"] = e.GetTaskListIdForDayResult;
                    await Application.Current.SavePropertiesAsync();
                    timService.GetTaskListCompleted -= TimService_GetTaskListCompleted;
                    timService.GetTaskListCompleted += TimService_GetTaskListCompleted;
                    timService.GetTaskListAsync(GlobalConstants.IMEINumber, GlobalConstants.VersionNumber, e.GetTaskListIdForDayResult, e.GetTaskListIdForDayResultSpecified);//To get the task list.
                    timService.GetGraphicsBlobProtobufGZippedCompleted -= TimService_GetGraphicsBlobProtobufGZippedCompleted;
                    timService.GetGraphicsBlobProtobufGZippedCompleted += TimService_GetGraphicsBlobProtobufGZippedCompleted;
                    timService.GetGraphicsBlobProtobufGZippedAsync(GlobalConstants.IMEINumber, GlobalConstants.VersionNumber, e.GetTaskListIdForDayResult, e.GetTaskListIdForDayResultSpecified);//To get the 3D drawing.
                    timService.GetFilesInformationCompleted -= TimService_GetFilesInformationCompleted;
                    timService.GetFilesInformationCompleted += TimService_GetFilesInformationCompleted;
                    timService.GetFilesInformationAsync(GlobalConstants.IMEINumber, GlobalConstants.VersionNumber, e.GetTaskListIdForDayResult, e.GetTaskListIdForDayResultSpecified);//To get the files.
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
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Error in TimService_GetTaskListIdForDayCompleted: {0}", ex?.Message));
            }
        }

        private List<Task> DownloadList;
        private async void TimService_GetFilesInformationCompleted(object sender, GetFilesInformationCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null && !e.Cancelled)
                {
                    if (e.Result != null && e.Result.Length > 0)
                    {
                        var json = JsonConvert.SerializeObject(e.Result);
                        await FileHelper.WriteTextAsync(GlobalConstants.FILES_INFO, json);
                        FileInfoHelper.Instance.SaveFileInfo(json);
                        Debug.WriteLine(String.Format("File information: {0}", json));
                        DownloadList = new List<Task>();
                        UserDialogs.Instance.ShowLoading("Downloading files.", MaskType.Gradient);
                        foreach (var item in e.Result)
                        {
                            if (item != null && item.Value.Length > 0)
                            {
                                foreach (var value in item.Value)
                                {
                                    DownloadList.Add(Task.Run(() => DownloadFile(Math.Abs(value.FileID), value.FileIDSpecified)));
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
                    UserDialogs.Instance.HideLoading();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Error in TimService_GetFilesInformationCompleted: {0}", ex?.Message));
            }
        }

        /// <summary>
        /// This method is used to get the 3D drawing file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void TimService_GetGraphicsBlobProtobufGZippedCompleted(object sender, GetGraphicsBlobProtobufGZippedCompletedEventArgs e)
        {
            try
            {
                //UserDialogs.Instance.HideLoading();
                if (e.Error == null && !e.Cancelled)
                {
                    var path = string.Format(GlobalConstants.GraphicsBlob_FILE, 198);
                    if (!FileHelper.IsFileExists(path))
                    {
                        if (e.Result != null && e.Result.Length >= 0)
                        {
                            await FileHelper.WriteAllBytesAsync(path, e.Result);
                        }
                    }
                    ViewModel.ImageSource = e.Result;
                    //await FileHelper.ReadAllBytesAsync(path);
                    Debug.WriteLine(e.Result);
                }
                else if (e.Error != null && !fromAutoSync)
                {
                    ViewModel.DisplayErrorMessage(e.Error.Message);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Error in TimService_GetGraphicsBlobProtobufGZippedCompleted: {0}", ex?.Message));
            }
        }

        private void TimService_GetTaskListCompleted(object sender, GetTaskListCompletedEventArgs e)
        {
            //UserDialogs.Instance.HideLoading();
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

        /// <summary>
        /// This is used to open the pdf file.
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="fileIdSpecified"></param>
        public async void OpenPdfFile(int fileId, bool fileIdSpecified)
        {
            string fileName = String.Format("{0}{1}", fileId, FileInfoHelper.Instance.GetExtension(fileId));
            var mime = MimeTypesMap.GetMimeType(fileName);
            Debug.WriteLine(mime);
            if (FileHelper.IsFileExists(fileName))
            {
                UserDialogs.Instance.ShowLoading(string.Empty, MaskType.Gradient);
                await Launcher.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(FileHelper.GetFilePath(fileName), mime)
                });
                UserDialogs.Instance.HideLoading();
            }
            else
            {
                DownloadAndOpenFile(fileId, fileIdSpecified);
            }
        }

        public async void DownloadAndOpenFile(int fileId, bool fileIdSpecified)
        {
            try
            {
                UserDialogs.Instance.ShowLoading("Downloading file.", MaskType.Gradient);
                var service = new MobileTimService(GlobalConstants.GetAppURL());
                service.GetFileCompleted += async delegate (object sender, GetFileCompletedEventArgs e)
                {
                    if (e.Error == null && !e.Cancelled)
                    {
                        if (e.Result.m_Item2.Length > 0)
                        {
                            string fileName = String.Format("{0}{1}", fileId, e.Result.m_Item1);
                            Debug.WriteLine("File saving: " + fileName);
                            Debug.WriteLine("File saving with this extension: " + e.Result.m_Item1);
                            FileInfoHelper.Instance.AddExtesion(fileId, e.Result.m_Item1);
                            await FileHelper.WriteAllBytesAsync(fileName, e.Result.m_Item2);
                            var json = JsonConvert.SerializeObject(FileInfoHelper.Instance.GetExtensionList());
                            FileHelper.WriteTextAsync(GlobalConstants.FileExtesons, json);
                            var mime = MimeTypesMap.GetMimeType(fileName);
                            Debug.WriteLine(mime);
                            await Launcher.OpenAsync(new OpenFileRequest
                            {
                                File = new ReadOnlyFile(FileHelper.GetFilePath(fileName), mime)
                            });
                            UserDialogs.Instance.HideLoading();
                        }
                    }
                };
                service.GetFileAsync(GlobalConstants.IMEINumber, GlobalConstants.VersionNumber, fileId, fileIdSpecified);

            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Error in DownloadAndOpenFile: {0}", ex?.Message));
            }
        }

        /// <summary>
        ///This method is used to download the file.
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="fileIdSpecified"></param>
        /// <returns></returns>
        public void DownloadFile(int fileId, bool fileIdSpecified)
        {
            try
            {
                MobileTimService service = new MobileTimService(GlobalConstants.GetAppURL());
                FileCompletedEvent(service, fileId);
                service.GetFileAsync(GlobalConstants.IMEINumber, GlobalConstants.VersionNumber, fileId, fileIdSpecified);

            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Error in DownloadFile: {0}", ex?.Message));
            }
        }



        private async Task FileCompletedEvent(MobileTimService service, int fileId)
        {
            service.GetFileCompleted += async delegate (object sender, GetFileCompletedEventArgs e)
            {
                if (e.Error == null && !e.Cancelled)
                {
                    if (e.Result.m_Item2.Length > 0)
                    {
                        string fileName = String.Format("{0}{1}", fileId, e.Result.m_Item1);
                        Debug.WriteLine("File saving: " + fileName);
                        Debug.WriteLine("File saving with this extension: " + e.Result.m_Item1);
                        FileInfoHelper.Instance.AddExtesion(fileId, e.Result.m_Item1);
                        await FileHelper.WriteAllBytesAsync(fileName, e.Result.m_Item2);
                        if (DownloadList.Count == FileInfoHelper.Instance.GetExtensionsCount())
                        {
                            var json = JsonConvert.SerializeObject(FileInfoHelper.Instance.GetExtensionList());
                            FileHelper.WriteTextAsync(GlobalConstants.FileExtesons, json);
                            UserDialogs.Instance.HideLoading();
                        }
                    }
                }
                else
                {
                    UserDialogs.Instance.HideLoading();
                }
            };
        }

        public void SyncTaskList(string taskListJson, bool isFromAutoSync = false)
        {
            try
            {
                var list = JsonConvert.DeserializeObject<TaskResult[]>(taskListJson);
                if (list != null && list.Length > 0)
                {
                    timService.PostResponsesAsync(GlobalConstants.IMEINumber, GlobalConstants.VersionNumber, list);
                }
                GetTasksListIDsFortheData(isFromAutoSync);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Error in SyncTaskList: {0}", ex?.Message));
            }
        }

        Action<string> ActionAppUpdate;
        public void QueryAppUpdate(Action<string> actionAppUpdate)
        {
            try
            {
                timService = new MobileTimService(GlobalConstants.GetAppURL());
                ActionAppUpdate = actionAppUpdate;
                timService.QueryAppUpdateCompleted -= TimService_QueryAppUpdateCompleted;
                timService.QueryAppUpdateCompleted += TimService_QueryAppUpdateCompleted;
                timService.QueryAppUpdateAsync(GlobalConstants.IMEINumber, GlobalConstants.VersionNumber);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Error in QueryAppUpdate: {0}", ex?.Message));
            }
        }

        private void TimService_QueryAppUpdateCompleted(object sender, QueryAppUpdateCompletedEventArgs e)
        {
            if (e.Error == null && !e.Cancelled)
            {
                string json = JsonConvert.SerializeObject(e.Result);
                Debug.WriteLine("App Data: " + json);
                ActionAppUpdate?.Invoke(json);
            }
        }
    }
}
