using System;
using System.IO;
using System.Net;
using System.Text;
using Acr.UserDialogs;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Telephony;
using Android.Util;
using Java.Lang;
using Java.Security;
using mTIM.Droid.Services;
using mTIM.Interfaces;
using mTIM.Models;
using Xamarin.Essentials;
using static Android.Provider.Settings;

[assembly: Xamarin.Forms.Dependency(typeof(AndroidDevice))]
namespace mTIM.Droid.Services
{
    public class AndroidDevice : IDevice
    {
        public void CloseApplication()
        {
            MvxAppCompatActivityBase.CurrentActivity?.FinishAffinity();
        }

        /// <summary>
        /// Downloading  and saving the application locally by using webclient.
        /// By following this reference installing the app: http://www.wepstech.com/download-and-install-app-programmatically/
        /// </summary>
        /// <param name="versionInfo"></param>
        public void DownloadAndInstallAPK(AppVersionUpdateInfo versionInfo)
        {
            var downloadPath = Path.Combine(Android.App.Application.Context.GetExternalFilesDir(null).AbsolutePath, "AppUpdate");
            using (WebClient client = new WebClient())
            {
                try
                {
                    if (!Directory.Exists(downloadPath.ToString()))
                    {
                        Directory.CreateDirectory(downloadPath.ToString());
                    }

                    UserDialogs.Instance.ShowLoading("Donloading app.", MaskType.Gradient);
                    Uri uri = new Uri(versionInfo.Url);
                    client.DownloadFileCompleted += (s, e) =>
                    {
                        UserDialogs.Instance.ShowLoading("Updating app.", MaskType.Gradient);
                        Java.IO.File updatePath = new Java.IO.File(MvxAppCompatActivityBase.CurrentActivity.GetExternalFilesDir(null).AbsolutePath, "AppUpdate");
                        Java.IO.File toInstall = new Java.IO.File(updatePath, "update.apk");

                        if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                        {
                            Android.Net.Uri apkUri = FileProvider.GetUriForFile(MvxAppCompatActivityBase.CurrentActivity, AppInfo.PackageName + ".provider", toInstall);
                            Intent intentS = new Intent(Intent.ActionView);
                            intentS.SetDataAndType(apkUri, "application/vnd.android.package-archive");
                            var resInfoList = MvxAppCompatActivityBase.CurrentActivity.PackageManager.QueryIntentActivities(intentS, PackageInfoFlags.MatchDefaultOnly);
                            foreach (ResolveInfo resolveInfo in resInfoList)
                            {
                                MvxAppCompatActivityBase.CurrentActivity.GrantUriPermission(MvxAppCompatActivityBase.CurrentActivity.ApplicationContext.PackageName+ ".provider", apkUri, ActivityFlags.GrantWriteUriPermission | ActivityFlags.GrantReadUriPermission);
                            }
                            intentS.SetFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.GrantWriteUriPermission | ActivityFlags.GrantReadUriPermission);
                            MvxAppCompatActivityBase.CurrentActivity.StartActivity(intentS);
                            UserDialogs.Instance.HideLoading();
                        }
                        else
                        {
                            Android.Net.Uri apkUri = Android.Net.Uri.FromFile(toInstall);
                            Intent intentS = new Intent(Intent.ActionView);
                            intentS.SetDataAndType(apkUri, "application/vnd.android.package-archive");
                            intentS.SetFlags(ActivityFlags.NewTask);
                            intentS.SetFlags(ActivityFlags.GrantReadUriPermission);
                            intentS.SetFlags(ActivityFlags.ClearTop);
                            intentS.PutExtra(Intent.ExtraNotUnknownSource, true);
                            MvxAppCompatActivityBase.CurrentActivity.StartActivity(intentS);
                        }
                    };
                    client.DownloadFileAsync(uri, downloadPath + "/" + "update.apk");
                }
                catch (System.Exception ex)
                {
                    UserDialogs.Instance.HideLoading();
                    Log.Debug("Fluchtpunkt", "Download Exception: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// This is used to get the device id.
        /// </summary>
        /// <returns></returns>
        public string GetDeviceID()
        {
            return Secure.GetString(Android.App.Application.Context.ContentResolver, Secure.AndroidId);
        }

        /// <summary>
        /// Android 10 onwords OS restricted to get the IMEI number from device, So instead of IMEI number I am restuning the AndroidId.
        /// </summary>
        /// <returns></returns>
        public string GetImeiNumeber()
        {
            string imeiNuber = string.Empty;
            TelephonyManager mTelephonyMgr;
            mTelephonyMgr = (TelephonyManager)Android.App.Application.Context.GetSystemService(Context.TelephonyService);
            if(mTelephonyMgr!= null)
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
                {
                    imeiNuber = Secure.GetString(Android.App.Application.Context.ContentResolver, Secure.AndroidId);

                }
                else if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    imeiNuber = mTelephonyMgr.Imei;
                }
                else
                {
                    imeiNuber = mTelephonyMgr.DeviceId;
                }
            }
            return imeiNuber;
        }

        public string GetUniqueID()
        {
            string androidID = GetDeviceID();             Log.Debug("Fluchtpunkt", "ID: ANDROID_ID: " + androidID);

            string imeString = string.Empty;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
                imeString = string.Empty;
            else
                imeString = GetImeiNumeber();
            Log.Debug("Fluchtpunkt", "ID: getDeviceId: " + imeString);              string pseudoId = imeString + androidID;             if (pseudoId.Length > 0)
            {
                string pseudoIdHash = string.Empty;                 Log.Debug("Fluchtpunkt", "ID: PseudoId without hash: " + pseudoId);                  MessageDigest m;                 try                 {                     m = MessageDigest.GetInstance("MD5");                     m.Update(Encoding.ASCII.GetBytes(pseudoId), 0, pseudoId.Length);                     byte[] md5Data = m.Digest();                     for (int i = 0; i < md5Data.Length; i++)                     {                         int b = (0xFF & md5Data[i]);                         // if it is a single digit, make sure it have 0 in front (proper padding)                         if (b <= 0xF) pseudoIdHash += "0";                         // add number to string                         pseudoIdHash += Integer.ToHexString(b);                     }                         pseudoIdHash = pseudoIdHash.ToLower();                 } catch (NoSuchAlgorithmException e)                 {                     // TODO Auto-generated catch block                     e.PrintStackTrace();                 }                 Log.Debug("Fluchtpunkt", "ID: PseudoId hashed: " + pseudoIdHash);                 return pseudoIdHash;             }             else             {                 pseudoId = "P" + getRandomHexString(24);                 Log.Debug("Fluchtpunkt", "ID: PseudoId created: " + pseudoId);                 return pseudoId;
            }
        }

        private string getRandomHexString(int numchars)
        {
            Random r = new Random();
            StringBuffer sb = new StringBuffer();
            while (sb.Length() < numchars)
            {
                sb.Append(Integer.ToHexString(r.Next()));
            }

            return sb.ToString().Substring(0, numchars);
        }
    }
}
