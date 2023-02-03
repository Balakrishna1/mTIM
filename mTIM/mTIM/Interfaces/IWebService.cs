using System;
using System.Threading.Tasks;
using mTIM.Models;
using mTIM.ViewModels;

namespace mTIM.Interfaces
{
    public interface IWebService
    {
        public BaseViewModel ViewModel { get; set; }
        public Action<bool> ActionRefreshCallBack { get; set; }
        public Action<bool> GraphicsDownloadedCallBack { get; set; }
        void QueryAppUpdate(Action<string> actionAppUpdate);
        void GetTasksListIDsFortheData(bool isFromAutoSync = false);
        void SyncTaskList(bool isFromAutoSync = false);
        void DownloadFile(int fileId, bool fileIdSpecified);
        void OpenFile(int fileId, bool fileIdSpecified);
        void UploadFile(bool taskListIdSpecified, int postId, int fileId, bool posIdSpecified, byte[] fileContent, string extension, string gps, string comment, System.DateTime time, bool timeSpecified, out int UploadFileResult, out bool UploadFileResultSpecified);
        void UploadFileAsync(bool taskListIdSpecified, int postId, int fileId, bool posIdSpecified, byte[] fileContent, string extension, string gps, string comment, System.DateTime time, bool timeSpecified);
        void ChangeFileComment(int postId, int fileId, bool fileIdSpecified, string comment);
        void DeleteFile(int postId, int fileId, bool fileIdSpecified);
        void PostReultAsync(string jsonresult);
    }
}
