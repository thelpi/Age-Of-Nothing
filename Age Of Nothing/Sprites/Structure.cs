using System.Collections.Generic;
using System.Windows;
using Age_Of_Nothing.Sprites.Attributes;

namespace Age_Of_Nothing.Sprites
{
    public abstract class Structure : FocusableSprite
    {
        protected Structure(Rect rect, IEnumerable<FocusableSprite> sprites)
            : base(rect, sprites, false, true)
        { }

        public int GetUnitsStorage()
        {
            return GetType().GetAttribute<UnitsStorageAttribute>()?.UnitsStorage ?? 0;
        }
    }
}
