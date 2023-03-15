using System.Collections.Generic;
using System.Windows;

namespace Age_Of_Nothing.Sprites
{
    public abstract class Structure : FocusableSprite
    {
        protected Structure(Rect rect, IEnumerable<FocusableSprite> sprites)
            : base(rect, sprites)
        { }

        public Point Center => new Point(Surface.TopLeft.X + Surface.Width / 2, Surface.TopLeft.Y + Surface.Height / 2);
    }
}
