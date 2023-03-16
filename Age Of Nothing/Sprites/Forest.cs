using System.Collections.Generic;
using System.Windows;

namespace Age_Of_Nothing.Sprites
{
    public class Forest : Resource
    {
        private const int _size = 30;
        private const int _quantity = 100;

        public override ResourceTypes ResourceType => ResourceTypes.Wood;

        public int ForestPatchIndex { get; }

        private Forest(Point center, int forestPatchIndex, IEnumerable<FocusableSprite> sprites)
            : base(_quantity, center, _size, sprites)
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
        public static IEnumerable<Forest> GenerateForestPatch(Rect patchSurface, IEnumerable<FocusableSprite> sprites, int forestPatchIndex)
        {
            for (var i = patchSurface.X; i < patchSurface.BottomRight.X; i += _size)
            {
                for (var j = patchSurface.Y; j < patchSurface.BottomRight.Y; j += _size)
                {
                    yield return new Forest(new Point(i + _size / (double)2, j + _size / (double)2), forestPatchIndex, sprites);
                }
            }
        }
    }
}
