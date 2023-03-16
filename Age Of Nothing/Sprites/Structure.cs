using System.Collections.Generic;
using System.Windows;

namespace Age_Of_Nothing.Sprites
{
    public abstract class Structure : FocusableSprite
    {
        protected Structure(Rect rect, IEnumerable<FocusableSprite> sprites)
            : base(rect, sprites, false, true)
        { }
    }
}
