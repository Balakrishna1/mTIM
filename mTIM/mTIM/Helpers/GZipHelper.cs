using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using mTIM.Managers;
using ProtoBuf;

namespace mTIM.Helpers
{
    public class GZipHelper
    {
        public static mTIM.Models.D.Result DeserializeResult(byte[] compressedBytes)
        {
            AnalyticsManager.TrackEvent(System.Reflection.MethodBase.GetCurrentMethod().Name);
            using (var compressedStream = new MemoryStream(compressedBytes))
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            {
                return Serializer.Deserialize<mTIM.Models.D.Result>(zipStream);
            }
        }

    }
}
