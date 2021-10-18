using System;
using System.Collections.Generic;

namespace mTIM.Models
{
    public class Model
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string Image { get; set; }
        public List<Model> LstModels { get; set; }
    }
}
