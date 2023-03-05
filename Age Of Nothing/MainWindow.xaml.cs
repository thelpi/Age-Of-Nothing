using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Age_Of_Nothing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int Fps = 20;

        private static readonly double Delay = 1 / (Fps / (double)1000);

        private readonly Timer _timer = new Timer(Delay);
        private readonly List<Unit> _units = new List<Unit>(10); // TODO: adjust
        private readonly Rectangle _selectionRectGu;

        private Point? _selectionPoint;
        private volatile bool _refreshing = false;

        public MainWindow()
        {
            InitializeComponent();
            _timer.Elapsed += Refresh;
            _timer.Start();

            _units.Add(new Unit(new Point(200, 200), 4, 20, _units));
            _units.Add(new Unit(new Point(100, 100), 3, 20, _units));
            _units.Add(new Unit(new Point(300, 300), 3, 20, _units));

            _selectionRectGu = new Rectangle
            {
                Fill = Brushes.Red,
                Opacity = 0.1
            };

            MainCanvas.Children.Add(_units[0].Visual);
            MainCanvas.Children.Add(_units[1].Visual);
            MainCanvas.Children.Add(_units[2].Visual);
            MainCanvas.Children.Add(_selectionRectGu);
        }

        private void Refresh(object sender, ElapsedEventArgs e)
        {
            if (_refreshing) return;
            _refreshing = true;

            foreach (var unit in _units)
            {
                if (unit.CheckForMovement(_units))
                    Dispatcher.BeginInvoke(new Action(() => unit.RefreshPosition()));
            }

            _refreshing = false;
        }

        #region Events

        private void MainCanvas_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            foreach (var unit in _units.Where(x => x.Selected))
                unit.TargetPosition = e.GetPosition(MainCanvas);
        }

        private void MainCanvas_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender == e.Source)
            {
                _selectionPoint = e.GetPosition(MainCanvas);
                _units.ForEach(x =>
                {
                    x.Selected = false;
                    x.RefreshVisual(false);
                });
            }
        }

        private void MainCanvas_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var endSelectionPoint = e.GetPosition(MainCanvas);
            if (_selectionRectGu == e.Source && _selectionPoint.HasValue)
            {
                var rect = new Rect(endSelectionPoint, _selectionPoint.Value);
                _units.ForEach(x =>
                {
                    if (rect.Contains(x.CurrentPosition))
                    {
                        x.Selected = true;
                        x.RefreshVisual(false);
                    }
                });
            }
            ResetSelectionRectangle();
        }

        private void MainCanvas_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ResetSelectionRectangle();
        }

        private void MainCanvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_selectionPoint.HasValue)
            {
                var rect = new Rect(_selectionPoint.Value, e.GetPosition(MainCanvas));
                _selectionRectGu.Width = rect.Width;
                _selectionRectGu.Height = rect.Height;
                _selectionRectGu.SetValue(Canvas.LeftProperty, rect.Left);
                _selectionRectGu.SetValue(Canvas.TopProperty, rect.Top);
                _units.ForEach(x => x.RefreshVisual(rect.Contains(x.CurrentPosition)));
            }
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
