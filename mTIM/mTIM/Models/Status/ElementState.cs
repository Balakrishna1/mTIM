using Newtonsoft.Json;

namespace mTIM.Models
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ElementState
    {
        public string[] Conditions { get; set; }

        public string ObjectID { get; set; }
    }
}
