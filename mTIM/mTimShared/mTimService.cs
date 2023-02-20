using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Acr.UserDialogs;
using mTIM.Helpers;
using mTIM.ViewModels;
using Newtonsoft.Json;
using Xamarin.Essentials;
using Xamarin.Forms;
using HeyRed.Mime;
using mTIM;
using mTIM.Managers;
#if __ANDROID__
using mTIM.Droid.mtimtest.precast_software.com;
#elif __IOS__
using mTIM.iOS.mtimtest.precast_software.com;
#endif

namespace mTimShared
{
    public class mTimService
    {
        MobileTimService timService { get; set; }
        public BaseViewModel ViewModel { get; set; }
        public Action<bool> ActionRefreshCallBack { get; set; }
        public Action<bool> GraphicsDownloadedCallBack { get; set; }
        private readonly string tag = "mTimService";

        private static mTimService instance;

        public static mTimService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new mTimService();
                }
                return instance;
            }
        }

        private bool fromAutoSync;

        /// <summary>
        /// This method is used the Tasks list for the day.
        /// </summary>
        /// <param name="isFromAutoSync"></param>
        public void GetTasksListIDsFortheData(bool isFromAutoSync = false)
        {
            try
            {
                AnalyticsManager.TrackEvent(System.Reflection.MethodBase.GetCurrentMethod().Name);
                timService = new MobileTimService(GlobalConstants.GetAppURL());
                fromAutoSync = isFromAutoSync;
                if (!fromAutoSync)
                    UserDialogs.Instance.ShowLoading("Loading list.", MaskType.Gradient);
                Debug.WriteLine(string.Format("Method Executed: GetTasksListIDsFortheData"));
                AnalyticsManager.TrackEvent(System.Reflection.MethodBase.GetCurrentMethod().Name);
                timService.GetTaskListIdForDayCompleted -= TimService_GetTaskListIdForDayCompleted;
                timService.GetTaskListIdForDayCompleted += TimService_GetTaskListIdForDayCompleted;
                timService.GetTaskListIdForDayAsync(GlobalConstants.IMEINumber, GlobalConstants.VersionNumber, DateTime.Now.Year, true, DateTime.Now.Month, true, DateTime.Now.Day, true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Error in GetTasksListIDsFortheData: {0}", ex?.Message));
                CrashReportManager.ReportError(ex, System.Reflection.MethodBase.GetCurrentMethod().Name, tag);
            }
        }

        /// <summary>
        /// GetTaskListIdForDayCompleted callback.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                    timService.GetStatusCompleted -= TimService_GetStatusCompleted;
                    timService.GetStatusCompleted += TimService_GetStatusCompleted;
                    timService.GetStatusAsync(GlobalConstants.IMEINumber, GlobalConstants.VersionNumber, e.GetTaskListIdForDayResult, e.GetTaskListIdForDayResultSpecified, Guid.Empty.ToString(), Guid.Empty.ToString());
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
                CrashReportManager.ReportError(ex, System.Reflection.MethodBase.GetCurrentMethod().Name, tag);
            }
        }

        private async void TimService_GetStatusCompleted(object sender, GetStatusCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null && !e.Cancelled)
                {
                    if (e.Result != null)
                    {
                        var json = JsonConvert.SerializeObject(e.Result);
                        await FileHelper.WriteTextAsync(GlobalConstants.EelementStatus_FILE, json);
                        TimTaskListHelper.UpdateStateInfo(json);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Error in TimService_GetStatusCompleted: {0}", ex?.Message));
                CrashReportManager.ReportError(ex, System.Reflection.MethodBase.GetCurrentMethod().Name, tag);
            }
        }

        /// <summary>
        /// GetFilesInformationCompleted callback. This will fire after getting the files information of the current project from service.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                                    if (DownloadList.Count > 50)
                                        return;
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
                CrashReportManager.ReportError(ex, System.Reflection.MethodBase.GetCurrentMethod().Name, tag);
            }
        }

        /// <summary>
        /// This is the GetGraphicsBlobProtobufGZippedCompleted callback of 3D drawing.
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
                    if (e.Result != null && e.Result.Length >= 0)
                    {
                        if (!FileHelper.IsFileExists(GlobalConstants.GraphicsBlob_FILE))
                        {
                            await FileHelper.WriteAllBytesAsync(GlobalConstants.GraphicsBlob_FILE, e.Result);
                            GraphicsDownloadedCallBack?.Invoke(true);
                        }
                        Debug.WriteLine(e.Result);
                    }
                    else if (e.Error != null && !fromAutoSync)
                    {
                        ViewModel.DisplayErrorMessage(e.Error.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Error in TimService_GetGraphicsBlobProtobufGZippedCompleted: {0}", ex?.Message));
                CrashReportManager.ReportError(ex, System.Reflection.MethodBase.GetCurrentMethod().Name, tag);
            }
        }

        /// <summary>
        /// GetTaskListCompleted Callback. This will fire after getting the list from service.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimService_GetTaskListCompleted(object sender, GetTaskListCompletedEventArgs e)
        {
            //UserDialogs.Instance.HideLoading();
            if (e.Error == null && !e.Cancelled)
            {
                string json = JsonConvert.SerializeObject(e.Result);
                Debug.WriteLine("Task Data:" + json);
                //Passing the list json to BaseViewModel.
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
        public async void OpenFile(int fileId, bool fileIdSpecified)
        {
            AnalyticsManager.TrackEvent(string.Format("{0} fileId={1}", System.Reflection.MethodBase.GetCurrentMethod().Name, fileId));
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
        /// <summary>
        /// This is used the download the project files data.
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="fileIdSpecified"></param>
        public async void DownloadAndOpenFile(int fileId, bool fileIdSpecified)
        {
            try
            {
                AnalyticsManager.TrackEvent(string.Format("{0} fileId={1}", System.Reflection.MethodBase.GetCurrentMethod().Name, fileId));
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
                            var mime = MimeTypesMap.GetMimeType(e.Result.m_Item1);
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
                CrashReportManager.ReportError(ex, System.Reflection.MethodBase.GetCurrentMethod().Name, tag);
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
                AnalyticsManager.TrackEvent(string.Format("{0} fileId={1}", System.Reflection.MethodBase.GetCurrentMethod().Name, fileId));
                MobileTimService service = new MobileTimService(GlobalConstants.GetAppURL());
                FileCompletedEvent(service, fileId);
                service.GetFileAsync(GlobalConstants.IMEINumber, GlobalConstants.VersionNumber, fileId, fileIdSpecified);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Error in DownloadFile: {0}", ex?.Message));
                CrashReportManager.ReportError(ex, System.Reflection.MethodBase.GetCurrentMethod().Name, tag);
            }
        }

        /// <summary>
        /// This is used the save the file in local FileSystem after getting the information from service.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="fileId"></param>
        /// <returns></returns>
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

        /// <summary>
        /// This is used to sync the local data to server.
        /// </summary>
        /// <param name="isFromAutoSync"></param>
        public void SyncTaskList(bool isFromAutoSync = false)
        {
            try
            {
                AnalyticsManager.TrackEvent(System.Reflection.MethodBase.GetCurrentMethod().Name);
                GetTasksListIDsFortheData(isFromAutoSync);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Error in SyncTaskList: {0}", ex?.Message));
                CrashReportManager.ReportError(ex, System.Reflection.MethodBase.GetCurrentMethod().Name, tag);
            }
        }

        /// <summary>
        /// This is used to get the App information.
        /// </summary>
        /// <param name="actionAppUpdate"></param>
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
                CrashReportManager.ReportError(ex, System.Reflection.MethodBase.GetCurrentMethod().Name, tag);
            }
        }

        /// <summary>
        /// QueryAppUpdateCompleted callback.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimService_QueryAppUpdateCompleted(object sender, QueryAppUpdateCompletedEventArgs e)
        {
            if (e.Error == null && !e.Cancelled)
            {
                if (e.Result != null)
                {
                    string json = JsonConvert.SerializeObject(e.Result);
                    Debug.WriteLine("App Data: " + json);
                    ActionAppUpdate?.Invoke(json);
                }
            }
        }

        /// <summary>
        /// This is used to upload the file to server.
        /// </summary>
        /// <param name="taskListIdSpecified"></param>
        /// <param name="postId"></param>
        /// <param name="fileId"></param>
        /// <param name="posIdSpecified"></param>
        /// <param name="fileContent"></param>
        /// <param name="extension"></param>
        /// <param name="gps"></param>
        /// <param name="comment"></param>
        /// <param name="time"></param>
        /// <param name="timeSpecified"></param>
        /// <param name="UploadFileResult"></param>
        /// <param name="UploadFileResultSpecified"></param>
        public void UploadFile(bool taskListIdSpecified, int postId, int fileId, bool posIdSpecified, byte[] fileContent, string extension, string gps, string comment, System.DateTime time, bool timeSpecified, out int UploadFileResult, out bool UploadFileResultSpecified)
        {
            try
            {
                AnalyticsManager.TrackEvent(string.Format("{0} taskId={1} postId = {2}", System.Reflection.MethodBase.GetCurrentMethod().Name, postId, fileId));
                timService = new MobileTimService(GlobalConstants.GetAppURL());
                timService.UploadFile(GlobalConstants.IMEINumber, GlobalConstants.VersionNumber, GlobalConstants.GetTaskListId(), taskListIdSpecified, postId, posIdSpecified, fileContent, extension, gps, comment, time, timeSpecified, out UploadFileResult, out UploadFileResultSpecified);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                UploadFileResult = 0;
                UploadFileResultSpecified = false;
                CrashReportManager.ReportError(ex, System.Reflection.MethodBase.GetCurrentMethod().Name, tag);
            }
        }

        /// <summary>
        /// This is used to upload the file to server asynchronously. 
        /// </summary>
        /// <param name="taskListIdSpecified"></param>
        /// <param name="postId"></param>
        /// <param name="fileId"></param>
        /// <param name="posIdSpecified"></param>
        /// <param name="fileContent"></param>
        /// <param name="extension"></param>
        /// <param name="gps"></param>
        /// <param name="comment"></param>
        /// <param name="time"></param>
        /// <param name="timeSpecified"></param>
        public void UploadFileAsync(bool taskListIdSpecified, int postId, int fileId, bool posIdSpecified, byte[] fileContent, string extension, string gps, string comment, System.DateTime time, bool timeSpecified)
        {
            AnalyticsManager.TrackEvent(string.Format("{0} postId={1} fileId = {2}", System.Reflection.MethodBase.GetCurrentMethod().Name, postId, fileId));
            timService = new MobileTimService(GlobalConstants.GetAppURL());
            UploadCompleteEvent(postId, fileId);
            timService.UploadFileAsync(GlobalConstants.IMEINumber, GlobalConstants.VersionNumber, GlobalConstants.GetTaskListId(), taskListIdSpecified, postId, posIdSpecified, fileContent, extension, gps, comment, time, timeSpecified);
        }

        /// <summary>
        /// To set UploadFileCompleted callback.
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="fileId"></param>
        private void UploadCompleteEvent(int postId, int fileId)
        {
            timService.UploadFileCompleted += async (object sender, UploadFileCompletedEventArgs e) =>
            {
                AnalyticsManager.TrackEvent(string.Format("{0} postId={1} fileId = {2}", System.Reflection.MethodBase.GetCurrentMethod().Name, postId, postId));
                FileInfoHelper.Instance.FileUploadCompleted?.Invoke(postId, fileId, e.UploadFileResult, e.UploadFileResultSpecified);
                await FileInfoHelper.Instance.UpdateFileInfoInList(postId, fileId, e.UploadFileResult, e.UploadFileResultSpecified);
            };
        }

        /// <summary>
        /// This is used to post the chnaged File comment to server.
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="fileId"></param>
        /// <param name="fileIdSpecified"></param>
        /// <param name="comment"></param>
        public void ChangeFileComment(int postId, int fileId, bool fileIdSpecified, string comment)
        {
            AnalyticsManager.TrackEvent(string.Format("{0} taskId={1} fileId = {2}", System.Reflection.MethodBase.GetCurrentMethod().Name, postId, fileId));
            timService = new MobileTimService(GlobalConstants.GetAppURL());
            ChangeFileCompleteEvent(postId, fileId, comment);
            timService.ChangeFileCommentAsync(GlobalConstants.IMEINumber, GlobalConstants.VersionNumber, fileId, fileIdSpecified, comment);
        }

        /// <summary>
        /// Callback of ChangeFileComment.
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="fileId"></param>
        /// <param name="comment"></param>
        private void ChangeFileCompleteEvent(int postId, int fileId, string comment)
        {
            timService.ChangeFileCommentCompleted += async (object sender, ChangeFileCommentCompletedEventArgs e) =>
            {
                AnalyticsManager.TrackEvent(string.Format("{0} taskId={1} fileId = {2}", System.Reflection.MethodBase.GetCurrentMethod().Name, postId, fileId));
                FileInfoHelper.Instance.CommentUpdatedCompleted?.Invoke(postId, fileId);
                await FileInfoHelper.Instance.UpdateFileComment(postId, fileId, comment);
            };
        }

        /// <summary>
        /// This is used to delete the file from server.
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="fileId"></param>
        /// <param name="fileIdSpecified"></param>
        public void DeleteFile(int postId, int fileId, bool fileIdSpecified)
        {
            AnalyticsManager.TrackEvent(System.Reflection.MethodBase.GetCurrentMethod().Name);
            timService = new MobileTimService(GlobalConstants.GetAppURL());
            DeleteFileCompleteEvent(postId, fileId);
            timService.DeleteFileAsync(GlobalConstants.IMEINumber, GlobalConstants.VersionNumber, fileId, fileIdSpecified);
        }

        /// <summary>
        /// To intialize the DeleteFileCompleted callback. 
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="fileId"></param>
        private void DeleteFileCompleteEvent(int postId, int fileId)
        {
            timService.DeleteFileCompleted += async (object sender, System.ComponentModel.AsyncCompletedEventArgs e) =>
            {
                await FileInfoHelper.Instance.DeleteFileInList(postId, fileId);
                FileInfoHelper.Instance.DeleteCompleted?.Invoke(postId, fileId);
            };
        }

        /// <summary>
        /// This is used to post the result from server.
        /// </summary>
        /// <param name="taskResultJson"></param>
        public void PostResultAsync(string taskResultJson)
        {
            AnalyticsManager.TrackEvent(System.Reflection.MethodBase.GetCurrentMethod().Name);
            var list = JsonConvert.DeserializeObject<TaskResult[]>(taskResultJson);
            timService = new MobileTimService(GlobalConstants.GetAppURL());
            timService.PostResponsesCompleted += async (object sender, System.ComponentModel.AsyncCompletedEventArgs e) =>
            {
                if (e.Error != null || e.Cancelled)
                {
                    PostResultHelper.Instance.SaveTaskResults(JsonConvert.DeserializeObject<mTIM.Models.TaskResult[]>(taskResultJson));
                }
            };
            timService.PostResponsesAsync(GlobalConstants.IMEINumber, GlobalConstants.VersionNumber, list);
        }
    }
}
