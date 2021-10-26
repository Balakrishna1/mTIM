using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace mTIM.Helpers
{
    public static class FileHelper
    {
        public static string AppDirectory = Path.Combine(FileSystem.AppDataDirectory , GlobalConstants.CACHEPATH);
        public static void CreateAppDirectory()
        {
            if (!Directory.Exists(AppDirectory))
            {
                Directory.CreateDirectory(AppDirectory);
            }
        }

        public static string GetFilePath(string fileName)
        {
            CreateAppDirectory();
            return Path.Combine(AppDirectory, fileName);
        }

        public static bool IsFileExists(string fileName)
        {
            CreateAppDirectory();
            string file = Path.Combine(AppDirectory, fileName);
            return File.Exists(file);
        }

        public static async Task WriteTextAsync(string fileName, string content)
        {
            CreateAppDirectory();
            string file = Path.Combine(AppDirectory, fileName);
            if(File.Exists(file))
            {
                File.Delete(file);
            }
            await File.WriteAllTextAsync(file, content);
        }

        public static void WriteText(string fileName, string content)
        {
            CreateAppDirectory();
            string file = Path.Combine(AppDirectory, fileName);
            if (!Directory.Exists(file))
            {
                Directory.CreateDirectory(file);
            }
            else
            {
                File.Delete(file);
            }
            File.WriteAllText(file, content);
        }

        public static async Task WriteAllBytesAsync(string fileName, byte[] content)
        {
            CreateAppDirectory();
            string file = Path.Combine(AppDirectory, fileName);
            if (File.Exists(file))
            {
                File.Delete(fileName);
            }
            await File.WriteAllBytesAsync(file, content);
        }

        public static async Task<string> ReadTextAsync(string fileName)
        {
            string data = string.Empty;
            CreateAppDirectory();
            string file = Path.Combine(AppDirectory, fileName);
            if (File.Exists(file))
            {
                data =  await File.ReadAllTextAsync(file);
            }
            return data;
        }

        public static string ReadText(string fileName)
        {
            string data = string.Empty;
            CreateAppDirectory();
            string file = Path.Combine(AppDirectory, fileName);
            if (File.Exists(file))
            {
                data = File.ReadAllText(file);
            }
            return data;
        }

        public static async Task<byte[]> ReadAllBytesAsync(string fileName)
        {
            CreateAppDirectory();
            string file = Path.Combine(AppDirectory, fileName);
            if (File.Exists(file))
            {
                return await File.ReadAllBytesAsync(file);
            }
            else
            {
                return new byte[0];
            }
        }

        public static void DeleteDirectory(string fileName)
        {
            string file = Path.Combine(AppDirectory, fileName);
            if (Directory.Exists(file))
            {
                Directory.Delete(file);
            }
        }
    }
}
