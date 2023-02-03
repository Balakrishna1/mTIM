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
        public List<FileInfo> Values { get; set; }
    }

    public class FileInfo
    {
        public string Comment { get; set; }
        public int FileID { get; set; }
        public bool FileIDSpecified { get; set; }
        public bool IsOffline { get; set; }
        public bool IsCommentEdited { get; set; }
        public bool IsDeleted { get; set; }
        public string OfflineFilePath { get; set; }
        public bool ShouldBeDelete { get; set; }
        public Color TextColor  => (IsOffline || IsCommentEdited) ? Color.Red : Color.Black;
    }
}
