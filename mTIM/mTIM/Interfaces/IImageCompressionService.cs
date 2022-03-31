using System;
namespace mTIM.Interfaces
{
    public interface IImageCompressionService
    {
        byte[] CompressImageBytes(byte[] imageData, int compressionPercentage);
    }
}
