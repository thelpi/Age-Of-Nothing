using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Age_Of_Nothing.Events;
using Age_Of_Nothing.Sprites;

namespace Age_Of_Nothing.SpritesUi
{
    /// <summary>
    /// Logique d'interaction pour MarketUi.xaml
    /// </summary>
    public partial class MarketUi : BaseSpriteUi<Market>
    {
        private const int IndexZ = 1;
        private const double FocusStroke = 1;
        private const double SpaceBetween = 1;

        private static double StrokeAndSpace => FocusStroke + SpaceBetween;
        private static double TotalStrokeSize => StrokeAndSpace * 2;

        private static readonly Brush _defaultBrush = Brushes.Purple;
        private static readonly Brush _defaultBrushHover = Brushes.MediumPurple;
        private readonly Brush _marketBrush = GetImageFill(_defaultBrush, "market");
        private readonly Brush _marketBrushHover = GetImageFill(_defaultBrushHover, "market");

        private readonly Rectangle _surround;
        private readonly Rectangle _visual;

        public MarketUi(Market market)
            : base(market)
        {
            InitializeComponent();

            SetValue(Panel.ZIndexProperty, IndexZ);

            _visual = new Rectangle
            {
                Width = Sprite.Surface.Width,
                Height = Sprite.Surface.Height,
                Fill = _marketBrush
            };
            MainCanvas.Children.Add(_visual);

            _surround = new Rectangle
            {
                Stroke = Brushes.Black,
                StrokeThickness = FocusStroke,
                Width = Sprite.Surface.Width + TotalStrokeSize,
                Height = Sprite.Surface.Height + TotalStrokeSize,
                Fill = Brushes.Transparent
            };

            // do not move this line above the _visual definition
            SetControlDimensionsAndPosition();

            MouseEnter += (a, b) => _visual.Fill = _marketBrushHover;
            MouseLeave += (a, b) => _visual.Fill = _marketBrush;
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
