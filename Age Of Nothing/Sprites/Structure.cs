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
    }
}
