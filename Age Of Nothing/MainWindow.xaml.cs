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

        private Size? _structureShadowSize;
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
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    switch (e.PropertyName)
                    {
                        case SpriteFocusChangedEventArgs.SpriteFocusPropertyName:
                            CreateDwellingButton.IsEnabled = _controller.HasVillagerFocus();
                            CreateVillagerButton.IsEnabled = _controller.HasMarketFocus();
                            break;
                        case SpritesCollectionChangedEventArgs.SpritesCollectionAddPropertyName:
                        case SpritesCollectionChangedEventArgs.CraftsCollectionAddPropertyName:
                            var addEvt = e as SpritesCollectionChangedEventArgs;
                            if (addEvt.Sprite != null)
                            {
                                // TODO: makes this generic
                                if (addEvt.Sprite.Is<Villager>(out var v))
                                    MainCanvas.Children.Add(new VillagerUi(v));
                                else if (addEvt.Sprite.Is<Structure>(out var s))
                                    MainCanvas.Children.Add(new StructureUi(s, addEvt.IsBlueprint));
                                else
                                    throw new NotImplementedException();
                            }
                            else // will be removed when every sprite will have their UI equivalent
                                MainCanvas.Children.Add(addEvt.SpriteVisualRecipe());
                            break;
                        case SpritesCollectionChangedEventArgs.SpritesCollectionRemovePropertyName:
                        case SpritesCollectionChangedEventArgs.CraftsCollectionRemovePropertyName:
                            var rmvEvt = e as SpritesCollectionChangedEventArgs;
                            if (rmvEvt.Sprite != null)
                            {
                                // TODO: makes this generic
                                if (rmvEvt.Sprite.Is<Villager>(out var v))
                                    MainCanvas.Children.Remove(FindCanvasElement<VillagerUi, Villager>(v));
                                else if (rmvEvt.Sprite.Is<Structure>(out var s))
                                    MainCanvas.Children.Remove(FindCanvasElement<StructureUi, Structure>(s));
                                else
                                    throw new NotImplementedException();
                            }
                            else // will be removed when every sprite will have their UI equivalent
                                MainCanvas.Children.Remove(rmvEvt.SpriteVisualRecipe());
                            break;
                    }
                }));
            };

            MainCanvas.Children.Add(_selectionRectGu);
            MainCanvas.Children.Add(_structureShadowGu);

            DataContext = _controller;

            _controller.Initialize();
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
                _controller.BuildDwelling(e.GetPosition(MainCanvas));
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
                _structureShadowGu.Width = _structureShadowSize.Value.Width;
                _structureShadowGu.Height = _structureShadowSize.Value.Height;
                _structureShadowGu.SetValue(Canvas.LeftProperty, pos.X - (_structureShadowSize.Value.Width / 2));
                _structureShadowGu.SetValue(Canvas.TopProperty, pos.Y - (_structureShadowSize.Value.Height / 2));
            }
        }

        private void CreateVillagerButton_Click(object sender, RoutedEventArgs e)
        {
            ResetStructureShadow();
            _controller.AddVillagerCreationToStack();
        }

        private void CreateDwellingButton_Click(object sender, RoutedEventArgs e)
        {
            _structureShadowSize = _controller.GetSpriteSize<Dwelling>();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            ResetStructureShadow();
            if (_deleteKeys.Contains(e.Key))
                _controller.CheckForDeletion();
        }

        #endregion Events

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
    }
}
