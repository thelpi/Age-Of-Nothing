using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Age_Of_Nothing.Sprites
{
    public abstract class Sprite
    {
        protected abstract Brush DefaultFill { get; }
        protected abstract Brush HoverFill { get; }
        public Rect Surface { get; private set; }
        public bool CanMove { get; }

        protected Sprite(Rect surface, Func<Shape> shaper, int zIndex = 1, bool canMove = false)
        {
            Surface = surface;
            CanMove = canMove;
            Visual = shaper();
            Visual.Width = Surface.Width;
            Visual.Height = Surface.Height;
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

        protected bool Move(Point topLeftPoint)
        {
            if (!CanMove)
                return false;

            Surface = new Rect(topLeftPoint, Surface.Size);
            return true;
        }
    }
}
