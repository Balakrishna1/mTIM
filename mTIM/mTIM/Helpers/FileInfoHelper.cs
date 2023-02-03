using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using mTIM.Models;
using Newtonsoft.Json;

namespace mTIM.Helpers
{
    public class FileInfoHelper
    {
        readonly string tag = "FileInfoHelper";
        Dictionary<int, string> FileExtensions = new Dictionary<int, string>();

        public List<FileInfoModel> TotalFilesList = new List<FileInfoModel>(); 

        public Action<int, int, int, bool> FileUploadCompleted;
        public Action<int, int> CommentUpdatedCompleted;
        public Action<int, int> DeleteCompleted;

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
            var list = await GetList();
            TotalFilesList.Clear();
            if (list != null)
            {
                TotalFilesList.AddRange(list);
            }
        }

        private async Task<List<FileInfoModel>> GetList()
        {
            List<FileInfoModel> list = new List<FileInfoModel>();
            if (FileHelper.IsFileExists(GlobalConstants.FILES_INFO))
            {
                var json = await FileHelper.ReadTextAsync(GlobalConstants.FILES_INFO);
                list = JsonConvert.DeserializeObject<List<FileInfoModel>>(json);
            }
            return list;
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
                values = item.Values ?? new List<FileInfo>();
            }
            else
            {
                FileInfoModel model = new FileInfoModel();
                model.Key = id;
                model.Values = new List<FileInfo>();
                TotalFilesList.Add(model);
            }
            return values?.Where(x=>x.IsDeleted==false).ToList();
        }

        public async Task UpdateValueInList(int id, FileInfo value)
        {
            TotalFilesList.Where(x => x.Key.Equals(id)).FirstOrDefault()?.Values.Add(value);
            await SaveFiles();
        }

        public async Task UpdateFileInfoInList(int postId, int fileId, int uploadFileId, bool uploadFileSepecified)
        {
            var values = TotalFilesList.Where(x => x.Key.Equals(postId)).FirstOrDefault()?.Values;
            var item = values.Where(x => x.FileID.Equals(fileId)).FirstOrDefault();
            if (item != null)
            {
                item.FileID = uploadFileId;
                item.FileIDSpecified = uploadFileSepecified;
                item.IsOffline = false;
            }
            await SaveFiles();
        }

        public async Task UpdateFileInfo(int id, int index, FileInfo item)
        {
            var files = TotalFilesList.Where(x => x.Key.Equals(id)).FirstOrDefault()?.Values;
            if (files != null && files.Count > 0)
                files[index] = item;
            await SaveFiles();
        }

        public async Task UpdateFileComment(int postId, int fileId, string comment)
        {
            var file = TotalFilesList.Where(x => x.Key.Equals(postId)).FirstOrDefault().Values?.Where(x => x.FileID.Equals(fileId)).FirstOrDefault();
            file.IsCommentEdited = false;
            file.Comment = comment;
            await SaveFiles();
        }

        public async Task DeleteValueInList(int id, FileInfo item)
        {
            TotalFilesList.Where(x => x.Key.Equals(id)).FirstOrDefault()?.Values.Remove(item);
            await SaveFiles();
        }

        public async Task DeleteFileInList(int postId, int fileId)
        {
            var values = TotalFilesList.Where(x => x.Key.Equals(postId)).FirstOrDefault()?.Values;
            values.Remove(values.SingleOrDefault(y => y.FileID.Equals(fileId)));
            await SaveFiles();
        }

        private async Task SaveFiles()
        {
            try
            {
                var json = JsonConvert.SerializeObject(TotalFilesList);
                await FileHelper.WriteTextAsync(GlobalConstants.FILES_INFO, json);
            }
            catch (Exception ex)
            {
                CrashReportManager.ReportError(ex, System.Reflection.MethodBase.GetCurrentMethod().Name, tag);
            }
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
