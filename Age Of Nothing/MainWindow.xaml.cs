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
        private readonly List<Mine> _mines = new List<Mine>(10); // TODO: adjust
        private readonly List<Forest> _forest = new List<Forest>(10); // TODO: adjust
        private readonly List<CenteredSprite> _sprites = new List<CenteredSprite>(10); // TODO: adjust
        private readonly Rectangle _selectionRectGu;

        private Point? _selectionPoint;
        private volatile bool _refreshing = false;

        public MainWindow()
        {
            InitializeComponent();
            _timer.Elapsed += Refresh;
            _timer.Start();

            _units.Add(new Unit(new Point(200, 200), 4, 20, _sprites));
            _units.Add(new Unit(new Point(100, 100), 3, 20, _sprites));
            _units.Add(new Unit(new Point(300, 300), 3, 20, _sprites));

            _selectionRectGu = new Rectangle
            {
                Fill = Brushes.Red,
                Opacity = 0.1
            };

            _mines.Add(new Mine(100, new Point(400, 120), 1, false, _sprites));
            _mines.Add(new Mine(75, new Point(200, 600), 1, true, _sprites));

            _forest.Add(new Forest(new Rect(700, 200, 300, 100)));

            _sprites.Add(_units[0]);
            _sprites.Add(_units[1]);
            _sprites.Add(_units[2]);
            _sprites.Add(_mines[0]);
            _sprites.Add(_mines[1]);

            foreach (var sprite in _sprites)
                MainCanvas.Children.Add(sprite.Visual);
            MainCanvas.Children.Add(_forest[0].Visual);
            MainCanvas.Children.Add(_selectionRectGu);
        }

        private void Refresh(object sender, ElapsedEventArgs e)
        {
            if (_refreshing) return;
            _refreshing = true;

            foreach (var unit in _units)
            {
                if (unit.CheckForMovement())
                    Dispatcher.BeginInvoke(new Action(() => unit.RefreshPosition()));
            }

            _refreshing = false;
        }

        #region Events

        private void MainCanvas_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var clickPosition = e.GetPosition(MainCanvas);

            var mine = _mines.FirstOrDefault(x => x.Surface.Contains(clickPosition));
            if (mine != null)
                clickPosition = mine.Position;

            foreach (var unit in _units.Where(x => x.Focused))
            {
                unit.TargetPosition = clickPosition;
            }
        }

        private void MainCanvas_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender == e.Source)
            {
                _selectionPoint = e.GetPosition(MainCanvas);
                _units.ForEach(x => x.ChangeFocus(false, false));
            }
        }

        private void MainCanvas_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var endSelectionPoint = e.GetPosition(MainCanvas);
            if (_selectionPoint.HasValue)
            {
                var rect = new Rect(endSelectionPoint, _selectionPoint.Value);
                _units.ForEach(x =>
                {
                    if (rect.Contains(x.Position))
                        x.ChangeFocus(true, false);
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
                _units.ForEach(x => x.RefreshVisual(rect.Contains(x.Position)));
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
