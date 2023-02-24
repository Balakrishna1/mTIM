﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace mTIM.Models
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class StateInformation
    {
        public StatusConfiguration Configuration { get; set; }
        public CurrentElementsState State { get; set; }
    }
}