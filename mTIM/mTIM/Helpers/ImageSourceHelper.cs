using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xamarin.Forms;

namespace mTIM.Helpers
{
    public static class ImageSourceHelper
    {
        public static ImageSource GetImageSource(string imageName)
        {
            if(!imageName.EndsWith(".png") && !imageName.EndsWith(".jpg"))
            {
                imageName = string.Format("{0}{1}", imageName, ".png");
            }
                imageName = string.Format("{0}{1}", "mTIM.Images.", imageName);
            return ImageSource.FromResource(imageName, typeof(ImageSourceHelper).GetTypeInfo().Assembly);
        }
    }
}
