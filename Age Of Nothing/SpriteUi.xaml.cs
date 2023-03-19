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

namespace Age_Of_Nothing
{
    /// <summary>
    /// Logique d'interaction pour SpriteUi.xaml
    /// </summary>
    public partial class SpriteUi : UserControl
    {
        private const int UnitIndexZ = 2;
        private const double UnitFocusStroke = 1;
        private const double UnitSpaceBetween = 1;

        private const int StructureIndexZ = 1;
        private const double StructureFocusStroke = 2;
        private const double StructureSpaceBetween = 2;

        private const int ResourceIndexZ = 1;
        private const double ResourceFocusStroke = 2;
        private const double ResourceSpaceBetween = 2;

        private readonly double _focusStroke;
        private readonly double _strokeAndSpace;
        private readonly double _totalStrokeSize;

        private readonly Shape _surround;
        private readonly Shape _visual;

        public Sprite Sprite { get; }

        private readonly Brush _goldCarryBrush = GetImageFill(Brushes.SandyBrown, "gold");
        private readonly Brush _woodCarryBrush = GetImageFill(Brushes.SandyBrown, "wood");
        private readonly Brush _rockCarryBrush = GetImageFill(Brushes.SandyBrown, "rock");
        private readonly Brush _goldCarryBrushHover = GetImageFill(Brushes.PeachPuff, "gold");
        private readonly Brush _woodCarryBrushHover = GetImageFill(Brushes.PeachPuff, "wood");
        private readonly Brush _rockCarryBrushHover = GetImageFill(Brushes.PeachPuff, "rock");

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

        public SpriteUi(Sprite sprite, bool isBlueprint)
        {
            InitializeComponent();

            Sprite = sprite;

            SetValue(Panel.ZIndexProperty, sprite.Is<Unit>()
                ? UnitIndexZ
                : (sprite.Is<Structure>()
                    ? StructureIndexZ
                    : ResourceIndexZ));

            _focusStroke = sprite.Is<Unit>()
                ? UnitFocusStroke
                : (sprite.Is<Structure>()
                    ? StructureFocusStroke
                    : ResourceFocusStroke);
            _strokeAndSpace = sprite.Is<Unit>()
                ? UnitFocusStroke + UnitSpaceBetween
                : (sprite.Is<Structure>()
                    ? StructureFocusStroke + StructureSpaceBetween
                    : ResourceFocusStroke + ResourceSpaceBetween);
            _totalStrokeSize = _strokeAndSpace * 2;

            _visual = sprite.Is<Structure>()
                ? (Shape)new Rectangle()
                : (sprite.Is<Unit>()
                    ? new Ellipse()
                    : new Ellipse());
            _visual.Width = Sprite.Surface.Width;
            _visual.Height = Sprite.Surface.Height;
            _visual.Fill = GetFill();
            _visual.Opacity = isBlueprint ? 0.5 : 1;
            MainCanvas.Children.Add(_visual);

            _surround = sprite.Is<Structure>()
                ? (Shape)new Rectangle()
                : (sprite.Is<Unit>()
                    ? new Ellipse()
                    : new Ellipse());
            _surround.Stroke = Brushes.Black;
            _surround.StrokeThickness = _focusStroke;
            _surround.Width = Sprite.Surface.Width + _totalStrokeSize;
            _surround.Height = Sprite.Surface.Height + _totalStrokeSize;
            _surround.Fill = Brushes.Transparent;

            // do not move this line above the _visual definition
            SetControlDimensionsAndPosition();

            if (!isBlueprint)
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
            if (Sprite.Is<Villager>(out var villager))
            {
                return villager.Carry?.r switch
                {
                    ResourceTypes.Gold => hover ? _goldCarryBrushHover : _goldCarryBrush,
                    ResourceTypes.Wood => hover ? _woodCarryBrushHover : _woodCarryBrush,
                    ResourceTypes.Rock => hover ? _rockCarryBrushHover : _rockCarryBrush,
                    _ => _brushes[(Sprite.GetType(), hover)]
                };
            }
            else
            {
                return _brushes[(Sprite.GetType(), hover)];
            }
        }

        private void SetControlDimensionsAndPosition()
        {
            MainCanvas.Width = Sprite.Surface.Width + (Sprite.Focused ? _totalStrokeSize : 0);
            MainCanvas.Height = Sprite.Surface.Height + (Sprite.Focused ? _totalStrokeSize : 0);
            SetValue(Canvas.LeftProperty, Sprite.Surface.Left - (Sprite.Focused ? _strokeAndSpace : 0));
            SetValue(Canvas.TopProperty, Sprite.Surface.Top - (Sprite.Focused ? _strokeAndSpace : 0));
            _visual.SetValue(Canvas.LeftProperty, Sprite.Focused ? _strokeAndSpace : double.NaN);
            _visual.SetValue(Canvas.TopProperty, Sprite.Focused ? _strokeAndSpace : double.NaN);
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

        public static bool DisplayBlueprint(Sprite sprite)
        {
            return !sprite.Is<Unit>();
        }
    }
}
