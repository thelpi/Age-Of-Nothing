using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Age_Of_Nothing.Sprites
{
    public class Forest : Resource
    {
        private const int _size = 30;
        private const int _quantity = 100;

        private static readonly double _scale = _size / (double)_quantity;

        public override ResourceTypes ResourceType => ResourceTypes.Wood;

        protected override string Info => $"{Quantity}";

        public int ForestPatchIndex { get; }

        private Forest(Point center, int forestPatchIndex, IEnumerable<FocusableSprite> sprites)
            : base(_quantity, center, _scale, sprites)
        {
            ForestPatchIndex = forestPatchIndex;
        }

        protected override Color DefaultFill => Colors.Green;

        protected override Color HoverFill => Colors.ForestGreen;

        public static IEnumerable<Forest> GenerateForestRectangle(Rect rect, IEnumerable<FocusableSprite> sprites, int forestPatchIndex)
        {
            for (var i = rect.X; i < rect.BottomRight.X; i += _size)
            {
                for (var j = rect.Y; j < rect.BottomRight.Y; j += _size)
                {
                    yield return new Forest(new Point(i + _size / (double)2, j + _size / (double)2), forestPatchIndex, sprites);
                }
            }
        }
    }
}
