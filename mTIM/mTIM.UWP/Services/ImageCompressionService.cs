using mTIM.Interfaces;
using mTIM.UWP.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: Xamarin.Forms.Dependency(typeof(ImageCompressionService))]
namespace mTIM.UWP.Services
{
    class ImageCompressionService : IImageCompressionService
    {
        public byte[] CompressImageBytes(byte[] imageData, int compressionPercentage)
        {
            return imageData;
        }
    }
}
