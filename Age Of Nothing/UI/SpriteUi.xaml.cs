using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Age_Of_Nothing.Sprites;
using Age_Of_Nothing.Sprites.Resources;
using Age_Of_Nothing.Sprites.Structures;
using Age_Of_Nothing.Sprites.Units;

namespace Age_Of_Nothing.UI
{
    /// <summary>
    /// Logique d'interaction pour SpriteUi.xaml
    /// </summary>
    public partial class SpriteUi : UserControl
    {
        private const int UnitIndexZ = 2;
        private const int DefaultIndexZ = 1;

        private const double FocusStroke = 2;
        private const double SpaceBetween = 2;
        private static readonly double StrokeAndSpace = FocusStroke + SpaceBetween;
        private static readonly double TotalStrokeSize = StrokeAndSpace * 2;

        private readonly Shape _surround;
        private readonly Shape _visual;

        public Sprite Sprite { get; }

        private static readonly IReadOnlyDictionary<(ResourceTypes, bool), Brush> _resourcebrushes = new Dictionary<(ResourceTypes, bool), Brush>
        {
            { (ResourceTypes.Gold, false), GetImageFill(Brushes.SandyBrown, "gold") },
            { (ResourceTypes.Gold, true), GetImageFill(Brushes.PeachPuff, "gold") },
            { (ResourceTypes.Wood, false), GetImageFill(Brushes.SandyBrown, "wood") },
            { (ResourceTypes.Wood, true), GetImageFill(Brushes.PeachPuff, "wood") },
            { (ResourceTypes.Rock, false), GetImageFill(Brushes.SandyBrown, "rock") },
            { (ResourceTypes.Rock, true), GetImageFill(Brushes.PeachPuff, "rock") },
        };

        private static readonly IReadOnlyDictionary<(Type, bool), Brush> _brushes = new Dictionary<(Type, bool), Brush>
        {
            { (typeof(Market), false), GetImageFill(Brushes.Purple, "market") },
            { (typeof(Market), true), GetImageFill(Brushes.MediumPurple, "market") },
            { (typeof(Dwelling), false), GetImageFill(Brushes.Sienna, "dwelling") },
            { (typeof(Dwelling), true), GetImageFill(Brushes.Peru, "dwelling") },
            { (typeof(Barracks), false), GetImageFill(Brushes.Crimson, "barracks") },
            { (typeof(Barracks), true), GetImageFill(Brushes.Salmon, "barracks") },
            { (typeof(Villager), false), Brushes.SandyBrown },
            { (typeof(Villager), true), Brushes.PeachPuff },
            { (typeof(Swordsman), false), GetImageFill(Brushes.Blue, "sword") },
            { (typeof(Swordsman), true), GetImageFill(Brushes.MediumSlateBlue, "sword") },
            { (typeof(Archer), false), GetImageFill(Brushes.Blue, "bow") },
            { (typeof(Archer), true), GetImageFill(Brushes.MediumSlateBlue, "bow") },
            { (typeof(Knight), false), GetImageFill(Brushes.Blue, "horse") },
            { (typeof(Knight), true), GetImageFill(Brushes.MediumSlateBlue, "horse") },
            { (typeof(GoldMine), false), GetImageFill(Brushes.Gold, "mine") },
            { (typeof(GoldMine), true), GetImageFill(Brushes.LightGoldenrodYellow, "mine") },
            { (typeof(RockMine), false), GetImageFill(Brushes.Silver, "mine") },
            { (typeof(RockMine), true), GetImageFill(Brushes.Gainsboro, "mine") },
            { (typeof(Forest), false), GetImageFill(Brushes.Green, "forest") },
            { (typeof(Forest), true), GetImageFill(Brushes.ForestGreen, "forest") }
        };

        public SpriteUi(Sprite sprite, bool isCraft)
        {
            InitializeComponent();

            Sprite = sprite;

            SetValue(Panel.ZIndexProperty, GetIndexZ());

            _visual = BuildShape();
            _visual.Width = Sprite.Surface.Width;
            _visual.Height = Sprite.Surface.Height;
            _visual.Fill = GetFill();
            _visual.Opacity = isCraft ? 0.5 : 1;
            MainCanvas.Children.Add(_visual);

            _surround = BuildShape();
            _surround.Stroke = Brushes.Black;
            _surround.StrokeThickness = FocusStroke;
            _surround.Width = Sprite.Surface.Width + TotalStrokeSize;
            _surround.Height = Sprite.Surface.Height + TotalStrokeSize;
            _surround.Fill = Brushes.Transparent;

            // do not move this line above the _visual definition
            SetControlDimensionsAndPosition();

            if (!isCraft)
            {
                MouseEnter += (a, b) => _visual.Fill = GetFill();
                MouseLeave += (a, b) => _visual.Fill = GetFill();
                MouseLeftButtonDown += (a, b) => Sprite.ToggleFocus();

                Sprite.PropertyChanged += (s, e) =>
                {
                    Action action = null;
                    if (e.PropertyName == nameof(Sprite.Focused))
                    {
                        action = () =>
                        {
                            SetControlDimensionsAndPosition();
                            if (Sprite.Focused)
                                MainCanvas.Children.Add(_surround);
                            else
                                MainCanvas.Children.Remove(_surround);
                        };
                    }
                    else if (e.PropertyName == nameof(Sprite.Center))
                    {
                        action = SetControlDimensionsAndPosition;
                    }
                    else if (e.PropertyName == nameof(Villager.Carry))
                    {
                        action = () => _visual.Fill = GetFill();
                    }
                    else if (e.PropertyName == Sprite.HoverPropertyName)
                    {
                        action = () => _visual.Fill = GetFill(true);
                    }
                    else if (e.PropertyName == Sprite.UnhoverPropertyName)
                    {
                        action = () => _visual.Fill = GetFill();
                    }

                    if (action != null)
                        Dispatcher.BeginInvoke(action);
                };
            }
        }

        private Brush GetFill(bool forceHover = false)
        {
            var hover = IsMouseOver || forceHover;
            return Sprite.Is<Villager>(out var villager) && villager.Carry.HasValue
                ? _resourcebrushes[(villager.Carry.Value.r, hover)]
                : _brushes[(Sprite.GetType(), hover)];
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

        private static Brush GetImageFill(Brush backgroundBrush, string imageName)
        {
            var oGrid = new Grid();
            oGrid.SetBinding(WidthProperty, new Binding(nameof(ActualWidth))
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor)
                {
                    AncestorType = typeof(Shape)
                }
            });
            oGrid.SetBinding(HeightProperty, new Binding(nameof(ActualHeight))
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor)
                {
                    AncestorType = typeof(Shape)
                }
            });

            var oRectangle = new Rectangle
            {
                Fill = backgroundBrush
            };
            oGrid.Children.Add(oRectangle);

            var border = new Border
            {
                BorderThickness = new Thickness(0),
                Padding = new Thickness(20)
            };

            var img = new Image
            {
                Source = new BitmapImage(new Uri($@"Resources/Images/{imageName}.png", UriKind.Relative))
            };
            border.Child = img;

            oGrid.Children.Add(border);

            return new VisualBrush
            {
                Visual = oGrid
            };
        }

        public static bool DisplayCraft(Sprite sprite)
        {
            return !sprite.Is<Unit>();
        }

        public int GetIndexZ()
        {
            return Sprite.Is<Unit>() ? UnitIndexZ : DefaultIndexZ;
        }

        public Shape BuildShape()
        {
            return Sprite.Is<Structure>() ? (Shape)new Rectangle() : new Ellipse();
        }
    }
}
