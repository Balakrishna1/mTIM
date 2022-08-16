using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace mTIM.Models
{
    public class FileInfoModel
    {
        [JsonProperty(PropertyName = "Key")]
        public int Key { get; set; }
        [JsonProperty(PropertyName = "Value")]
        public List<FileInfo> Values { get; set; } = new List<FileInfo>();
    }

    public class FileInfo
    {
        public FileInfo()
        {
        }
        public FileInfo(string comment, int fileId)
        {
            if(fileId < 0 && fileId.ToString().Contains("-"))
            {
                fileId = Convert.ToInt32(fileId.ToString().Replace("-", ""));
            }
            FileId = fileId;
            Comment = comment;
        }
        public string Comment { get; set; }
        public int FileId { get; set; }
        public bool FileIDSpecified { get; set; }
        public bool IsOffline { get; set; }
        public bool IsCommentEdited { get; set; }
        public bool IsDeleted { get; set; }
        public string OfflineFilePath { get; set; }
        public bool IsShowDeleteIcon { get; set; }
        public Color TextColor  => (IsOffline || IsCommentEdited) ? Color.Red : Color.Black;
    }
}
