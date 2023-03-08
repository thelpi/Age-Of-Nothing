using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Age_Of_Nothing.Sprites
{
    public abstract class FocusableSprite : Sprite
    {
        protected IReadOnlyList<FocusableSprite> Sprites { get; }

        protected abstract Brush FocusFill { get; }
        protected abstract Brush HoverFocusFill { get; }

        protected FocusableSprite(Rect surface, Func<Shape> shaper, IReadOnlyList<FocusableSprite> sprites, int zIndex = 1, bool canMove = false)
            : base(surface, shaper, zIndex, canMove)
        {
            Sprites = sprites;

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
            RefreshToolTip();
        }

        public void ChangeFocus(bool focus, bool hover)
        {
            Focused = focus;
            RefreshVisual(hover);
        }
    }
}
