using System;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Age_Of_Nothing.Events;

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
        private readonly Controller _controller;

        private Point? _selectionPoint;
        private volatile bool _refreshing = false;

        public MainWindow()
        {
            InitializeComponent();
            _timer.Elapsed += Refresh;
            _timer.Start();

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
                            MainCanvas.Children.Add((e as SpritesCollectionChangedEventArgs).SpriteVisualRecipe());
                            break;
                        case SpritesCollectionChangedEventArgs.SpritesCollectionRemovePropertyName:
                            MainCanvas.Children.Remove((e as SpritesCollectionChangedEventArgs).SpriteVisualRecipe());
                            break;
                        case SpritePositionChangedEventArgs.SpritePositionPropertyName:
                            (e as SpritePositionChangedEventArgs).PositionCallback();
                            break;
                    }
                }));
            };

            MainCanvas.Children.Add(_selectionRectGu);

            DataContext = _controller;

            _controller.Initialize();
        }

        private void Refresh(object sender, ElapsedEventArgs e)
        {
            if (_refreshing) return;
            _refreshing = true;

            _controller.NewFrameCheck();

            _refreshing = false;
        }

        #region Events

        private void MainCanvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
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
        }

        private void CreateVillagerButton_Click(object sender, RoutedEventArgs e)
        {
            _controller.AddVillagerCreationToStack();
        }

        private void CreateDwellingButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
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
    }
}
