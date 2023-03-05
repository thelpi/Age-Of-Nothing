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

                    unit.CurrentPosition = new Point(x2, y2);

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
    }
}
