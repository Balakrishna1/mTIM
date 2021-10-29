using System;
using System.Collections.Generic;
using Newtonsoft.Json;

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
    }
}
