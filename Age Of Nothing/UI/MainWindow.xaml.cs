using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Age_Of_Nothing.Events;
using Age_Of_Nothing.Sprites;
using Age_Of_Nothing.Sprites.Structures;
using Age_Of_Nothing.Sprites.Units;

namespace Age_Of_Nothing.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int Fps = 20;
        private const double AreaMoveRateY = 5;

        private static readonly double AreaMoveRateX = AreaMoveRateY * 16 / 9;
        private static readonly Key[] _deleteKeys = new[] { Key.Delete, Key.X };
        private static readonly double Delay = 1 / (Fps / (double)1000);

        private readonly Timer _timer = new Timer(Delay);
        private readonly Rectangle _selectionRectGu;
        private readonly Rectangle _structureShadowGu;
        private readonly Controller _controller;

        private (Size size, Type target, bool continuous)? _structureShadowSize;
        private Point? _selectionPoint;
        private Point? _craftPoint;
        private volatile bool _refreshing = false;
        private Directions? _scrollingX;
        private Directions? _scrollingY;
        private Button[] _areaButtons = null;

        public double OffsetX { get; private set; }
        public double OffsetY { get; private set; }
        private Button[] AreaButtons => _areaButtons ??= new[]
        {
            LeftTopButton, TopButton, RightTopButton,
            LeftButton, RightButton,
            LeftBottomButton, BottomButton, RightBottomButton
        };

        public MainWindow()
        {
            InitializeComponent();
            _timer.Elapsed += Refresh;
            _timer.Start();

            _structureShadowGu = new Rectangle
            {
                Fill = Brushes.Black,
                Opacity = 0.1
            };

            _selectionRectGu = new Rectangle
            {
                Fill = Brushes.Red,
                Opacity = 0.1
            };

            _controller = new Controller();
            OffsetX = -(_controller.Width / 2);
            OffsetY = -(_controller.Height / 2);

            _controller.PropertyChanged += (s, e) =>
            {
                Action action = null;
                if (e.PropertyName == nameof(Sprite.Focused))
                {
                    action = () =>
                    {
                        var villagerFocus = _controller.FocusedSprites<Villager>().Any();
                        CreateDwellingButton.IsEnabled = villagerFocus;
                        CreateWallButton.IsEnabled = villagerFocus;
                        CreateMarketButton.IsEnabled = villagerFocus;
                        CreateBarracksButton.IsEnabled = villagerFocus;

                        var structureFocus = _controller.FocusedSprites<Structure>();
                        CreateVillagerButton.IsEnabled = structureFocus.Any(x => x.CanBuild<Villager>());
                        CreateSwordsmanButton.IsEnabled = structureFocus.Any(x => x.CanBuild<Swordsman>());
                        CreateArcherButton.IsEnabled = structureFocus.Any(x => x.CanBuild<Archer>());
                        CreateKnightButton.IsEnabled = structureFocus.Any(x => x.CanBuild<Knight>());
                    };
                }
                else if (e.PropertyName == SpritesCollectionChangedEventArgs.SpritesCollectionAddPropertyName)
                {
                    action = () =>
                    {
                        var addEvt = e as SpritesCollectionChangedEventArgs;
                        if (!addEvt.IsCraft || SpriteUi.DisplayCraft(addEvt.Sprite))
                            MainCanvas.Children.Add(new SpriteUi(addEvt.Sprite, addEvt.IsCraft, this));
                        if (addEvt.IsCraft)
                            CraftQueuePanel.Children.Add(new UI.CraftUi(addEvt.Craft));
                    };
                }
                else if (e.PropertyName == SpritesCollectionChangedEventArgs.SpritesCollectionRemovePropertyName)
                {
                    action = () =>
                    {
                        var rmvEvt = e as SpritesCollectionChangedEventArgs;
                        if (!rmvEvt.IsCraft || SpriteUi.DisplayCraft(rmvEvt.Sprite))
                            MainCanvas.Children.Remove(FindCanvasElement(rmvEvt.Sprite));
                        if (rmvEvt.IsCraft)
                            CraftQueuePanel.Children.Remove(GetCraftSpriteVisualItem(rmvEvt.Craft));
                    };
                }
                else if (e.PropertyName == nameof(_controller.Population))
                {
                    action = () => PopulationValueText.Text = $"{_controller.Population} / {_controller.PotentialPopulation}";
                }
                if (action != null)
                    Dispatcher.BeginInvoke(action);
            };

            MainCanvas.Children.Add(_selectionRectGu);
            MainCanvas.Children.Add(_structureShadowGu);

            DataContext = _controller;

            _controller.Initialize();
        }

        #region Events

        private void MainCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ResetStructureShadow();
        }

        private void MainCanvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ResetStructureShadow();
            _controller.SetTargetPositionsOnFocused(e.GetPosition(MainCanvas));
        }

        private void MainCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender == e.Source)
            {
                _selectionPoint = e.GetPosition(MainCanvas);
                _controller.ClearFocus();
            }
            else if (_structureShadowGu == e.Source)
            {
                _craftPoint = e.GetPosition(MainCanvas).RescaleBase10();
            }
        }

        private void MainCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_selectionPoint.HasValue)
                _controller.FocusOnZone(new Rect(e.GetPosition(MainCanvas), _selectionPoint.Value));
            if (_structureShadowSize.HasValue)
            {
                var finalPoint = e.GetPosition(MainCanvas);
                List<Point> centers;
                if (_structureShadowSize.Value.continuous && _craftPoint.HasValue)
                    centers = GetAllContiguousStructuresCenters(finalPoint);
                else
                    centers = new List<Point> { finalPoint };
                _controller.BuildStructure(_structureShadowSize.Value.target, centers);
                ResetStructureShadow();
            }
            ResetSelectionRectangle();
        }

        private List<Point> GetAllContiguousStructuresCenters(Point finalPoint)
        {
            var centers = new List<Point>(50);
            var x1 = Math.Min(finalPoint.X, _craftPoint.Value.X);
            var y1 = Math.Min(finalPoint.Y, _craftPoint.Value.Y);
            var x2 = Math.Max(finalPoint.X, _craftPoint.Value.X);
            var y2 = Math.Max(finalPoint.Y, _craftPoint.Value.Y);
            for (var x = x1; x <= x2; x += _structureShadowSize.Value.size.Width)
            {
                for (var y = y1; y <= y2; y += _structureShadowSize.Value.size.Height)
                    centers.Add(new Point(x, y));
            }
            return centers;
        }

        private void MainCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            ResetSelectionRectangle();
        }

        private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_selectionPoint.HasValue)
            {
                var rect = new Rect(_selectionPoint.Value, e.GetPosition(MainCanvas));
                _selectionRectGu.Width = rect.Width;
                _selectionRectGu.Height = rect.Height;
                _selectionRectGu.SetValue(Canvas.LeftProperty, rect.Left);
                _selectionRectGu.SetValue(Canvas.TopProperty, rect.Top);
                _controller.RefreshHover(rect);
            }

            if (_structureShadowSize.HasValue)
            {
                var (size, _, continuous) = _structureShadowSize.Value;
                var pos = e.GetPosition(MainCanvas).RescaleBase10();
                if (_craftPoint.HasValue && continuous)
                {
                    _structureShadowGu.Width = GeometryTools.GetModuloDim(size.Width, pos.X, _craftPoint.Value.X);
                    _structureShadowGu.Height = GeometryTools.GetModuloDim(size.Height, pos.Y, _craftPoint.Value.Y);
                    _structureShadowGu.SetValue(Canvas.LeftProperty, Math.Min(pos.X, _craftPoint.Value.X) - (size.Width / 2));
                    _structureShadowGu.SetValue(Canvas.TopProperty, Math.Min(pos.Y, _craftPoint.Value.Y) - (size.Width / 2));
                }
                else
                {
                    _structureShadowGu.Width = size.Width;
                    _structureShadowGu.Height = size.Height;
                    _structureShadowGu.SetValue(Canvas.LeftProperty, pos.X - (size.Width / 2));
                    _structureShadowGu.SetValue(Canvas.TopProperty, pos.Y - (size.Height / 2));
                }
            }
        }

        private void CreateVillagerButton_Click(object sender, RoutedEventArgs e)
        {
            ResetStructureShadow();
            _controller.AddUnitToStack<Villager>();
        }

        private void CreateSwordsmanButton_Click(object sender, RoutedEventArgs e)
        {
            ResetStructureShadow();
            _controller.AddUnitToStack<Swordsman>();
        }

        private void CreateArcherButton_Click(object sender, RoutedEventArgs e)
        {
            ResetStructureShadow();
            _controller.AddUnitToStack<Archer>();
        }

        private void CreateKnightButton_Click(object sender, RoutedEventArgs e)
        {
            ResetStructureShadow();
            _controller.AddUnitToStack<Knight>();
        }

        private void CreateDwellingButton_Click(object sender, RoutedEventArgs e)
        {
            SetStructureShadowSize<Dwelling>(false);
        }

        private void CreateMarketButton_Click(object sender, RoutedEventArgs e)
        {
            SetStructureShadowSize<Market>(false);
        }

        private void CreateBarracksButton_Click(object sender, RoutedEventArgs e)
        {
            SetStructureShadowSize<Barracks>(false);
        }

        private void CreateWallButton_Click(object sender, RoutedEventArgs e)
        {
            SetStructureShadowSize<Wall>(true);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            ResetStructureShadow();
            if (_deleteKeys.Contains(e.Key))
                _controller.CheckForDeletion();
        }

        private void AreaButton_MouseEnter(object sender, MouseEventArgs e)
        {
            SetAreaDirections();
        }

        private void AreaButton_MouseLeave(object sender, MouseEventArgs e)
        {
            SetAreaDirections();
        }

        #endregion Events

        private void SetAreaDirections()
        {
            var hasHit = false;
            if (Mouse.DirectlyOver is Decorator decorator)
            {
                var btn = AreaButtons.FirstOrDefault(x => decorator.TemplatedParent == x);
                if (btn != null)
                {
                    var directions = btn.Tag.ToString().Split('-').Select(x =>
                        string.IsNullOrWhiteSpace(x) ? default(Directions?) : Enum.Parse<Directions>(x));
                    _scrollingX = directions.ElementAt(0);
                    _scrollingY = directions.ElementAt(1);
                    hasHit = true;
                }
            }

            if (!hasHit)
            {
                _scrollingX = null;
                _scrollingY = null;
            }
        }

        private void SetStructureShadowSize<T>(bool continuous) where T : Structure
        {
            _structureShadowSize = (Sprite.GetSpriteSize(typeof(T)), typeof(T), continuous);
        }

        private void ResetSelectionRectangle()
        {
            _selectionPoint = null;
            _selectionRectGu.Width = 0;
            _selectionRectGu.Height = 0;
        }

        private void ResetStructureShadow()
        {
            _structureShadowSize = null;
            _structureShadowGu.Width = 0;
            _structureShadowGu.Height = 0;
            _craftPoint = null;
        }

        private UIElement FindCanvasElement(Sprite sprite)
        {
            return MainCanvas.Children.OfType<SpriteUi>().FirstOrDefault(x => x.Sprite == sprite);
        }

        private void Refresh(object sender, ElapsedEventArgs e)
        {
            if (_refreshing) return;
            _refreshing = true;

            _controller.NewFrameCheck();

            if (_scrollingX.HasValue || _scrollingY.HasValue)
            {
                var newOffsetX = OffsetX;
                if (_scrollingX == Directions.Right)
                    newOffsetX -= AreaMoveRateX;
                else if (_scrollingX == Directions.Left)
                    newOffsetX += AreaMoveRateX;

                var newOffsetY = OffsetY;
                if (_scrollingY == Directions.Bottom)
                    newOffsetY -= AreaMoveRateY;
                else if (_scrollingY == Directions.Top)
                    newOffsetY += AreaMoveRateY;

                if (newOffsetX != OffsetX || newOffsetY != OffsetY)
                {
                    OffsetX = newOffsetX;
                    OffsetY = newOffsetY;
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        foreach (var spriteUi in MainCanvas.Children.OfType<SpriteUi>())
                            spriteUi.RefreshPosition();
                    }));
                }
            }

            _refreshing = false;
        }

        private CraftUi GetCraftSpriteVisualItem(Craft craft)
        {
            return CraftQueuePanel.Children.OfType<CraftUi>().FirstOrDefault(x => x.Craft == craft);
        }
    }
}
