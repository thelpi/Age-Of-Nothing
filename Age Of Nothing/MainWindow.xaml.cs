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
        private volatile bool _refreshing = false;
        private readonly List<Unit> _units = new List<Unit>(10); // TODO: adjust
        private Point? _selectionPoint;
        private Rectangle _selectionRectGu;

        public MainWindow()
        {
            InitializeComponent();
            _timer.Elapsed += Refresh;
            _timer.Start();

            _units.Add(new Unit
            {
                CurrentPosition = new Point(200, 200),
                Speed = 3
            });
            _units.Add(new Unit
            {
                CurrentPosition = new Point(100, 100),
                Speed = 2
            });
            _units.Add(new Unit
            {
                CurrentPosition = new Point(300, 300),
                Speed = 1
            });

            foreach (var unit in _units)
                GenerateUnit(unit);
        }

        private void GenerateUnit(Unit unit)
        {
            var gu = new Ellipse
            {
                Width = 20,
                Height = 20,
                Fill = Brushes.Blue,
                Tag = unit
            };
            SetGraphicUnitPosition(gu, unit);
            gu.MouseEnter += (a, b) =>
            {
                gu.Fill = unit.Selected
                    ? (Brush)new RadialGradientBrush(Colors.CornflowerBlue, Colors.Red)
                    : Brushes.CornflowerBlue;
            };
            gu.MouseLeave += (a, b) =>
            {
                gu.Fill = unit.Selected
                    ? (Brush)new RadialGradientBrush(Colors.Blue, Colors.Red)
                    : Brushes.Blue;
            };
            gu.MouseLeftButtonDown += (a, b) =>
            {
                unit.Selected = !unit.Selected;
                _units.ForEach(x =>
                {
                    if (x != unit)
                    {
                        x.Selected = false;
                        FindGraphicUnit<Ellipse>(x).Fill = Brushes.Blue;
                    }
                });
                gu.Fill = unit.Selected
                    ? (Brush)new RadialGradientBrush(Colors.CornflowerBlue, Colors.Red)
                    : Brushes.CornflowerBlue;
            };

            MainCanvas.Children.Add(gu);
        }

        private void Refresh(object sender, ElapsedEventArgs e)
        {
            if (_refreshing) return;
            _refreshing = true;

            foreach (var unit in _units)
            {
                if (unit.TargetPosition.HasValue)
                {
                    var (x2, y2) = MathTools.ComputePointOnLine(unit.CurrentPosition.X,
                        unit.CurrentPosition.Y,
                        unit.TargetPosition.Value.X,
                        unit.TargetPosition.Value.Y,
                        unit.Speed);

                    var target = new Point(x2, y2);

                    // TODO: manage colision
                    unit.CurrentPosition = target;

                    if (unit.TargetPosition == unit.CurrentPosition)
                        unit.TargetPosition = null;

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        var gu = FindGraphicUnit<Ellipse>(unit);
                        SetGraphicUnitPosition(gu, unit);
                    }));
                }
            }

            _refreshing = false;
        }

        private T FindGraphicUnit<T>(Unit unit) where T : FrameworkElement
        {
            return MainCanvas.Children.OfType<T>().First(x => x.Tag == unit);
        }

        private void SetGraphicUnitPosition(FrameworkElement gu, Unit unit)
        {
            gu.SetValue(Canvas.LeftProperty, unit.CurrentPosition.X - (gu.Width / 2));
            gu.SetValue(Canvas.TopProperty, unit.CurrentPosition.Y - (gu.Height / 2));
        }

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
                    FindGraphicUnit<Ellipse>(x).Fill = Brushes.Blue;
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
                        FindGraphicUnit<Ellipse>(x).Fill = new RadialGradientBrush(Colors.Blue, Colors.Red);
                    }
                });
            }
            _selectionPoint = null;
            if (_selectionRectGu != null)
            {
                MainCanvas.Children.Remove(_selectionRectGu);
                _selectionRectGu = null;
            }
        }

        private void MainCanvas_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _selectionPoint = null;
            if (_selectionRectGu != null)
            {
                MainCanvas.Children.Remove(_selectionRectGu);
                _selectionRectGu = null;
            }
        }

        private void MainCanvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_selectionPoint.HasValue)
            {
                var rect = new Rect(_selectionPoint.Value, e.GetPosition(MainCanvas));

                if (_selectionRectGu == null)
                {
                    _selectionRectGu = new Rectangle
                    {
                        Fill = Brushes.Red,
                        Opacity = 0.1
                    };
                    MainCanvas.Children.Add(_selectionRectGu);
                }
                _selectionRectGu.Width = rect.Width;
                _selectionRectGu.Height = rect.Height;
                _selectionRectGu.SetValue(Canvas.LeftProperty, rect.Left);
                _selectionRectGu.SetValue(Canvas.TopProperty, rect.Top);
            }
        }
    }
}
