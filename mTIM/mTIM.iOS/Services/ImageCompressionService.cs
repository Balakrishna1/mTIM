using mTIM.Interfaces;
using mTIM.iOS.Services;
using mTimShared;

[assembly: Xamarin.Forms.Dependency(typeof(ImageCompressionService))]
namespace mTIM.iOS.Services
{
    public class ImageCompressionService : IImageCompressionService
    {
        public byte[] CompressImageBytes(byte[] imageData, int compressionPercentage)
        {
            return ImageCompression.CompressImageBytes(imageData, compressionPercentage);
        }
    }
}
