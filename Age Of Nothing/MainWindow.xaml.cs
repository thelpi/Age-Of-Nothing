using System;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Age_Of_Nothing.Events;
using Age_Of_Nothing.Sprites;
using Age_Of_Nothing.Sprites.Resources;
using Age_Of_Nothing.Sprites.Structures;
using Age_Of_Nothing.Sprites.Units;
using Age_Of_Nothing.SpritesUi;

namespace Age_Of_Nothing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int Fps = 20;

        private static readonly Key[] _deleteKeys = new[] { Key.Delete, Key.X };
        private static readonly double Delay = 1 / (Fps / (double)1000);

        private readonly Timer _timer = new Timer(Delay);
        private readonly Rectangle _selectionRectGu;
        private readonly Rectangle _structureShadowGu;
        private readonly Controller _controller;

        private (Size size, Type target)? _structureShadowSize;
        private Point? _selectionPoint;
        private volatile bool _refreshing = false;

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
            _controller.PropertyChanged += (s, e) =>
            {
                Action action = null;
                if (e.PropertyName == nameof(Sprite.Focused))
                {
                    action = () =>
                    {
                        // TODO: we should do better with the target type and CraftInAttribute
                        var villagerFocus = _controller.HasFocus<Villager>();
                        var barracksFocus = _controller.HasFocus<Barracks>();
                        CreateDwellingButton.IsEnabled = villagerFocus;
                        CreateMarketButton.IsEnabled = villagerFocus;
                        CreateBarracksButton.IsEnabled = villagerFocus;
                        CreateVillagerButton.IsEnabled = _controller.HasFocus<Market>();
                        CreateSwordsmanButton.IsEnabled = barracksFocus;
                        CreateArcherButton.IsEnabled = barracksFocus;
                        CreateKnightButton.IsEnabled = barracksFocus;
                    };
                }
                else if (e.PropertyName == SpritesCollectionChangedEventArgs.SpritesCollectionAddPropertyName)
                {
                    action = () =>
                    {
                        var addEvt = e as SpritesCollectionChangedEventArgs;
                        // We wont fix this if only 3 subtypes
                        if (addEvt.Sprite.Is<Unit>(out var addU) && !addEvt.IsBlueprint)
                            MainCanvas.Children.Add(new UnitUi(addU));
                        else if (addEvt.Sprite.Is<Structure>(out var addS))
                            MainCanvas.Children.Add(new StructureUi(addS, addEvt.IsBlueprint));
                        else if (addEvt.Sprite.Is<Resource>(out var addR))
                            MainCanvas.Children.Add(new ResourceUi(addR));

                        if (addEvt.IsBlueprint)
                        {
                            var panel = new StackPanel
                            {
                                Orientation = Orientation.Vertical,
                                Margin = new Thickness(0, 5, 0, 0),
                                Tag = addEvt.Sprite
                            };

                            var lbl = new Label
                            {
                                Content = addEvt.Sprite.GetType().Name
                            };
                            panel.Children.Add(lbl);

                            var pgb = new ProgressBar
                            {
                                Width = 100,
                                Height = 23,
                                Margin = new Thickness(0, 2, 0, 0),
                                Minimum = 0,
                                Maximum = 100,
                                Value = 0
                            };
                            panel.Children.Add(pgb);

                            CraftQueuePanel.Children.Add(panel);
                        }
                    };
                }
                else if (e.PropertyName == SpritesCollectionChangedEventArgs.SpritesCollectionRemovePropertyName)
                {
                    action = () =>
                    {
                        var rmvEvt = e as SpritesCollectionChangedEventArgs;
                        // We wont fix this if only 3 subtypes
                        if (rmvEvt.Sprite.Is<Unit>(out var rmvU) && !rmvEvt.IsBlueprint)
                            MainCanvas.Children.Remove(FindCanvasElement<UnitUi, Unit>(rmvU));
                        else if (rmvEvt.Sprite.Is<Structure>(out var rmvS))
                            MainCanvas.Children.Remove(FindCanvasElement<StructureUi, Structure>(rmvS));
                        else if (rmvEvt.Sprite.Is<Resource>(out var rmvR))
                            MainCanvas.Children.Remove(FindCanvasElement<ResourceUi, Resource>(rmvR));

                        if (rmvEvt.IsBlueprint)
                            CraftQueuePanel.Children.Remove(GetCraftSpriteVisualItem(rmvEvt.Sprite));
                    };
                }
                else if (e.PropertyName == nameof(_controller.Population))
                {
                    action = () => PopulationValueText.Text = $"{_controller.Population} / {_controller.PotentialPopulation}";
                }
                else if (e.PropertyName == nameof(Craft.Progression))
                {
                    action = () =>
                    {
                        var craft = s as Craft;
                        var panel = GetCraftSpriteVisualItem(craft.Target);
                        var pgb = (panel as StackPanel).Children[1] as ProgressBar;
                        pgb.Value = craft.Progression;
                    };
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
        }

        private void MainCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_selectionPoint.HasValue)
                _controller.FocusOnZone(new Rect(e.GetPosition(MainCanvas), _selectionPoint.Value));
            if (_structureShadowSize.HasValue)
            {
                _controller.BuildStructure(_structureShadowSize.Value.target, e.GetPosition(MainCanvas));
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
                _controller.RefreshHover(rect);
            }

            if (_structureShadowSize.HasValue)
            {
                var pos = e.GetPosition(MainCanvas);
                _structureShadowGu.Width = _structureShadowSize.Value.size.Width;
                _structureShadowGu.Height = _structureShadowSize.Value.size.Height;
                _structureShadowGu.SetValue(Canvas.LeftProperty, pos.X - (_structureShadowSize.Value.size.Width / 2));
                _structureShadowGu.SetValue(Canvas.TopProperty, pos.Y - (_structureShadowSize.Value.size.Height / 2));
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
            SetStructureShadowSize<Dwelling>();
        }

        private void CreateMarketButton_Click(object sender, RoutedEventArgs e)
        {
            SetStructureShadowSize<Market>();
        }

        private void CreateBarracksButton_Click(object sender, RoutedEventArgs e)
        {
            SetStructureShadowSize<Barracks>();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            ResetStructureShadow();
            if (_deleteKeys.Contains(e.Key))
                _controller.CheckForDeletion();
        }

        #endregion Events

        private void SetStructureShadowSize<T>() where T : Structure
        {
            _structureShadowSize = (Sprite.GetSpriteSize(typeof(T)), typeof(T));
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
        }

        private UIElement FindCanvasElement<T, T2>(T2 sprite)
            where T : BaseSpriteUi<T2>
            where T2 : Sprite
        {
            return MainCanvas.Children.OfType<T>().FirstOrDefault(x => x.Sprite == sprite);
        }

        private void Refresh(object sender, ElapsedEventArgs e)
        {
            if (_refreshing) return;
            _refreshing = true;

            _controller.NewFrameCheck();

            _refreshing = false;
        }

        private FrameworkElement GetCraftSpriteVisualItem(Sprite sprite)
        {
            return CraftQueuePanel.Children.OfType<FrameworkElement>()
                .FirstOrDefault(x => x.Tag == sprite);
        }
    }
}
