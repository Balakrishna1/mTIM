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
        Task QueryAppUpdate(Action<string> actionAppUpdate);
        Task GetTasksListIDsFortheData(bool isFromAutoSync = false);
        Task SyncTaskList(string taskListJson, bool isFromAutoSync = false);
        Task DownloadFile(int fileId, bool fileIdSpecified);
        Task OpenFile(int fileId, bool fileIdSpecified);
        void UploadFile(bool taskListIdSpecified, int taskId, int posId, bool posIdSpecified, byte[] fileContent, string extension, string gps, string comment, System.DateTime time, bool timeSpecified, out int UploadFileResult, out bool UploadFileResultSpecified);
        Task UploadFileAsync(bool taskListIdSpecified, int taskId, int posId, bool posIdSpecified, byte[] fileContent, string extension, string gps, string comment, System.DateTime time, bool timeSpecified);
        Task ChangeFileComment(int taskId, int fileId, bool fileIdSpecified, string comment);
        Task DeleteFile(int taskId, int fileId, bool fileIdSpecified);
    }
}
