using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mTIM.Helpers;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace mTIM.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class mTimPrjladenViewCell : TimBaseViewCell
    {
        public mTimPrjladenViewCell()
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
            imgInfoButton.Source = ImageSource.FromFile("icon_download.png");
            imgInfoButton.Clicked -= ImgInfoButton_Clicked;
            imgInfoButton.Clicked += ImgInfoButton_Clicked;
        }
        private async void ImgInfoButton_Clicked(object sender, EventArgs e)
        {
            await TouchHelper.Instance.TouchEffectsWithActionStruct(imgInfoButton, 0.9, 100, ID, ActionRightIconClicked);
        }
    }
}
