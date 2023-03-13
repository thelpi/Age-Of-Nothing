using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Age_Of_Nothing.Events;
using Age_Of_Nothing.Sprites;

namespace Age_Of_Nothing.SpritesUi
{
    /// <summary>
    /// Logique d'interaction pour VillagerUi.xaml
    /// </summary>
    public partial class VillagerUi : UserControl
    {
        private const int IndexZ = 2;
        private const double FocusStroke = 1;
        private const double SpaceBetween = 1;

        private static double StrokeAndSpace => FocusStroke + SpaceBetween;
        private static double TotalStrokeSize => StrokeAndSpace * 2;

        private readonly Shape _surround;
        private readonly Shape _visual;
        private readonly Villager _villager;

        public VillagerUi(Villager villager)
        {
            InitializeComponent();

            _villager = villager;

            SetValue(Panel.ZIndexProperty, IndexZ);

            _visual = new Ellipse
            {
                Width = _villager.Surface.Width,
                Height = _villager.Surface.Height,
                Fill = Brushes.SandyBrown
            };
            MainCanvas.Children.Add(_visual);

            _surround = new Ellipse
            {
                Stroke = Brushes.Black,
                StrokeThickness = FocusStroke,
                Width = _villager.Surface.Width + TotalStrokeSize,
                Height = _villager.Surface.Height + TotalStrokeSize,
                Fill = Brushes.Transparent
            };
            
            // do not move this line above the _visual definition
            SetControlDimensionsAndPosition();

            MouseEnter += (a, b) => _visual.Fill = Brushes.PeachPuff;
            MouseLeave += (a, b) => _visual.Fill = Brushes.SandyBrown;
            MouseLeftButtonDown += (a, b) => _villager.ToggleFocus();

            _villager.PropertyChanged += (s, e) =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e is SpriteFocusChangedEventArgs)
                    {
                        SetControlDimensionsAndPosition();
                        if (_villager.Focused)
                            MainCanvas.Children.Add(_surround);
                        else
                            MainCanvas.Children.Remove(_surround);
                    }
                    else if (e is SpritePositionChangedEventArgs)
                        SetControlDimensionsAndPosition();
                    else if (e.PropertyName == "ResourcesChanged")
                    {
                        var carry = _villager.IsCarrying();
                        if (carry == PrimaryResources.Gold)
                        {
                            _visual.Fill = new ImageBrush(new BitmapImage(new Uri(@"Resources/Images/gold.png", UriKind.Relative)));
                        }
                        else if (carry == PrimaryResources.Wood)
                        {
                            _visual.Fill = new ImageBrush(new BitmapImage(new Uri(@"Resources/Images/wood.png", UriKind.Relative)));
                        }
                        else if (carry == PrimaryResources.Rock)
                        {
                            _visual.Fill = new ImageBrush(new BitmapImage(new Uri(@"Resources/Images/rock.png", UriKind.Relative)));
                        }
                        else
                        {
                            _visual.Fill =  Brushes.SandyBrown;
                        }
                    }
                }));
            };

            Tag = _villager;
        }

        private void SetControlDimensionsAndPosition()
        {
            MainCanvas.Width = _villager.Surface.Width + (_villager.Focused ? TotalStrokeSize : 0);
            MainCanvas.Height = _villager.Surface.Height + (_villager.Focused ? TotalStrokeSize : 0);
            SetValue(Canvas.LeftProperty, _villager.Surface.Left - (_villager.Focused ? StrokeAndSpace : 0));
            SetValue(Canvas.TopProperty, _villager.Surface.Top - (_villager.Focused ? StrokeAndSpace : 0));
            _visual.SetValue(Canvas.LeftProperty, _villager.Focused ? StrokeAndSpace : double.NaN);
            _visual.SetValue(Canvas.TopProperty, _villager.Focused ? StrokeAndSpace : double.NaN);
        }
    }
}
