using System;
using System.Collections.Generic;
using mTIM.Models;
using Xamarin.Forms;

namespace mTIM
{
    public partial class ElementViewCell : ViewCell
    {
        public ElementViewCell()
        {
            InitializeComponent();
        }

        public static readonly BindableProperty NameProperty =
            BindableProperty.Create("Name", typeof(string), typeof(ElementViewCell), "Name");
        public static readonly BindableProperty TypeProperty =
            BindableProperty.Create("Type", typeof(object), typeof(ElementViewCell), "Type");
        public static readonly BindableProperty ColorProperty =
            BindableProperty.Create("Color", typeof(string), typeof(ElementViewCell), "Color");
        public static readonly BindableProperty LevelProperty =
            BindableProperty.Create("Level", typeof(string), typeof(ElementViewCell), "Level");

        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        public object Type
        {
            get { return (object)GetValue(TypeProperty); }
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

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            lblName.Text = Name;
            lblValue.Text = Type?.ToString();
            if (!string.IsNullOrEmpty(Level) && Level.Equals("1"))
            {
                imgInfoButton.Source = ImageSource.FromFile("icon_forword.png");
            }
            else
            {
                rootView.BackgroundColor = Xamarin.Forms.Color.GhostWhite;
                imgInfoButton.Source = ImageSource.FromFile("icon_gray_info.png");
            }
            imgInfoButton.Clicked -= ImgInfoButton_Clicked;
            imgInfoButton.Clicked += ImgInfoButton_Clicked;
        }

        void OnItemTapped(object sender, EventArgs e)
        {
            
        }

        private void ImgInfoButton_Clicked(object sender, EventArgs e)
        {

        }
    }
}
