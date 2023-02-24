using Newtonsoft.Json;

namespace mTIM.Models
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class CurrentElementsState
    {
        public ElementState[] Elements { get; set; }

        public string Timestamp { get; set;}
    }
}
