﻿using System;
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

            _controller = new Controller(MainCanvas.Width, MainCanvas.Height);
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
                            MainCanvas.Children.Add(new SpriteUi(addEvt.Sprite, addEvt.IsCraft));
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

        private void CreateWallButton_Click(object sender, RoutedEventArgs e)
        {
            SetStructureShadowSize<Wall>();
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

        private UIElement FindCanvasElement(Sprite sprite)
        {
            return MainCanvas.Children.OfType<SpriteUi>().FirstOrDefault(x => x.Sprite == sprite);
        }

        private void Refresh(object sender, ElapsedEventArgs e)
        {
            if (_refreshing) return;
            _refreshing = true;

            _controller.NewFrameCheck();

            _refreshing = false;
        }

        private CraftUi GetCraftSpriteVisualItem(Craft craft)
        {
            return CraftQueuePanel.Children.OfType<CraftUi>().FirstOrDefault(x => x.Craft == craft);
        }
    }
}