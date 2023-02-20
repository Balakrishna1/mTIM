using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace mTIM.Models
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Status
    {
        public string Color;

        public string[] Conditions;

        public string Name;

        public int Sequence;

        public bool SequenceSpecified;

        public bool TestExact(List<string> _otherConditions)
        {
            if (Conditions.Count() != _otherConditions.Count())
                return false;
            for (int i = 0; i < _otherConditions.Count(); i++)
            {
                var value = Conditions?.Contains(_otherConditions[i]);
                if (value == false)
                    return false;
            }
            return true;
        }

        public bool TestCompatible(List<string> _otherConditions)
        {
            if (Conditions.Count() != _otherConditions.Count())
                return false;
            for (int i = 0; i < Conditions.Count(); i++)
            {
                var value = _otherConditions?.Contains(_otherConditions[i]);
                if (value == false)
                    return false;
            }
            return true;
        }
    }
}
