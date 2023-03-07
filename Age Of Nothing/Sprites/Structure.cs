using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Age_Of_Nothing.Sprites
{
    public abstract class Structure : Sprite
    {
        protected Structure(Rect rect)
            : base(rect, () => new Rectangle())
        { }

        protected abstract override Brush DefaultFill { get; }

        protected abstract override Brush HoverFill { get; }
    }
}
