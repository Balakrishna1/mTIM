using System;
using mTIM.Helpers;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace mTIM.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class mTIMBooleanViewCell : TimBaseViewCell
    {
        public static readonly BindableProperty ValueProperty =
            BindableProperty.Create("Value", typeof(string), typeof(mTIMValueViewCell), "Value", propertyChanged: HandleValueChangesPropertyChanged);
        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }


        private bool Check { get; set; } 

        private static void HandleValueChangesPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            mTIMBooleanViewCell targetView;
            targetView = (mTIMBooleanViewCell)bindable;
            if (targetView != null)
                targetView.UpdateValue((string)newValue);
        }

        private void UpdateValue(string Value)
        {
            try
            {
                if (Type == Enums.DataType.Bool)
                {
                    var result = (Value == "J" || Value == "N");
                    if (result)
                        Check = Value == "J" ? true : false;
                    else
                        Check = Convert.ToBoolean(Value);
                    chbValue.Source = ImageSource.FromFile(Check ? "icon_checked" : "icon_unchecked");
                }
            }
            catch (Exception ex)
            {

            }
        }

        public mTIMBooleanViewCell()
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
            //chbValue.Source = ImageSource.FromFile(Convert.ToBoolean(Value) ? "icon_checked" : "icon_unchecked");
        }

        async void OnCheckBoxTapped(object sender, EventArgs e)
        {
            chbValue.Source = ImageSource.FromFile(!Check ? "icon_checked" : "icon_unchecked");
            await TouchHelper.Instance.TouchEffectsWithActionStruct(chbValue, 0.9, 100, ID, ActionValueClicked);
        }
    }
}
