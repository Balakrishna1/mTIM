using System;
using mTIM.Helpers;
using Xamarin.Forms;

namespace mTIM
{
    public partial class ElementViewCell : ViewCell
    {
        public ElementViewCell()
        {
            InitializeComponent();
        }

        public static Action<int> ActionArrowClicked;
        public static Action<int> ActionValueClicked;

        public static readonly BindableProperty IdProperty =
            BindableProperty.Create("ID", typeof(int), typeof(ElementViewCell), 0);
        public static readonly BindableProperty NameProperty =
            BindableProperty.Create("Name", typeof(string), typeof(ElementViewCell), "Name");
        public static readonly BindableProperty TypeProperty =
            BindableProperty.Create("Type", typeof(object), typeof(ElementViewCell), "Type");
        public static readonly BindableProperty ColorProperty =
            BindableProperty.Create("Color", typeof(string), typeof(ElementViewCell), "Color");
        public static readonly BindableProperty LevelProperty =
            BindableProperty.Create("Level", typeof(string), typeof(ElementViewCell), "Level");
        public static readonly BindableProperty ValueProperty =
            BindableProperty.Create("Value", typeof(string), typeof(ElementViewCell), "Value");
        public static readonly BindableProperty HasChaildsProperty =
            BindableProperty.Create("HasChailds", typeof(bool), typeof(ElementViewCell), false);

        public int ID
        {
            get { return (int)GetValue(IdProperty); }
            set { SetValue(IdProperty, value); }
        }

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

        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public bool HasChailds
        {
            get { return (bool)GetValue(HasChaildsProperty); }
            set { SetValue(HasChaildsProperty, value); }
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            lblName.Text = Name;
            if (HasChailds)
            {
                absContent.IsVisible = false;
                imgInfoButton.Source = ImageSource.FromFile("icon_forword.png");
                imgInfoButton.Clicked -= ImgInfoButton_Clicked;
                imgInfoButton.Clicked += ImgInfoButton_Clicked;
            }
            else
            {
                if (Type != null)
                {
                    switch (Type)
                    {
                        case "int":
                        case "string":
                        case "float":
                            imgInfoButton.IsVisible = false;
                            lblValue.Text = Value;
                            stackValue.IsVisible = true;
                            break;
                        case "bool":
                            imgInfoButton.IsVisible = false;
                            chbValue.Source = ImageSource.FromFile(Convert.ToBoolean(Value) ? "icon_checked" : "icon_unchecked");
                            stackCheckBox.IsVisible = true;
                            break;
                        case "Doc":
                            imgInfoButton.IsVisible = false;
                            lblDocValue.Text = FileInfoHelper.Instance.GetCount(ID).ToString();
                            stackDocument.IsVisible = true;
                            break;
                        default:
                            rootView.BackgroundColor = Xamarin.Forms.Color.GhostWhite;
                            imgInfoButton.IsVisible = true;
                            absContent.IsVisible = false;
                            imgInfoButton.Source = ImageSource.FromFile("icon_gray_info.png");
                            break;
                    }
                }
                else
                {
                    absContent.IsVisible = false;
                    imgInfoButton.Source = ImageSource.FromFile("icon_gray_info.png");
                }
            }
        }

        void OnTapped(object sender, EventArgs e)
        {
            ActionValueClicked?.Invoke(ID);
        }

        private void ImgInfoButton_Clicked(object sender, EventArgs e)
        {
            ActionArrowClicked?.Invoke(ID);
        }

        void OnCheckBoxTapped(object sender, EventArgs e)
        {
            chbValue.Source = ImageSource.FromFile(!Convert.ToBoolean(Value) ? "icon_checked" : "icon_unchecked");
            ActionValueClicked?.Invoke(ID);
        }

        void OnDocumentTapped(object sender, EventArgs e)
        {
            ActionValueClicked?.Invoke(ID);
        }
    }
}
