using System;
using Newtonsoft.Json;

namespace mTIM.Models
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class TimTaskModel
    {
        public string Level { get; set; }
        public string Parent { get; set; }
        public string Id { get; set; }
        public string Path { get; set; }
        public string ObjectId { get; set; }
        public string Name { get; set; }
        public string ForSubLevel { get; set; }
        public string Type { get; set; }
        public string ShowInList { get; set; }
        public string Range { get; set; }
        public string Color { get; set; }
        public string HasGPS { get; set; }
        public string SortNr { get; set; }
        public string EvaluationType { get; set; }
        public string Value { get; set; }
        public string SplitGraphic { get; set; }
        public string Action { get; set; }
        [JsonProperty("ExternId")]
        public string ExternId { get; set; }
    }
}
