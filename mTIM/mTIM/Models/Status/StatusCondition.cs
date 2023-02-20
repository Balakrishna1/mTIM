using Newtonsoft.Json;

namespace mTIM.Models
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class StatusCondition
    {
        public string Name { get; set; }
    }
}
