using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace mTIM
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BottomUpdateAppInfoContentView
    {
        public BottomUpdateAppInfoContentView()
        {
            InitializeComponent();
        }

        public void InvokeView(double height, double width)
        {
            StackOrientation stackOrientation;
            double cellWidth = 0;
            if (height > width)
            {
                stackOrientation = StackOrientation.Vertical;
                cellWidth = width;
                option2.Margin = new Thickness(0, -5, 0, 0);
            }
            else
            {
                stackOrientation = StackOrientation.Horizontal;
                cellWidth = width / 2;
                option2.Margin = new Thickness(-5, -5, 0, -5);
            }

            stackMenuOptions1.Orientation = stackOrientation;
            option1.WidthRequest = cellWidth;
            option2.WidthRequest = cellWidth;
        }
    }
}
