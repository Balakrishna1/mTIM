using System;
using mTIM.Enums;
using Xamarin.Forms;

namespace mTIM.Views
{
    public class TimBaseViewCell : ViewCell
    {
        public static Action<int> ActionRightIconClicked;
        public static Action<int> ActionValueClicked;
        public static Action<int> ActionItemClicked;

        public TimBaseViewCell()
        {
        }

        public static readonly BindableProperty IDProperty =
            BindableProperty.Create("ID", typeof(int), typeof(TimBaseViewCell), 0);
        public static readonly BindableProperty NameProperty =
            BindableProperty.Create("Name", typeof(string), typeof(TimBaseViewCell), "Name");
        public static readonly BindableProperty TypeProperty =
            BindableProperty.Create("Type", typeof(DataType), typeof(TimBaseViewCell), DataType.None);
        public static readonly BindableProperty ColorProperty =
            BindableProperty.Create("Color", typeof(string), typeof(TimBaseViewCell), "Color");
        public static readonly BindableProperty LevelProperty =
            BindableProperty.Create("Level", typeof(string), typeof(TimBaseViewCell), "Level");
        //public static readonly BindableProperty ValueProperty =
        //    BindableProperty.Create("Value", typeof(string), typeof(ElementViewCell), "Value", propertyChanged: HandleValueChangesPropertyChanged);
        public static readonly BindableProperty HasChildsProperty =
            BindableProperty.Create("HasChilds", typeof(bool), typeof(TimBaseViewCell), false, BindingMode.TwoWay);
        //public static readonly BindableProperty IsSelectedProperty =
        //    BindableProperty.Create("IsSelected", typeof(bool), typeof(ElementViewCell), false, BindingMode.TwoWay, propertyChanged: HandleSelectionChangesPropertyChanged);

        public int ID
        {
            get { return (int)GetValue(IDProperty); }
            set { SetValue(IDProperty, value); }
        }

        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        public DataType Type
        {
            get { return (DataType)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public string Color
        {
            get { return (string)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public string Level
        {
            get { return (string)GetValue(LevelProperty); }
            set { SetValue(LevelProperty, value); }
        }

        public bool HasChilds
        {
            get { return (bool)GetValue(HasChildsProperty); }
            set { SetValue(HasChildsProperty, value); }
        }
    }
}
