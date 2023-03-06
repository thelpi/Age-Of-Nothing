using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Age_Of_Nothing.Sprites
{
    public abstract class CenteredSprite : Sprite
    {
        protected IReadOnlyList<CenteredSprite> Sprites { get; }

        protected abstract Brush FocusFill { get; }
        protected abstract Brush HoverFocusFill { get; }

        protected CenteredSprite(Point position, double size, IReadOnlyList<CenteredSprite> sprites, int zIndex)
            : base(size, size, () => new Ellipse(), zIndex)
        {
            Sprites = sprites;
            Position = position;

            Visual.MouseLeftButtonDown += (a, b) =>
            {
                ChangeFocus(!Focused, true);
                foreach (var x in Sprites)
                {
                    if (x != this)
                        x.ChangeFocus(false, false);
                }
            };

            RefreshVisual(false);
            RefreshPosition();
        }

        public Point Position { get; protected set; }

        public bool Focused { get; private set; }

        public override Rect Surface => new Rect(
            new Point(Position.X - Width / 2, Position.Y - Width / 2),
            new Point(Position.X + Height / 2, Position.Y + Height / 2));

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
    }
}
