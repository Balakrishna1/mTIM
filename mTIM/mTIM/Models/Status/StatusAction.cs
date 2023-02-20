using Newtonsoft.Json;

namespace mTIM.Models
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class StatusAction
    {
        public string Name { get; set; }

        public string[] NegativePreconditions { get; set; }

        public string[] PositivePreconditions { get; set; }

        public string[] ResetConditions { get; set; }

        public string[] SetConditions { get; set; }
    }
}
