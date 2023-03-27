using System.Linq;
using System.Windows;
using Age_Of_Nothing.Sprites.Attributes;

namespace Age_Of_Nothing.Sprites.Structures
{
    public abstract class Structure : Sprite
    {
        public bool Tangible { get; set; }

        protected Structure(Point basePoint, Controller parent, bool tangible, int team)
            : base(basePoint, false, false, parent, team)
        {
            Tangible = tangible;
        }

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
