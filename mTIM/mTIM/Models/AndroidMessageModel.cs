using System;
using Newtonsoft.Json;

namespace mTIM.Models
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class AndroidMessageModel
    {
        // AndroidMessageBuilder amb = new AndroidMessageBuilder();
        // amb.Add("VersionName");
        //amb.Add(versionName);
        //amb.Add("VersionCode");
        //amb.Add(versionCode);
        //amb.Add("Build.Brand");
        //amb.Add(android.os.Build.BRAND);
        //amb.Add("Build.Device");
        //amb.Add(android.os.Build.DEVICE);
        //amb.Add("PseudoID");
        //amb.Add(this.calcUDID());
        //amb.Add("IMEI");
        //amb.Add(imeString);
        //amb.Add("StatusBarHeight");
        //amb.Add(getStatusBarHeight());
        //amb.Add("DeviceId");
        //amb.Add(deviceId);

        public string VersionName { get; set; }
        public string VersionCode { get; set; }
        public string Brand { get; set; }
        public string Device { get; set; }
        public string PseudoID { get; set; }
        public string IMEI { get; set; }
        public string DeviceId { get; set; }
    }
}
