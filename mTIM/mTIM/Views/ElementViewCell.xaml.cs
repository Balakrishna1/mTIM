using System;
using mTIM.Enums;
using mTIM.Helpers;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace mTIM
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    partial class ElementViewCell : ViewCell
    {
        public ElementViewCell()
        {
            InitializeComponent();
        }

        public static Action<int> ActionRightIconClicked;
        public static Action<int> ActionValueClicked;
        public static Action<int> ActionItemClicked;

        public static readonly BindableProperty IdProperty =
            BindableProperty.Create("Id", typeof(int), typeof(ElementViewCell), 0);
        public static readonly BindableProperty NameProperty =
            BindableProperty.Create("Name", typeof(string), typeof(ElementViewCell), "Name");
        public static readonly BindableProperty TypeProperty =
            BindableProperty.Create("Type", typeof(DataType), typeof(ElementViewCell), DataType.None);
        public static readonly BindableProperty ColorProperty =
            BindableProperty.Create("Color", typeof(string), typeof(ElementViewCell), "Color");
        public static readonly BindableProperty LevelProperty =
            BindableProperty.Create("Level", typeof(string), typeof(ElementViewCell), "Level");
        public static readonly BindableProperty ValueProperty =
            BindableProperty.Create("Value", typeof(string), typeof(ElementViewCell), "Value", propertyChanged: HandleValueChangesPropertyChanged);
        public static readonly BindableProperty HasChildsProperty =
            BindableProperty.Create("HasChilds", typeof(bool), typeof(ElementViewCell), false);
        public static readonly BindableProperty IsSelectedProperty =
            BindableProperty.Create("IsSelected", typeof(bool), typeof(ElementViewCell), false, BindingMode.TwoWay, propertyChanged: HandleSelectionChangesPropertyChanged);

        public int Id
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

        private static void HandleSelectionChangesPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                ElementViewCell targetView;
                targetView = (ElementViewCell)bindable;
                if (targetView != null)
                    targetView.rootView.BackgroundColor = ((bool)newValue) ? Xamarin.Forms.Color.LightGray : Xamarin.Forms.Color.White;
            });
        }

        private static void HandleHasChaildPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                ElementViewCell targetView;
                targetView = (ElementViewCell)bindable;
                if (targetView != null)
                   targetView.UpdateChaild((bool)newValue);
            });
        }

        private static void HandleValueChangesPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                ElementViewCell targetView;
                targetView = (ElementViewCell)bindable;
                if (targetView != null)
                    targetView.UpdateValue((string)newValue);
            });
        }

        private void UpdateChaild(bool hasChilds)
        {
            if (hasChilds)
            {
                imgInfoButton.Source = ImageSourceHelper.GetImageSource("icon_forword.png");
                imgInfoButton.Clicked -= ImgInfoButton_Clicked;
                imgInfoButton.Clicked += ImgInfoButton_Clicked;
                absContent.IsVisible = false;
            }
        }

        private void UpdateValue(string Value)
        {
                switch (Type)
                {
                    case DataType.Int:
                    case DataType.String:
                    case DataType.Float:
                        lblValue.Text = Value;
                        break;
                    case DataType.Bool:
                        chbValue.Source = ImageSourceHelper.GetImageSource(Convert.ToBoolean(Value) ? "icon_checked.png" : "icon_unchecked.png");
                        break;
                    case DataType.Aktion:
                    case DataType.Aktion2:
                    lblTime.IsVisible = true;
                    lblTime.Text = Value;
                        break;
                    default:
                        break;
                }
        }


        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            Device.BeginInvokeOnMainThread(() =>
            {
                var maxLength = GlobalConstants.IsLandscape ? 12 : 40;
                if (Name.Length > maxLength)
                {
                    lblName.FontSize = (int)lblName.FontSize - 4;
                }
                lblTime.IsVisible = false;
                lblName.Text = Name;
                rootView.BackgroundColor = IsSelected ? Xamarin.Forms.Color.LightGray : Xamarin.Forms.Color.Transparent;
                if (HasChilds)
                {
                    imgInfoButton.Source = ImageSourceHelper.GetImageSource("icon_forword.png");
                    imgInfoButton.Clicked -= ImgInfoButton_Clicked;
                    imgInfoButton.Clicked += ImgInfoButton_Clicked;
                    absContent.IsVisible = false;
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
                            chbValue.Source = ImageSourceHelper.GetImageSource(Convert.ToBoolean(Value) ? "icon_checked.png" : "icon_unchecked.png");
                            stackCheckBox.IsVisible = true;
                            break;
                        case DataType.Doc:
                            stackInfo.IsVisible = false;
                            int count = FileInfoHelper.Instance.GetCount(Id);
                            lblDocValue.Text = count <= 0 ? string.Empty : count.ToString() + (count == 1 ? "Document" : "Documemts");
                            stackDocument.IsVisible = true;
                            break;
                        case DataType.Prjladen:
                        case DataType.Prjladen2:
                            stackInfo.IsVisible = true;
                            absContent.IsVisible = false;
                            imgInfoButton.IsEnabled = true;
                            imgInfoButton.Source = ImageSourceHelper.GetImageSource("icon_download.png");
                            imgInfoButton.Clicked -= ImgInfoButton_Clicked;
                            imgInfoButton.Clicked += ImgInfoButton_Clicked;
                            break;
                        case DataType.Aktion:
                        case DataType.Aktion2:
                            stackInfo.IsVisible = true;
                            absContent.IsVisible = false;
                            lblTime.IsVisible = true;
                            lblTime.Text = Value;
                            imgInfoButton.IsEnabled = true;
                            imgInfoButton.Source = ImageSourceHelper.GetImageSource("icon_edit.png");
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
                            imgInfoButton.Source = ImageSourceHelper.GetImageSource("icon_gray_info.png");
                            break;
                    }
                }
            });
        }

        async void OnTapped(object sender, EventArgs e)
        {
            await TouchHelper.Instance.TouchEffectsWithActionStruct<int>(stackValue, 0.9, 100, Id, ActionValueClicked);
        }

        private async void ImgInfoButton_Clicked(object sender, EventArgs e)
        {
            await TouchHelper.Instance.TouchEffectsWithActionStruct(imgInfoButton, 0.9, 100, Id, ActionRightIconClicked);
        }

        async void OnCheckBoxTapped(object sender, EventArgs e)
        {
            chbValue.Source = ImageSourceHelper.GetImageSource(!Convert.ToBoolean(Value) ? "icon_checked.png" : "icon_unchecked.png");
            await TouchHelper.Instance.TouchEffectsWithActionStruct(chbValue, 0.9, 100, Id, ActionValueClicked);
        }

        async void OnDocumentTapped(object sender, EventArgs e)
        {
            await TouchHelper.Instance.TouchEffectsWithActionStruct(stackDocument, 0.9, 100, Id, ActionValueClicked);
        }

        async void OnItemTapped(object sender, EventArgs e)
        {
            if (!GlobalConstants.IsLandscape && HasChilds)
            {
                await TouchHelper.Instance.TouchEffectsWithActionStruct(rootView, 0.95, 100, Id, ActionItemClicked);
            }
            else
            {
                ActionItemClicked?.Invoke(Id);
            }
        }
    }
}
