using System;
namespace mTIM.Interfaces
{
    public interface IDevice
    {
        string GetImeiNumeber();
        string GetDeviceID();
        string GetUniqueID();
    }
}
