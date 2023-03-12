using System.Collections.Generic;
using System.Windows;
using System.Windows.Shapes;

namespace Age_Of_Nothing.Sprites
{
    public abstract class Structure : FocusableSprite
    {
        protected Structure(Rect rect, IEnumerable<FocusableSprite> sprites)
            : base(rect, () => new Rectangle(), 0.9, sprites)
        { }

        public Point Center => new Point(Surface.TopLeft.X + Surface.Width / 2, Surface.TopLeft.Y + Surface.Height / 2);
    }
}
