using System;

#if __ANDROID__
using System.IO;
using Android.Graphics;
using mTIM.Managers;
#elif __IOS__
using Foundation;
using mTIM.Managers;
using UIKit;
#endif

namespace mTimShared
{
    public static class ImageCompression
    {
        static ImageCompression()
        {
        }

        /// <summary>
        /// This methos is used to get the resized/compressed image byte code.
        /// </summary>
        /// <param name="imageData"></param>
        /// <param name="compressionPercentage"></param>
        /// <returns></returns>
        public static byte[] CompressImageBytes(byte[] imageData, int compressionPercentage)
        {
            AnalyticsManager.TrackEvent(System.Reflection.MethodBase.GetCurrentMethod().Name);
            return GetResizedImage(imageData, compressionPercentage);
        }

#if __ANDROID__
        private static byte[] GetResizedImage(byte[] imageData, int compressionPercentage)
        {
            Bitmap originalImage = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length);
            using (MemoryStream ms = new MemoryStream())
            {
                originalImage.Compress(Bitmap.CompressFormat.Jpeg, compressionPercentage, ms);
                return ms.ToArray();
            }
        }
#elif __IOS__
        private static byte[] GetResizedImage(byte[] imageData, int compressionPercentage)
        {
            UIImage originalImage = new UIImage(NSData.FromArray(imageData));
            if (originalImage != null)
            {
                nfloat comressionQuality = (nfloat)(compressionPercentage / 100);
                return originalImage.AsJPEG(comressionQuality).ToArray();
            }
            else
            {
                return null;
            }
        }
#endif
    }
}
