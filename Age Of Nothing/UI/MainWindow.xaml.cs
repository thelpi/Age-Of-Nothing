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
        private const double AreaMoveRateY = 20;

        private static readonly double AreaMoveRateX = AreaMoveRateY * 16 / 9;
        private static readonly Key[] _deleteKeys = new[] { Key.Delete, Key.X };
        private static readonly double Delay = 1 / (Fps / (double)1000);

        private readonly Timer _timer = new Timer(Delay);
        private readonly Rectangle _selectionRectGu;
        private readonly Rectangle _structureShadowGu;
        private readonly Rectangle _area;
        private readonly Controller _controller;
        private readonly Button[] _areaButtons;

        private (Size size, Type target, bool continuous)? _structureShadowSize;
        private Point? _selectionPoint;
        private Point? _craftPoint;
        private bool _refreshing = false;
        private Directions? _scrollingX;
        private Directions? _scrollingY;

        public Point Offset { get; private set; }
        public Point Middle => new Point(MainCanvas.ActualWidth / 2, MainCanvas.ActualHeight / 2);

        public MainWindow()
        {
            InitializeComponent();
            _timer.Elapsed += Refresh;

            _areaButtons = new[]
            {
                LeftTopButton, TopButton, RightTopButton,
                LeftButton, RightButton,
                LeftBottomButton, BottomButton, RightBottomButton
            };

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

            _controller = new Controller(new GameParameters());

            _area = new Rectangle
            {
                Fill = Brushes.LightGreen,
                Width = _controller.Width,
                Height = _controller.Height
            };
            _area.SetValue(Panel.ZIndexProperty, 0);

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

            MainCanvas.Children.Add(_area);
            MainCanvas.Children.Add(_selectionRectGu);
            MainCanvas.Children.Add(_structureShadowGu);

            DataContext = _controller;

            Offset = _controller.Initialize();

            _timer.Start();
        }

        #region Events

        private void MainCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ResetStructureShadow();
        }

        private void MainCanvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ResetStructureShadow();
            var targetPoint = GetTargetPointOffset(e);
            _controller.SetTargetPositionsOnFocused(targetPoint);
        }

        private void MainCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_area == e.Source)
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
                _controller.FocusOnZone(new Rect(GetTargetPointOffset(e), _selectionPoint.Value.MoveFromOffset(Offset, Middle)));
            if (_structureShadowSize.HasValue)
            {
                var finalPoint = e.GetPosition(MainCanvas);
                var centers = _structureShadowSize.Value.continuous && _craftPoint.HasValue
                    ? finalPoint.GetAllContiguousStructuresCenters(_craftPoint.Value, _structureShadowSize.Value.size)
                    : new List<Point> { finalPoint };
                _controller.BuildStructure(_structureShadowSize.Value.target, centers.Select(x => x.MoveFromOffset(Offset, Middle).RescaleBase10()).ToList());
                ResetStructureShadow();
            }
            ResetSelectionRectangle();
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
                _controller.RefreshHover(new Rect(_selectionPoint.Value.MoveFromOffset(Offset, Middle), GetTargetPointOffset(e)));
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshEveryPosition();
        }

        #endregion Events

        private void SetAreaDirections()
        {
            var hasHit = false;
            if (Mouse.DirectlyOver is Decorator decorator)
            {
                var btn = _areaButtons.FirstOrDefault(x => decorator.TemplatedParent == x);
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
                var newOffsetX = GetNewOffsetX();
                var newOffsetY = GetNewOffsetY();
                if (newOffsetX != Offset.X || newOffsetY != Offset.Y)
                {
                    Offset = new Point(newOffsetX, newOffsetY);
                    Dispatcher.BeginInvoke(new Action(RefreshEveryPosition));
                }
            }

            _refreshing = false;
        }

        private void RefreshEveryPosition()
        {
            foreach (var spriteUi in MainCanvas.Children.OfType<SpriteUi>())
                spriteUi.RefreshPosition();
            SetAreaPosition();
        }

        private double GetNewOffsetY()
        {
            var newOffsetY = Offset.Y;
            if (_scrollingY == Directions.Top && Offset.Y - AreaMoveRateY >= 0)
                newOffsetY -= AreaMoveRateY;
            else if (_scrollingY == Directions.Bottom && Offset.Y + AreaMoveRateX <= _controller.Height)
                newOffsetY += AreaMoveRateY;
            return newOffsetY;
        }

        private double GetNewOffsetX()
        {
            var newOffsetX = Offset.X;
            if (_scrollingX == Directions.Left && Offset.X - AreaMoveRateX >= 0)
                newOffsetX -= AreaMoveRateX;
            else if (_scrollingX == Directions.Right && Offset.X + AreaMoveRateX <= _controller.Width)
                newOffsetX += AreaMoveRateX;
            return newOffsetX;
        }

        private void SetAreaPosition()
        {
            _area.SetValue(Canvas.LeftProperty, -Offset.X + Middle.X);
            _area.SetValue(Canvas.TopProperty, -Offset.Y + Middle.Y);
        }

        private CraftUi GetCraftSpriteVisualItem(Craft craft)
        {
            return CraftQueuePanel.Children.OfType<CraftUi>().FirstOrDefault(x => x.Craft == craft);
        }

        private Point GetTargetPointOffset(MouseEventArgs e)
        {
            return e.GetPosition(MainCanvas).MoveFromOffset(Offset, Middle);
        }
    }
}
