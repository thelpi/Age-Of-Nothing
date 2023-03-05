using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Age_Of_Nothing
{
    public abstract class Sprite<T> where T : Shape, new()
    {
        public double Width { get; }
        public double Height { get; }

        protected abstract int IndexZ { get; }
        protected abstract Brush DefaultFill { get; }
        protected abstract Brush HoverFill { get; }
        public abstract Rect Surface { get; }

        protected Sprite(double width, double height)
        {
            Width = width;
            Height = height;
            Visual = new T
            {
                Width = Width,
                Height = Height
            };
            Visual.MouseEnter += (a, b) => RefreshVisual(true);
            Visual.MouseLeave += (a, b) => RefreshVisual(false);

            RefreshVisual(false);
            RefreshPosition();
            Visual.SetValue(Panel.ZIndexProperty, IndexZ);
        }

        public void RefreshPosition()
        {
            Visual.SetValue(Canvas.LeftProperty, Surface.Left);
            Visual.SetValue(Canvas.TopProperty, Surface.Top);
        }

        public T Visual { get; }

        public virtual void RefreshVisual(bool hover)
        {
            Visual.Fill = hover
                ? HoverFill
                : DefaultFill;
        }
    }
}
