using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace mTIM
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BottomMenuContentView
    {
        public BottomMenuContentView()
        {
            InitializeComponent();
            ScaleTo1x(iconBadge);
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
                option4.Margin = new Thickness(0, -5, 0, 0);
            }
            else
            {
                stackOrientation = StackOrientation.Horizontal;
                cellWidth = width / 2;
                option2.Margin = new Thickness(-5, -5, 0, -5);
                option4.Margin = new Thickness(-5, -5, 0, 0);
            }

            stackMenuOptions1.Orientation = stackOrientation;
            stackMenuOptions2.Orientation = stackOrientation;
            option1.WidthRequest = cellWidth;
            option2.WidthRequest = cellWidth;
            option3.WidthRequest = cellWidth;
            option4.WidthRequest = cellWidth;
        }

        public void ScaleTo1x(Image image)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                var a = new Animation();
                a.Add(0, 0.5, new Animation(v => image.Scale = v, 0.7, 1.2, Easing.CubicInOut));
                a.Add(0.5, 1, new Animation(v => image.Scale = v, 1.2, 0.7, Easing.CubicIn));
                a.Commit(this, "animation", length: 1000,
                    finished: (v, c) => image.Scale = 0.7, repeat: () => true);
            });
        }
    }
}
