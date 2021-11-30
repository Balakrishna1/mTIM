using System;
using mTIM.Models;

namespace mTIM.Interfaces
{
    public interface IDevice
    {
        string GetImeiNumeber();
        string GetDeviceID();
        string GetUniqueID();
        void DownloadAndInstallAPK(AppVersionUpdateInfo versionInfo);
        void CloseApplication();
    }
}
