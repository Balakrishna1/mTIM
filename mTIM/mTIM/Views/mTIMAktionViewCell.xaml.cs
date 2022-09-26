using System;
using mTIM.Helpers;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace mTIM.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class mTIMAktionViewCell : TimBaseViewCell
    {
        public mTIMAktionViewCell()
        {
            InitializeComponent();
        }

        public static readonly BindableProperty ValueProperty =
           BindableProperty.Create("Value", typeof(string), typeof(mTIMValueViewCell), "Value", propertyChanged: HandleValueChangesPropertyChanged);
        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private static void HandleValueChangesPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            mTIMAktionViewCell targetView;
            targetView = (mTIMAktionViewCell)bindable;
            if (targetView != null)
                targetView.UpdateValue((string)newValue);
        }

        private void UpdateValue(string Value)
        {
            lblTime.Text = Value;
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
            lblTime.Text = Value;
            imgInfoButton.Source = ImageSource.FromFile("icon_edit.png");
            imgInfoButton.Clicked -= ImgInfoButton_Clicked;
            imgInfoButton.Clicked += ImgInfoButton_Clicked;
        }

        private async void ImgInfoButton_Clicked(object sender, EventArgs e)
        {
            await TouchHelper.Instance.TouchEffectsWithActionStruct(imgInfoButton, 0.9, 100, ID, ActionRightIconClicked);
        }
    }
}
