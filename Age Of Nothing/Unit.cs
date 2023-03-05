using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Age_Of_Nothing
{
    public class Unit
    {
        private readonly double _size;

        public Unit(Point position, double speed, double visualSize, IEnumerable<Unit> units)
        {
            CurrentPosition = position;
            Speed = speed;
            _size = visualSize;

            Visual = new Ellipse
            {
                Width = _size,
                Height = _size
            };
            Visual.MouseEnter += (a, b) => RefreshVisual(true);
            Visual.MouseLeave += (a, b) => RefreshVisual(false);
            Visual.MouseLeftButtonDown += (a, b) =>
            {
                Selected = !Selected;
                RefreshVisual(true);
                foreach (var x in units)
                {
                    if (x != this)
                    {
                        x.Selected = false;
                        x.RefreshVisual(false);
                    }
                }
            };

            RefreshVisual(false);
            RefreshPosition();
            Visual.SetValue(Panel.ZIndexProperty, 2);
        }

        public Shape Visual { get; }
        // pixels by frame
        public double Speed { get; }

        public Rect Surface => new Rect(
            new Point(CurrentPosition.X - _size / 2, CurrentPosition.Y - _size / 2),
            new Point(CurrentPosition.X + _size / 2, CurrentPosition.Y + _size / 2));

        public bool Selected { get; set; }
        public Point? TargetPosition { get; set; }
        public Point CurrentPosition { get; set; }

        public void RefreshVisual(bool hover)
        {
            Visual.Fill = Selected
                ? (Brush)new RadialGradientBrush(hover ? Colors.CornflowerBlue : Colors.Blue, Colors.Red)
                : hover ? Brushes.CornflowerBlue : Brushes.Blue;
        }

        public void RefreshPosition()
        {
            Visual.SetValue(Canvas.LeftProperty, CurrentPosition.X - (_size / 2));
            Visual.SetValue(Canvas.TopProperty, CurrentPosition.Y - (_size / 2));
        }

        public bool CheckForMovement(IEnumerable<Unit> units)
        {
            if (TargetPosition.HasValue)
            {
                var (x2, y2) = MathTools.ComputePointOnLine(CurrentPosition.X,
                    CurrentPosition.Y,
                    TargetPosition.Value.X,
                    TargetPosition.Value.Y,
                    Speed);

                CurrentPosition = new Point(x2, y2);
                if (TargetPosition == CurrentPosition)
                    TargetPosition = null;

                return true;
            }

            return false;
        }
    }
}
