using System;
using System.Threading.Tasks;
using mTIM.ViewModels;

namespace mTIM.Interfaces
{
    public interface IWebService
    {
        public BaseViewModel ViewModel { get; set; }
        public Action<bool> ActionRefreshCallBack { get; set; }
        void QueryAppUpdate(Action<string> actionAppUpdate);
        void GetTasksListIDsFortheData(bool isFromAutoSync = false);
        void SyncTaskList(string taskListJson, bool isFromAutoSync = false);
        void DownloadFile(int fileId, bool fileIdSpecified);
        void OpenPdfFile(int fileId, bool fileIdSpecified);
    }
}
