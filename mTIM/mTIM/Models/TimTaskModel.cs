using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using mTIM.Enums;
using Newtonsoft.Json;
using SkiaSharp;
using Xamarin.Forms;

namespace mTIM.Models.D
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class TimTaskModel : INotifyPropertyChanged
    {
        public string Action { get; set; }
        public string Color { get; set; }
        public object EvaluationType { get; set; }
        public string ExternId { get; set; }
        public int ForSubLevel { get; set; }
        public bool ForSubLevelSpecified { get; set; }
        public bool HasGPS { get; set; }
        public bool HasGPSSpecified { get; set; }
        public int Id { get; set; }
        public bool IdSpecified { get; set; }
        public int Level { get; set; }
        public bool LevelSpecified { get; set; }
        public string Name { get; set; }
        public string ObjectId { get; set; }
        public int Parent { get; set; }
        public bool ParentSpecified { get; set; }
        public string Path { get; set; }
        public string Range { get; set; }
        public bool ShowInList { get; set; }
        public bool ShowInListSpecified { get; set; }
        public int SortNr { get; set; }
        public bool SortNrSpecified { get; set; }
        public bool SplitGraphic { get; set; }
        public bool SplitGraphicSpecified { get; set; }
        public DataType Type { get; set; }

        [JsonIgnore]
        public bool ShowInLineList { get; set; }

        [JsonIgnore]
        public List<string> Conditions { get; set; } = new List<string>();

        [JsonIgnore]
        public string StatusColor { get; set; }

        [JsonIgnore]
        public int StatusIndex { get; set; }

        private object value;
        public object Value
        {
            get { return value; }
            set
            {
                this.value = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public IList<Value> Values { get; set; } = new List<Value>();

        [JsonIgnore]
        public IEnumerable<TimTaskModel> Childrens { get; set; }

        [JsonIgnore]
        public IList<string> PostiveActions { get; set; } = new List<string>();

        [JsonIgnore]
        public IList<string> NegativeActions { get; set; } = new List<string>();

        //[JsonIgnore]
        //public IEnumerable<TimTaskModel> Ancestors { get; set; }

        [JsonIgnore]
        public int ProjectId { get; set; }

        // Make base class for this logic, something like BindableBase
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [JsonIgnore]
        public bool HasChilds => Childrens?.Count() > 0;

        [JsonIgnore]
        private bool _isSelected;
        [JsonIgnore]
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public void LoadValues()
        {
            UpdateActions();
            switch (Type)
            {
                case DataType.Int:
                    if (!string.IsNullOrEmpty(Range))
                    {
                        string v = Range;
                        int step = 1;
                        int indexStep = v.IndexOf(':');
                        if (indexStep == -1)
                            indexStep = v.IndexOf('|');
                        if (indexStep != -1)
                        {
                            string stepString = v.Substring(indexStep + 1);
                            v = v.Substring(0, indexStep);
                            int.TryParse(stepString, out step);
                        }
                        int indexTo = v.IndexOf('-');
                        if (indexTo != -1)
                        {
                            string fromString = v.Substring(0, indexTo);
                            string toString = v.Substring(indexTo + 1);
                            int valueFrom = 0;
                            int.TryParse(fromString, out valueFrom);

                            int valueTo = valueFrom;
                            int.TryParse(toString, out valueTo);

                            for (int i = valueFrom; i <= valueTo; i += step)
                                Values.Add(new Value() { Data = i });
                        }
                        if (Range.Contains(","))
                        {
                            var result = Range.Split(',');
                            if (result.Length > 0)
                            {
                                foreach (var value in result)
                                {
                                    Values.Add(new Value() { Data = value });
                                }
                            }
                        }
                        if (Value != null)
                        {
                            var item = Values.Where(x => x.Data.Equals(Value)).FirstOrDefault();
                            if (item != null)
                            {
                                item.BackgroundColor = Xamarin.Forms.Color.LightGray;
                            }
                        }
                    }
                    break;
                case DataType.Float:
                    if (!string.IsNullOrEmpty(Range))
                    {
                        string v = Range;
                        float step = 1;
                        int indexStep = v.IndexOf(':');
                        if (indexStep == -1)
                            indexStep = v.IndexOf('|');
                        if (indexStep != -1)
                        {
                            string stepString = v.Substring(indexStep + 1);
                            v = v.Substring(0, indexStep);
                            float.TryParse(stepString, out step);
                        }
                        int indexTo = v.IndexOf('-');
                        if (indexTo != -1)
                        {
                            string fromString = v.Substring(0, indexTo);
                            string toString = v.Substring(indexTo + 1);
                            float valueFrom = 0;
                            float.TryParse(fromString, out valueFrom);

                            float valueTo = valueFrom;
                            float.TryParse(toString, out valueTo);

                            for (float i = valueFrom; i <= valueTo; i += step)
                                Values.Add(new Value() { Data = Convert.ToDecimal(i).ToString("0.00") });
                        }
                        if (Range.Contains(","))
                        {
                            var result = Range.Split(',');
                            if (result.Length > 0)
                            {
                                foreach (var value in result)
                                {
                                    Values.Add(new Value() { Data = Convert.ToDecimal(value).ToString("0.00") });
                                }
                            }
                        }
                        UpdateValue(Value);
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// To update the actions.
        /// </summary>
        private void UpdateActions()
        {
            if (!string.IsNullOrEmpty(Action))
            {
                var actions = Action.ToString().Split("/");
                if (actions?.Count() > 1)
                {
                    PostiveActions.Add(actions[0]);
                    NegativeActions.Add(actions[1]);
                }
                else
                {
                    PostiveActions.Add(actions.FirstOrDefault());
                }
            }
        }

        /// <summary>
        /// To update the selected value
        /// </summary>
        /// <param name="value"></param>
        public void UpdateValue(object value)
        {
            if (value != null)
            {
                Values?.ToList().ForEach(x => x.BackgroundColor = Xamarin.Forms.Color.White);
                var item = Values?.Where(x => x.Data.Equals(value)).FirstOrDefault();
                if (item != null)
                {
                    item.BackgroundColor = Xamarin.Forms.Color.LightGray;
                }
                Value = value;
            }
        }

        public void RecalcStatus(StatusConfiguration configuration)
        {
            int newStatusIndex = configuration.CalcStatusIndex(Conditions);
            if (newStatusIndex < 0 || newStatusIndex >= configuration.States.Count())
            {
                StatusColor = Xamarin.Forms.Color.Black.ToHex();
                StatusIndex = -1;
            }
            else
            {
                StatusColor = configuration.States[newStatusIndex].Color;
                StatusIndex = newStatusIndex;
            }
        }  

    }
}
