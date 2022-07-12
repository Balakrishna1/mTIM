using System;
using System.Threading.Tasks;
using mTIM.ViewModels;

namespace mTIM.Interfaces
{
    public interface IWebService
    {
        BaseViewModel ViewModel { get; set; }
        Action<bool> ActionRefreshCallBack { get; set; }
        Action<bool> GraphicsDownloadedCallBack { get; set; }
        void QueryAppUpdate(Action<string> actionAppUpdate);
        void GetTasksListIDsFortheData(bool isFromAutoSync = false);
        void SyncTaskList(string taskListJson, bool isFromAutoSync = false);
        void DownloadFile(int fileId, bool fileIdSpecified);
        void OpenFile(int fileId, bool fileIdSpecified);
        void UploadFile(bool taskListIdSpecified, int taskId, int posId, bool posIdSpecified, byte[] fileContent, string extension, string gps, string comment, System.DateTime time, bool timeSpecified, out int UploadFileResult, out bool UploadFileResultSpecified);
        void UploadFileAsync(bool taskListIdSpecified, int taskId, int posId, bool posIdSpecified, byte[] fileContent, string extension, string gps, string comment, System.DateTime time, bool timeSpecified);
        void ChangeFileComment(int taskId, int fileId, bool fileIdSpecified, string comment);
        void DeleteFile(int taskId, int fileId, bool fileIdSpecified);
    }
}
