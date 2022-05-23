using System;
using mTIM.Enums;
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

        public static Action<int> ActionRightIconClicked;
        public static Action<int> ActionValueClicked;
        public static Action<int> ActionItemClicked;

        public static readonly BindableProperty IdProperty =
            BindableProperty.Create("ID", typeof(int), typeof(ElementViewCell), 0);
        public static readonly BindableProperty NameProperty =
            BindableProperty.Create("Name", typeof(string), typeof(ElementViewCell), "Name");
        public static readonly BindableProperty TypeProperty =
            BindableProperty.Create("Type", typeof(DataType), typeof(ElementViewCell), DataType.None);
        public static readonly BindableProperty ColorProperty =
            BindableProperty.Create("Color", typeof(string), typeof(ElementViewCell), "Color");
        public static readonly BindableProperty LevelProperty =
            BindableProperty.Create("Level", typeof(string), typeof(ElementViewCell), "Level");
        public static readonly BindableProperty ValueProperty =
            BindableProperty.Create("Value", typeof(string), typeof(ElementViewCell), "Value");
        public static readonly BindableProperty HasChildsProperty =
            BindableProperty.Create("HasChilds", typeof(bool), typeof(ElementViewCell), false, BindingMode.TwoWay);
        public static readonly BindableProperty IsSelectedProperty =
            BindableProperty.Create("IsSelected", typeof(bool), typeof(ElementViewCell), false, BindingMode.TwoWay);

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

        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public bool HasChilds
        {
            get { return (bool)GetValue(HasChildsProperty); }
            set { SetValue(HasChildsProperty, value); }
        }

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            var maxLength = GlobalConstants.IsLandscape ? 12 : 40;
            if (Name.Length > maxLength)
            {
                lblName.FontSize = lblName.FontSize - 4;
            }
            lblName.Text = Name;
            rootView.BackgroundColor = IsSelected ? Xamarin.Forms.Color.LightGray : Xamarin.Forms.Color.Transparent;
            if (HasChilds)
            {
                absContent.IsVisible = false;
                imgInfoButton.Source = ImageSource.FromFile("icon_forword.png");
                imgInfoButton.Clicked -= ImgInfoButton_Clicked;
                imgInfoButton.Clicked += ImgInfoButton_Clicked;
            }
            else
            {
                switch (Type)
                {
                    case DataType.Int:
                    case DataType.String:
                    case DataType.Float:
                        stackInfo.IsVisible = false;
                        lblValue.Text = Value;
                        stackValue.IsVisible = true;
                        break;
                    case DataType.Bool:
                        stackInfo.IsVisible = false;
                        chbValue.Source = ImageSource.FromFile(Convert.ToBoolean(Value) ? "icon_checked" : "icon_unchecked");
                        stackCheckBox.IsVisible = true;
                        break;
                    case DataType.Doc:
                        stackInfo.IsVisible = false;
                        int count = FileInfoHelper.Instance.GetCount(ID);
                        lblDocValue.Text = count <= 0 ? string.Empty : count.ToString() + (count == 1 ? "Document" : "Documemts");
                        stackDocument.IsVisible = true;
                        break;
                    case DataType.Prjladen:
                    case DataType.Prjladen2:
                        stackInfo.IsVisible = true;
                        absContent.IsVisible = false;
                        imgInfoButton.IsEnabled = true;
                        imgInfoButton.Source = ImageSource.FromFile("icon_download.png");
                        imgInfoButton.Clicked -= ImgInfoButton_Clicked;
                        imgInfoButton.Clicked += ImgInfoButton_Clicked;
                        break;
                    case DataType.Aktion:
                    case DataType.Aktion2:
                        stackInfo.IsVisible = true;
                        absContent.IsVisible = false;
                        lblTime.Text = Value;
                        imgInfoButton.IsEnabled = true;
                        imgInfoButton.Source = ImageSource.FromFile("icon_edit.png");
                        imgInfoButton.Clicked -= ImgInfoButton_Clicked;
                        imgInfoButton.Clicked += ImgInfoButton_Clicked;
                        break;
                    case DataType.None:
                    case DataType.Referenz:
                    case DataType.Count:
                    default:
                        rootView.IsEnabled = false;
                        rootView.BackgroundColor = Xamarin.Forms.Color.GhostWhite;
                        stackInfo.IsVisible = true;
                        absContent.IsVisible = false;
                        imgInfoButton.IsEnabled = false;
                        imgInfoButton.Source = ImageSource.FromFile("icon_gray_info.png");
                        break;
                }
            }
        }

        async void OnTapped(object sender, EventArgs e)
        {
            await TouchHelper.Instance.TouchEffectsWithActionStruct<int>(stackValue, 0.9, 100, ID, ActionValueClicked);
        }

        private async void ImgInfoButton_Clicked(object sender, EventArgs e)
        {
            await TouchHelper.Instance.TouchEffectsWithActionStruct(imgInfoButton, 0.9, 100, ID, ActionRightIconClicked);
        }

        async void OnCheckBoxTapped(object sender, EventArgs e)
        {
            chbValue.Source = ImageSource.FromFile(!Convert.ToBoolean(Value) ? "icon_checked" : "icon_unchecked");
            await TouchHelper.Instance.TouchEffectsWithActionStruct(chbValue, 0.9, 100, ID, ActionValueClicked);
        }

        async void OnDocumentTapped(object sender, EventArgs e)
        {
            await TouchHelper.Instance.TouchEffectsWithActionStruct(stackDocument, 0.9, 100, ID, ActionValueClicked);
        }

        async void OnItemTapped(object sender, EventArgs e)
        {
            if (!GlobalConstants.IsLandscape && HasChilds)
            {
                await TouchHelper.Instance.TouchEffectsWithActionStruct(rootView, 0.95, 100, ID, ActionItemClicked);
            }
            else
            {
                ActionItemClicked?.Invoke(ID);
            }
        }
    }
}
