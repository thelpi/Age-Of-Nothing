using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Age_Of_Nothing
{
    public abstract class CenteredSprite
    {
        protected IReadOnlyList<CenteredSprite> Sprites { get; }

        protected abstract int IndexZ { get; }
        protected abstract Brush DefaultFill { get; }
        protected abstract Brush HoverFill { get; }
        protected abstract Brush FocusFill { get; }
        protected abstract Brush HoverFocusFill { get; }

        protected CenteredSprite(Point position, double size, IReadOnlyList<CenteredSprite> sprites)
        {
            Sprites = sprites;
            Position = position;
            Size = size;

            Visual = new Ellipse
            {
                Width = Size,
                Height = Size
            };
            Visual.MouseEnter += (a, b) => RefreshVisual(true);
            Visual.MouseLeave += (a, b) => RefreshVisual(false);
            Visual.MouseLeftButtonDown += (a, b) =>
            {
                ChangeFocus(!Focused, true);
                foreach (var x in sprites)
                {
                    if (x != this)
                        x.ChangeFocus(false, false);
                }
            };

            RefreshVisual(false);
            RefreshPosition();
            Visual.SetValue(Panel.ZIndexProperty, IndexZ);
        }

        public void RefreshPosition()
        {
            Visual.SetValue(Canvas.LeftProperty, Position.X - (Size / 2));
            Visual.SetValue(Canvas.TopProperty, Position.Y - (Size / 2));
        }

        public Point Position { get; protected set; }

        public double Size { get; }

        public Ellipse Visual { get; }

        public bool Focused { get; private set; }

        public Rect Surface => new Rect(
            new Point(Position.X - Size / 2, Position.Y - Size / 2),
            new Point(Position.X + Size / 2, Position.Y + Size / 2));

        public virtual void RefreshVisual(bool hover)
        {
            Visual.Fill = hover
                ? (Focused
                    ? HoverFocusFill
                    : HoverFill)
                : (Focused
                    ? FocusFill
                    : DefaultFill);
        }

        public void ChangeFocus(bool focus, bool hover)
        {
            Focused = focus;
            RefreshVisual(hover);
        }
    }
}
