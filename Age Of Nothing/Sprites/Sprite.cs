using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Age_Of_Nothing.Sprites
{
    public abstract class Sprite
    {
        public double Width { get; }
        public double Height { get; }

        protected abstract Brush DefaultFill { get; }
        protected abstract Brush HoverFill { get; }
        public abstract Rect Surface { get; }

        protected Sprite(double width, double height, Func<Shape> shaper, int zIndex)
        {
            Width = width;
            Height = height;
            Visual = shaper();
            Visual.Width = Width;
            Visual.Height = Height;
            Visual.MouseEnter += (a, b) => RefreshVisual(true);
            Visual.MouseLeave += (a, b) => RefreshVisual(false);

            RefreshVisual(false);
            RefreshPosition();
            Visual.SetValue(Panel.ZIndexProperty, zIndex);
        }

        public void RefreshPosition()
        {
            Visual.SetValue(Canvas.LeftProperty, Surface.Left);
            Visual.SetValue(Canvas.TopProperty, Surface.Top);
        }

        public Shape Visual { get; }

        public virtual void RefreshVisual(bool hover)
        {
            Visual.Fill = hover
                ? HoverFill
                : DefaultFill;
        }
    }
}
