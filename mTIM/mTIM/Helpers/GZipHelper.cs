using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using ProtoBuf;

namespace mTIM.Helpers
{
    public class GZipHelper
    {
        private static void CopyTo(Stream sourceStream, Stream destinationStream)
        {
            byte[] bytes = new byte[4096];
            int count;
            while ((count = sourceStream.Read(bytes, 0, bytes.Length)) != 0)
            {
                destinationStream.Write(bytes, 0, count);
            }
        }

        public static byte[] Zip(String stringValue)
        {
            var bytes = Encoding.UTF8.GetBytes(stringValue);
            using (var inputStream = new MemoryStream(bytes))
            {
                using (var outputStream = new MemoryStream())
                {
                    using (var gzipStream = new GZipStream(outputStream, CompressionMode.Compress))
                    {
                        CopyTo(inputStream, gzipStream);
                    }
                    return outputStream.ToArray();
                }
            }
        }
        public static String Unzip(byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }

        public static byte[] Decompress(byte[] bytes)
        {
            using (var inputStream = new MemoryStream(bytes))
            {
                using (var outputStream = new MemoryStream())
                {
                    using (var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress))
                    {
                        string deserialized = Serializer.Deserialize<string>(gzipStream);
                        CopyTo(gzipStream, outputStream);
                    }

                    return outputStream.ToArray();
                }
            }
        }

        public static MemoryStream LoadDecompressed(byte[] data)
        {
            using (var compressedStream = new MemoryStream(data))
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            using (var resultStream = new MemoryStream())
            {
                zipStream.CopyTo(resultStream);
                MemoryStream stream = new MemoryStream(resultStream.ToArray(), 0, (int)resultStream.Length, true, publiclyVisible: true);
                return stream;
            }
        }


        public static mTIM.Models.D.Result DeserializeResult(byte[] compressedBytes)
        {
            using (var compressedStream = new MemoryStream(compressedBytes))
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            {
                return Serializer.Deserialize<mTIM.Models.D.Result>(zipStream);
            }
        }

    }
}
