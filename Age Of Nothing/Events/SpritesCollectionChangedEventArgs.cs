using System;
using System.ComponentModel;
using System.Windows;

namespace Age_Of_Nothing.Events
{
    public class SpritesCollectionChangedEventArgs : PropertyChangedEventArgs
    {
        public const string SpritesCollectionAddPropertyName = "SpritesCollectionAdd";
        public const string SpritesCollectionRemovePropertyName = "SpritesCollectionRemove";

        public Func<UIElement> SpriteVisualRecipe { get; }

        public SpritesCollectionChangedEventArgs(Func<UIElement> spriteVisualRecipe, bool add)
            : base(add ? SpritesCollectionAddPropertyName : SpritesCollectionRemovePropertyName)
        {
            SpriteVisualRecipe = spriteVisualRecipe;
        }
    }
}
