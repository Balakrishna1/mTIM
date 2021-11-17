using System;
using System.Threading.Tasks;
using mTIM.ViewModels;

namespace mTIM.Interfaces
{
    public interface IWebService
    {
        public BaseViewModel ViewModel { get; set; }
        void GetTasksListIDsFortheData(bool isFromAutoSync = false);
        void SyncTaskList(string taskListJson);
        void DownloadFile(int fileId, bool fileIdSpecified);
        void OpenPdfFile(int fileId, bool fileIdSpecified);
    }
}
