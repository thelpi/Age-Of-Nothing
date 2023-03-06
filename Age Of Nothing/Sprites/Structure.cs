using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Age_Of_Nothing.Sprites
{
    public abstract class Structure : Sprite
    {
        public override Rect Surface { get; }

        protected Structure(Rect rect) : base(rect.Width, rect.Height, () => new Rectangle())
        {
            Surface = rect;

            RefreshVisual(false);
            RefreshPosition();
            Visual.SetValue(Panel.ZIndexProperty, IndexZ);
        }

        protected override int IndexZ => 1;

        protected abstract override Brush DefaultFill { get; }

        protected abstract override Brush HoverFill { get; }
    }
}
