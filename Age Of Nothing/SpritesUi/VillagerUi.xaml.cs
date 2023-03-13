using System;
using System.Windows.Controls;
using System.Windows.Media;
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

        private readonly Ellipse _surround;
        private readonly Ellipse _visual;

        public VillagerUi(Villager villager)
        {
            InitializeComponent();
            MainCanvas.Width = villager.Surface.Width;
            MainCanvas.Height = villager.Surface.Height;
            SetValue(Canvas.LeftProperty, villager.Surface.Left);
            SetValue(Canvas.TopProperty, villager.Surface.Top);
            SetValue(Panel.ZIndexProperty, IndexZ);

            _visual = new Ellipse
            {
                Width = villager.Surface.Width,
                Height = villager.Surface.Height,
                Fill = Brushes.SandyBrown
            };
            _visual.SetValue(Canvas.LeftProperty, double.NaN);
            _visual.SetValue(Canvas.TopProperty, double.NaN);
            MainCanvas.Children.Add(_visual);

            MouseEnter += (a, b) => _visual.Fill = Brushes.PeachPuff;
            MouseLeave += (a, b) => _visual.Fill = Brushes.SandyBrown;
            MouseLeftButtonDown += (a, b) => villager.ToggleFocus();

            _surround = new Ellipse
            {
                Stroke = Brushes.Black,
                StrokeThickness = 2,
                Width = villager.Surface.Width + 4,
                Height = villager.Surface.Height + 4,
                Fill = Brushes.Transparent
            };
            _surround.SetValue(Panel.ZIndexProperty, IndexZ);

            villager.PropertyChanged += (s, e) =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {

                    if (e is SpriteFocusChangedEventArgs)
                    {
                        if (villager.Focused)
                        {
                            MainCanvas.Width = villager.Surface.Width + 4;
                            MainCanvas.Height = villager.Surface.Height + 4;
                            SetValue(Canvas.LeftProperty, villager.Surface.Left - 2);
                            SetValue(Canvas.TopProperty, villager.Surface.Top - 2);
                            _visual.SetValue(Canvas.LeftProperty, 2D);
                            _visual.SetValue(Canvas.TopProperty, 2D);
                            MainCanvas.Children.Add(_surround);
                        }
                        else
                        {
                            MainCanvas.Width = villager.Surface.Width;
                            MainCanvas.Height = villager.Surface.Height;
                            SetValue(Canvas.LeftProperty, villager.Surface.Left);
                            SetValue(Canvas.TopProperty, villager.Surface.Top);
                            _visual.SetValue(Canvas.LeftProperty, double.NaN);
                            _visual.SetValue(Canvas.TopProperty, double.NaN);
                            MainCanvas.Children.Remove(_surround);
                        }
                    }
                    else if (e is SpritePositionChangedEventArgs)
                    {
                        if (villager.Focused)
                        {
                            SetValue(Canvas.LeftProperty, villager.Surface.Left - 2);
                            SetValue(Canvas.TopProperty, villager.Surface.Top - 2);
                        }
                        else
                        {
                            SetValue(Canvas.LeftProperty, villager.Surface.Left);
                            SetValue(Canvas.TopProperty, villager.Surface.Top);
                        }
                    }
                }));
            };

            Tag = villager;
        }
    }
}
