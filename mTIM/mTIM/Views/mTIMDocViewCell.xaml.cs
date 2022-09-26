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
    public partial class mTIMDocViewCell : TimBaseViewCell
    {
        public mTIMDocViewCell()
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
            int count = FileInfoHelper.Instance.GetCount(ID);
            lblDocValue.Text = count <= 0 ? string.Empty : count.ToString() + (count == 1 ? "Document" : "Documemts");
        }

        async void OnDocumentTapped(object sender, EventArgs e)
        {
            await TouchHelper.Instance.TouchEffectsWithActionStruct(stackDocument, 0.9, 100, ID, ActionValueClicked);
        }
    }
}
