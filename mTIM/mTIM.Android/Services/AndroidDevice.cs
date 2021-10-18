using System;
using System.Text;
using Android.Content;
using Android.OS;
using Android.Telephony;
using Android.Util;
using Java.Lang;
using Java.Security;
using mTIM.Droid.Services;
using mTIM.Interfaces;
using static Android.Provider.Settings;

[assembly: Xamarin.Forms.Dependency(typeof(AndroidDevice))]
namespace mTIM.Droid.Services
{
    public class AndroidDevice : IDevice
    {
        public string GetDeviceID()
        {
            return Secure.GetString(Android.App.Application.Context.ContentResolver, Secure.AndroidId);
        }

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

            string imeString = GetImeiNumeber();
            Log.Debug("Fluchtpunkt", "ID: getDeviceId: " + imeString);             //Log.Debug("Fluchtpunkt", "IME: " + imeString);              string pseudoId = imeString + androidID;             if (pseudoId.Length > 0)
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
