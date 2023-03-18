using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Age_Of_Nothing.Sprites.Attributes;

namespace Age_Of_Nothing.Sprites.Structures
{
    public abstract class Structure : FocusableSprite
    {
        protected Structure(Point basePoint, IEnumerable<FocusableSprite> sprites)
            : base(basePoint, false, sprites, false)
        { }

        public int GetUnitsStorage()
        {
            return GetType().GetAttribute<UnitsStorageAttribute>()?.UnitsStorage ?? 0;
        }

        public bool CanBuild<TUnit>()
            where TUnit : Units.Unit
        {
            return typeof(TUnit).GetAttribute<CraftInAttribute>().CraftIn?.Contains(GetType()) == true;
        }
    }
}
