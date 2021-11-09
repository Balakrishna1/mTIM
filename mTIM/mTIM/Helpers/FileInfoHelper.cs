using System;
using System.Collections.Generic;
using System.Linq;
using mTIM.Models;
using Newtonsoft.Json;

namespace mTIM.Helpers
{
    public class FileInfoHelper
    {
        Dictionary<int, string> FileExtensions = new Dictionary<int, string>();

        public List<FileInfoModel> TotalFilesList = new List<FileInfoModel>();

        public static FileInfoHelper _instance;
        public static FileInfoHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FileInfoHelper();
                }

                return _instance;
            }
        }

        public async void SaveFileInfo(string json)
        {
            if (!string.IsNullOrEmpty(json))
            {
                await FileHelper.WriteTextAsync(GlobalConstants.FILES_INFO, json);
                var list = JsonConvert.DeserializeObject<List<FileInfoModel>>(json);
                TotalFilesList.Clear();
                if (list != null)
                {
                    TotalFilesList.AddRange(list);
                }
            }
        }

        public async void LoadFileList()
        {
            if(FileHelper.IsFileExists(GlobalConstants.FILES_INFO))
            {
                var json = await FileHelper.ReadTextAsync(GlobalConstants.FILES_INFO);
                var list = JsonConvert.DeserializeObject<List<FileInfoModel>>(json);
                TotalFilesList.Clear();
                if (list != null)
                {
                    TotalFilesList.AddRange(list);
                }
            }
        }

        public async void LoadExtensions()
        {
            if (FileHelper.IsFileExists(GlobalConstants.FileExtesons))
            {
                var json = await FileHelper.ReadTextAsync(GlobalConstants.FileExtesons);
                FileExtensions = JsonConvert.DeserializeObject<Dictionary<int, string>>(json);
            }
        }

        public int GetExtensionsCount()
        {
            return FileExtensions?.Count ?? 0;
        }

        public int GetCount(int id)
        {
            int count = 0;
            if (TotalFilesList?.Count > 0)
            {
                var item = TotalFilesList.Where(x => x.Key.Equals(id)).FirstOrDefault();
                if (item != null && item.Values != null)
                {
                    count = item.Values.Count;
                }
            }
            return count;
        }

        public List<FileInfo> GetValues(int id)
        {
            List<FileInfo> values = new List<FileInfo>();
            var item = TotalFilesList.Where(x => x.Key.Equals(id)).FirstOrDefault();
            if (item != null)
            {
                values = item.Values;
            }
            return values;
        }

        public void AddExtesion(int fileId, string extesion)
        {
            FileExtensions.Add(fileId, extesion);
        }

        public string GetExtension(int fileId)
        {
            string extension = "";
            FileExtensions?.TryGetValue(fileId, out extension);
            return extension ?? ".pdf";
        }

        public Dictionary<int, string> GetExtensionList()
        {
            return FileExtensions;
        }

        public void Clear()
        {
            FileExtensions.Clear();
        }
    }
}
