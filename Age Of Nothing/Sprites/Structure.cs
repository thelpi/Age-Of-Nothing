using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Age_Of_Nothing.Sprites
{
    public abstract class Structure : Sprite
    {
        public override Rect Surface { get; }

        protected Structure(Rect rect) : base(rect.Width, rect.Height, () => new Rectangle(), 1)
        {
            Surface = rect;

            RefreshVisual(false);
            RefreshPosition();
        }

        protected abstract override Brush DefaultFill { get; }

        protected abstract override Brush HoverFill { get; }
    }
}
