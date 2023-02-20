using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace mTIM.Models
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class StatusConfiguration
    {
        public StatusAction[] Actions { get; set; }

        public StatusCondition[] Conditions { get; set; }

        public Status[] States { get; set; }

        public string Timestamp { get; set; }

        public int CalcStatusIndex(List<string> conditions)
        {
            for (int i = 0; i < States.Count(); i++)
            {
                var status = States[i];
                if (status.TestExact(conditions))
                    return i;
            }
            for (int i = 0; i < States.Count(); i++)
            {
                var status = States[i];
                if (status.TestCompatible(conditions))
                    return i;
            }
            return -1;
        }

    }
}
