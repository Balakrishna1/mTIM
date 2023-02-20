using System;
using mTIM.Helpers;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace mTIM.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class mTIMNormalViewCell : TimBaseViewCell
    {
        public static readonly BindableProperty IsSelectedProperty =
               BindableProperty.Create("IsSelected", typeof(bool), typeof(mTIMValueViewCell), false, BindingMode.TwoWay, propertyChanged: HandleSelectionChangesPropertyChanged);

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        private static void HandleSelectionChangesPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            mTIMNormalViewCell targetView;
            targetView = (mTIMNormalViewCell)bindable;
            if (targetView != null)
                targetView.rootView.BackgroundColor = ((bool)newValue) ? Xamarin.Forms.Color.LightGray : Xamarin.Forms.Color.Transparent;
        }

        public mTIMNormalViewCell()
        {
            InitializeComponent();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            var maxLength = GlobalConstants.IsLandscape ? 12 : 40;
            if (Name.Length > maxLength)
            {
                lblName.FontSize = (int)lblName.FontSize - 4;
            }
            lblName.Text = Name;
            rootView.BackgroundColor = IsSelected ? Xamarin.Forms.Color.LightGray : Xamarin.Forms.Color.White;
            if (HasChilds)
            {
                if(ShowInLineList)
                {
                    stackInfo.IsVisible = false;
                    foreach(var item in Childrens)
                    {
                        var checkbox = new mTIMCheckbox(item);
                        checkbox.Clicked -= Checkbox_Clicked;
                        checkbox.Clicked += Checkbox_Clicked;
                        stackInlineList.Children.Add(checkbox);
                    }
                }
                else
                {
                    imgInfoButton.Source = ImageSource.FromFile("icon_forword.png");
                    imgInfoButton.Clicked -= ImgInfoButton_Clicked;
                    imgInfoButton.Clicked += ImgInfoButton_Clicked;
                }
            }
            else
            {
                rootView.IsEnabled = false;
                rootView.BackgroundColor = Xamarin.Forms.Color.GhostWhite;
                stackInfo.IsVisible = true;
                imgInfoButton.IsEnabled = false;
                imgInfoButton.Source = ImageSource.FromFile("icon_gray_info.png");
            }
        }

        private void Checkbox_Clicked(int id, bool value)
        {
            ActionValueClicked?.Invoke(id);
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

        private async void ImgInfoButton_Clicked(object sender, EventArgs e)
        {
            await TouchHelper.Instance.TouchEffectsWithActionStruct(imgInfoButton, 0.9, 100, ID, ActionRightIconClicked);
        }
    }
}
