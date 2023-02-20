using System;
using Xamarin.Forms;
using mTIM.Models.D;

namespace mTIM.Views
{
	public class mTIMCheckbox : ContentView
	{
        protected Grid ContentGrid;
        protected ContentView ContentContainer;
        public Label TextContainer;
        protected Image ImageContainer;
        public bool Checked { get; set; }
        public TimTaskModel Model { get; set; }

        public mTIMCheckbox ()
		{
            Intialize();
		}

        public mTIMCheckbox(TimTaskModel model)
        {
            Model = model;
            Intialize();
        }

        public void Intialize()
		{
            var TapGesture = new TapGestureRecognizer();
            TapGesture.Tapped += TapGestureOnTapped;
            GestureRecognizers.Add(TapGesture);

            ContentGrid = new Grid
            {
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.Fill
            };

            ContentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(35, GridUnitType.Absolute) });
            ContentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) });

            ImageContainer = new Image
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
            };
            ImageContainer.HeightRequest = 35;
            ImageContainer.WidthRequest = 35;

            ContentGrid.Children.Add(ImageContainer);
            Grid.SetRow(ImageContainer, 0);

            TextContainer = new Label
            {
                TextColor = Color.Black,
                VerticalOptions = LayoutOptions.EndAndExpand,
                HorizontalOptions = LayoutOptions.Center,
            };

            ContentGrid.Children.Add(TextContainer);
            Grid.SetRow(TextContainer, 1);
            ContentGrid.Padding = new Thickness(0, 0, 15, 0);
            base.Content = ContentGrid;

            if(Model.Value != null && Model.Type == Enums.DataType.Bool)
            {
                Checked = Model.Value.ToString() == "J" ? true : false;
            }
            TextContainer.Text = Model.Name;
            UpdateImageSource();
            this.BackgroundColor = Color.Transparent;
        }

        public Image Image
        {
            get { return ImageContainer; }
            set { ImageContainer = value; }
        }

        public event Action<int, bool> Clicked;
        private void TapGestureOnTapped(object sender, EventArgs eventArgs)
        {
            if (IsEnabled)
            {
                Checked = !Checked;
                UpdateImageSource();
                if (Clicked != null)
                    Clicked.Invoke(Model.Id, Checked);
            }
        }

        private void UpdateImageSource()
        {
            this.Image.Source = ImageSource.FromFile(Checked ? "icon_checked" : "icon_unchecked");
        }
    }
}


