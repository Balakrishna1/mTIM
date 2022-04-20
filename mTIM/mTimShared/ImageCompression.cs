using System;

#if __ANDROID__
using System.IO;
using Android.Graphics;
#elif __IOS__
using Foundation;
using UIKit;
#endif

namespace mTimShared
{
    public static class ImageCompression
    {
        static ImageCompression()
        {
        }

        public static byte[] CompressImageBytes(byte[] imageData, int compressionPercentage)
        {
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
