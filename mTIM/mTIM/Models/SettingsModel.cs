using System;
using mTIM.Helpers;

namespace mTIM.Models
{
    public class SettingsModel
    {
        //writer.Write("BaseUrl: ");
        //writer.Write(GetBaseUrl(false));
        //writer.Write("\n");
        //writer.Write("Language: ");
        //writer.Write(language);
        //writer.Write("\n");
        //writer.Write("StatusSyncTime: ");
        //writer.Write(String(statusSyncTime));

        public string BaseUrl { get; set; } 
        public string Language { get; set; }
        public int StatusSyncTime { get; set; }
    }
}
