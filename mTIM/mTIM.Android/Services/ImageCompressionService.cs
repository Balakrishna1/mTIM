using mTIM.Droid.Services;
using mTIM.Interfaces;
using mTimShared;

[assembly: Xamarin.Forms.Dependency(typeof(ImageCompressionService))]
namespace mTIM.Droid.Services
{
    public class ImageCompressionService : IImageCompressionService
    {
        public byte[] CompressImageBytes(byte[] imageData, int compressionPercentage)
        {
            return ImageCompression.CompressImageBytes(imageData, compressionPercentage);
        }
    }
}
