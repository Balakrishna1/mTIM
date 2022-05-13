using System.Diagnostics;
using mTIM.Models;

namespace mTIM.Helpers
{
    public static class GlobalConstants
    {
        public static string SelectedLanguage { get; set; } = string.Empty;
        public static int StatusSyncTime { get; set; }
        public static string AppBaseURL { get; set; } = string.Empty;
        public static Location LocationDetails { get; set; }
        public static string VersionNumber { get; set; }
        public static string VersionCode { get; set; }
        public static string IMEINumber { get; set; }
        public static string DeviceID { get; set; }
        public static string UniqueID { get; set; }
        public const string VERSION = "v1.2";
        public const string DATA_VERSION = "v0.6";
        public static string CACHEPATH = "cache/" + VERSION + "/" + GetUser() + "/";
        public const string SETTINGS_FILE = "settings.json";
        public const string IMEI_FILE = "imei_02.json";
        public const string TASKLIST_FILE = "tasklist.json";
        public const string FILES_INFO = "filesinformation.json";
        public const string GraphicsBlob_FILE = "model_3d.proto";
        public const string FileExtesons = "file_extesons.json";
        public static  bool IsLandscape = false;
        public static int DefaultSyncMinites { get; set; } = 5;
        public static int SyncMinutes { get; set; }

        public static string GetUser()
        {
#if DEBUG
            return "354473060142487";
#else 
            return UniqueID;
#endif
        }



        /// <summary>
        /// This is used to get the baseURL.
        /// </summary>
        /// <returns></returns>
        public static string GetAppURL()
        {
            var baseUrl = GlobalConstants.AppBaseURL;
            if (baseUrl.Length > 0)
            {
                if (!baseUrl.EndsWith('/'))
                    baseUrl = baseUrl + '/';
                baseUrl = baseUrl + "External/" + GlobalConstants.VERSION + "/External.svc";
            }
            //else
            //{
            //    baseUrl = "http://";
            //}
            Debug.WriteLine("App URL:" + baseUrl);
            return baseUrl;
        }
    }
}
