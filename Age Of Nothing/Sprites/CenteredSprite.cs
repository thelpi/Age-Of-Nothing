using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Age_Of_Nothing.Sprites
{
    public abstract class CenteredSprite : Sprite
    {
        private Point _position;

        protected IReadOnlyList<CenteredSprite> Sprites { get; }

        protected abstract Brush FocusFill { get; }
        protected abstract Brush HoverFocusFill { get; }

        protected CenteredSprite(Point position, double size, IReadOnlyList<CenteredSprite> sprites, int zIndex = 1, bool canMove = false)
            : base(ComputeSurfaceFromMiddlePoint(position, size, size), () => new Ellipse(), zIndex, canMove)
        {
            Sprites = sprites;
            _position = position;

            Visual.MouseLeftButtonDown += (a, b) =>
            {
                ChangeFocus(!Focused, true);
                foreach (var x in Sprites)
                {
                    if (x != this)
                        x.ChangeFocus(false, false);
                }
            };
        }

        public Point Position
        {
            get { return _position; }
            protected set
            {
                _position = value;
                Move(ComputeSurfaceFromMiddlePoint(Position, Surface.Width, Surface.Height).TopLeft);
            }
        }

        public bool Focused { get; private set; }

        public override void RefreshVisual(bool hover)
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

        private static Rect ComputeSurfaceFromMiddlePoint(Point point, double width, double height)
        {
            return new Rect(
                new Point(point.X - width / 2, point.Y - height / 2),
                new Point(point.X + width / 2, point.Y + height / 2));
        }
    }
}
