using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Age_Of_Nothing.Events;
using Age_Of_Nothing.Sprites;

namespace Age_Of_Nothing.SpritesUi
{
    /// <summary>
    /// Logique d'interaction pour ResourceUi.xaml
    /// </summary>
    public partial class ResourceUi : BaseSpriteUi<Resource>
    {
        private const int IndexZ = 1;
        private const double FocusStroke = 2;
        private const double SpaceBetween = 2;

        private static double StrokeAndSpace => FocusStroke + SpaceBetween;
        private static double TotalStrokeSize => StrokeAndSpace * 2;

        private static IReadOnlyDictionary<(Type, bool), Brush> _brushes = new Dictionary<(Type, bool), Brush>
        {
            { (typeof(GoldMine), false), GetImageFill(Brushes.Gold, "mine") },
            { (typeof(GoldMine), true), GetImageFill(Brushes.LightGoldenrodYellow, "mine") },
            { (typeof(RockMine), false), GetImageFill(Brushes.Silver, "mine") },
            { (typeof(RockMine), true), GetImageFill(Brushes.Gainsboro, "mine") },
            { (typeof(Forest), false), GetImageFill(Brushes.Green, "forest") },
            { (typeof(Forest), true), GetImageFill(Brushes.ForestGreen, "forest") }
        };

        private readonly Ellipse _surround;
        private readonly Ellipse _visual;

        public ResourceUi(Resource structure)
            : base(structure)
        {
            InitializeComponent();

            SetValue(Panel.ZIndexProperty, IndexZ);

            _visual = new Ellipse
            {
                Width = Sprite.Surface.Width,
                Height = Sprite.Surface.Height,
                Fill = _brushes[(Sprite.GetType(), false)]
            };
            MainCanvas.Children.Add(_visual);

            _surround = new Ellipse
            {
                Stroke = Brushes.Black,
                StrokeThickness = FocusStroke,
                Width = Sprite.Surface.Width + TotalStrokeSize,
                Height = Sprite.Surface.Height + TotalStrokeSize,
                Fill = Brushes.Transparent
            };

            // do not move this line above the _visual definition
            SetControlDimensionsAndPosition();

            MouseEnter += (a, b) => _visual.Fill = _brushes[(Sprite.GetType(), true)];
            MouseLeave += (a, b) => _visual.Fill = _brushes[(Sprite.GetType(), false)];
            MouseLeftButtonDown += (a, b) => Sprite.ToggleFocus();

            Sprite.PropertyChanged += (s, e) =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e is SpriteFocusChangedEventArgs)
                    {
                        SetControlDimensionsAndPosition();
                        if (Sprite.Focused)
                            MainCanvas.Children.Add(_surround);
                        else
                            MainCanvas.Children.Remove(_surround);
                    }
                }));
            };
        }

        private void SetControlDimensionsAndPosition()
        {
            MainCanvas.Width = Sprite.Surface.Width + (Sprite.Focused ? TotalStrokeSize : 0);
            MainCanvas.Height = Sprite.Surface.Height + (Sprite.Focused ? TotalStrokeSize : 0);
            SetValue(Canvas.LeftProperty, Sprite.Surface.Left - (Sprite.Focused ? StrokeAndSpace : 0));
            SetValue(Canvas.TopProperty, Sprite.Surface.Top - (Sprite.Focused ? StrokeAndSpace : 0));
            _visual.SetValue(Canvas.LeftProperty, Sprite.Focused ? StrokeAndSpace : double.NaN);
            _visual.SetValue(Canvas.TopProperty, Sprite.Focused ? StrokeAndSpace : double.NaN);
        }
    }
}
