using System.ComponentModel;
using mTIM.Enums;
using Newtonsoft.Json;

namespace mTIM.Models.D
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class TimTaskModel : INotifyPropertyChanged
    {
        public object Action { get; set; }
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
        public object Value { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(object sender, string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
            }
        }

        [JsonIgnore]
        public bool HasChilds { get; set; }

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
                    OnPropertyChanged(this, "IsSelected");
                }
            }
        }
    }
}
