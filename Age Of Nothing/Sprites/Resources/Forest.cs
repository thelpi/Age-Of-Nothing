using System.Collections.Generic;
using System.Windows;
using Age_Of_Nothing.Sprites.Attributes;

namespace Age_Of_Nothing.Sprites.Resources
{
    [Dimensions(30)]
    public class Forest : Resource
    {
        private const int _quantity = 100;

        public override ResourceTypes ResourceType => ResourceTypes.Wood;

        public int ForestPatchIndex { get; }

        private Forest(Point center, int forestPatchIndex, IEnumerable<Sprite> sprites)
            : base(center, _quantity, sprites)
        {
            ForestPatchIndex = forestPatchIndex;
        }

        /// <summary>
        /// Sets a collection of forest sprites to creates a bigger rectangle of forest (a "patch")
        /// </summary>
        /// <param name="patchSurface">The patch surface</param>
        /// <param name="sprites">Every focusable sprite</param>
        /// <param name="forestPatchIndex">The forest patch index</param>
        /// <returns></returns>
        public static IEnumerable<Forest> GenerateForestPatch(Rect patchSurface, IEnumerable<Sprite> sprites, int forestPatchIndex)
        {
            var size = GetSpriteSize(typeof(Forest));
            for (var i = patchSurface.X; i < patchSurface.BottomRight.X; i += size.Width)
            {
                for (var j = patchSurface.Y; j < patchSurface.BottomRight.Y; j += size.Height)
                {
                    yield return new Forest(new Point(i + size.Width / 2, j + size.Height / 2), forestPatchIndex, sprites);
                }
            }
        }
    }
}
