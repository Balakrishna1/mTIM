using System;
using mTIM.Helpers;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace mTIM.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class mTIMValueViewCell : TimBaseViewCell
    {
        public static readonly BindableProperty ValueProperty =
            BindableProperty.Create("Value", typeof(string), typeof(mTIMValueViewCell), "Value", propertyChanged: HandleValueChangesPropertyChanged);
        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private static void HandleValueChangesPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            mTIMValueViewCell targetView;
            targetView = (mTIMValueViewCell)bindable;
            if (targetView != null)
                targetView.UpdateValue((string)newValue);
        }

        private void UpdateValue(string Value)
        {
            lblValue.Text = Value;
        }

        public mTIMValueViewCell()
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
            UpdateValue(Value);
        }

        async void OnTapped(object sender, EventArgs e)
        {
            await TouchHelper.Instance.TouchEffectsWithActionStruct<int>(stackValue, 0.9, 100, ID, ActionValueClicked);
        }
    }
}
