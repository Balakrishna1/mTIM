using System;
using mTIM.ViewModels;

namespace mTIM.Interfaces
{
    public interface IWebService
    {
        public BaseViewModel ViewModel { get; set; }
        void GetTasksListIDsFortheData(bool isFromAutoSync = false);
    }
}
