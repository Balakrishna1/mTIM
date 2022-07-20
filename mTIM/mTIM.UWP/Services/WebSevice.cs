using mTIM.Interfaces;
using mTIM.UWP.Services;
using mTIM.ViewModels;
using System;

[assembly: Xamarin.Forms.Dependency(typeof(WebSevice))]
namespace mTIM.UWP.Services
{
    class WebSevice : IWebService
    {
        public BaseViewModel ViewModel { get; set; }
        public Action<bool> ActionRefreshCallBack { get; set; }
        public Action<bool> GraphicsDownloadedCallBack { get; set; }

        public void ChangeFileComment(int taskId, int fileId, bool fileIdSpecified, string comment)
        {
        }

        public void DeleteFile(int taskId, int fileId, bool fileIdSpecified)
        {
        }

        public void DownloadFile(int fileId, bool fileIdSpecified)
        {
        }

        public void GetTasksListIDsFortheData(bool isFromAutoSync = false)
        {
        }

        public void OpenFile(int fileId, bool fileIdSpecified)
        {
        }

        public void QueryAppUpdate(Action<string> actionAppUpdate)
        {
        }

        public void SyncTaskList(string taskListJson, bool isFromAutoSync = false)
        {
        }

        public void UploadFile(bool taskListIdSpecified, int taskId, int posId, bool posIdSpecified, byte[] fileContent, string extension, string gps, string comment, DateTime time, bool timeSpecified, out int UploadFileResult, out bool UploadFileResultSpecified)
        {
            UploadFileResult = 0;
            UploadFileResultSpecified = true;
        }

        public void UploadFileAsync(bool taskListIdSpecified, int taskId, int posId, bool posIdSpecified, byte[] fileContent, string extension, string gps, string comment, DateTime time, bool timeSpecified)
        {
        }
    }
}
