using System;
using mTIM.Droid.Services;
using mTIM.Interfaces;
using mTIM.ViewModels;
using mTimShared;
using Xamarin.Forms;

[assembly: Dependency(typeof(WebSevice))]
namespace mTIM.Droid.Services
{
    public class WebSevice : IWebService
    {
        public BaseViewModel ViewModel { get; set; }
        public Action<bool> ActionRefreshCallBack { get; set; }
        public Action<bool> GraphicsDownloadedCallBack { get; set; }

        private void intialize()
        {
            mTimService.Instance.ViewModel = ViewModel;
            mTimService.Instance.ActionRefreshCallBack = ActionRefreshCallBack;
            mTimService.Instance.GraphicsDownloadedCallBack = GraphicsDownloadedCallBack;
        }

        /// <summary>
        /// This method is used the Tasks list for the day.
        /// </summary>
        /// <param name="isFromAutoSync"></param>
        public void GetTasksListIDsFortheData(bool isFromAutoSync = false)
        {
            intialize();
            mTimService.Instance.GetTasksListIDsFortheData(isFromAutoSync);
        }

        /// <summary>
        /// This is used to open the pdf file.
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="fileIdSpecified"></param>
        public void OpenFile(int fileId, bool fileIdSpecified)
        {
            intialize();
            mTimService.Instance.OpenFile(fileId, fileIdSpecified);
        }

        public void DownloadAndOpenFile(int fileId, bool fileIdSpecified)
        {
            intialize();
            mTimService.Instance.DownloadAndOpenFile(fileId, fileIdSpecified);
        }

        /// <summary>
        ///This method is used to download the file.
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="fileIdSpecified"></param>
        /// <returns></returns>
        public void DownloadFile(int fileId, bool fileIdSpecified)
        {
            intialize();
            mTimService.Instance.DownloadFile(fileId, fileIdSpecified);
        }

        public void SyncTaskList(bool isFromAutoSync = false)
        {
            intialize();
            mTimService.Instance.SyncTaskList(isFromAutoSync);
        }

        public void QueryAppUpdate(Action<string> actionAppUpdate)
        {
            intialize();
            mTimService.Instance.QueryAppUpdate(actionAppUpdate);
        }

        public void UploadFile(bool taskListIdSpecified, int postId, int fileId, bool posIdSpecified, byte[] fileContent, string extension, string gps, string comment, System.DateTime time, bool timeSpecified, out int UploadFileResult, out bool UploadFileResultSpecified)
        {
            intialize();
            mTimService.Instance.UploadFile(true, postId, fileId, posIdSpecified, fileContent, extension, gps, comment, time, timeSpecified, out UploadFileResult, out UploadFileResultSpecified);
        }

        public void UploadFileAsync(bool taskListIdSpecified, int postId, int fileId, bool posIdSpecified, byte[] fileContent, string extension, string gps, string comment, DateTime time, bool timeSpecified)
        {
            intialize();
            mTimService.Instance.UploadFileAsync(true, postId, fileId, posIdSpecified, fileContent, extension, gps, comment, time, timeSpecified);
        }

        public void ChangeFileComment(int postId, int fileId, bool fileIdSpecified, string comment)
        {
            intialize();
            mTimService.Instance.ChangeFileComment(postId, fileId, fileIdSpecified, comment);
        }

        public void DeleteFile(int postId, int fileId, bool fileIdSpecified)
        {
            intialize();
            mTimService.Instance.DeleteFile(postId, fileId, fileIdSpecified);
        }

        public void PostReultAsync(string jsonresult)
        {
            intialize();
            mTimService.Instance.PostResultAsync(jsonresult);
        }
    }
}
