using System;
using System.ComponentModel;
using System.Windows;

namespace Age_Of_Nothing.Events
{
    public class SpritesCollectionChangedEventArgs : PropertyChangedEventArgs
    {
        public const string SpritesCollectionAddPropertyName = "SpritesCollectionAdd";
        public const string SpritesCollectionRemovePropertyName = "SpritesCollectionRemove";
        public const string CraftsCollectionAddPropertyName = "CraftsCollectionAdd";
        public const string CraftsCollectionRemovePropertyName = "CraftsCollectionRemove";

        public Func<UIElement> SpriteVisualRecipe { get; }

        public SpritesCollectionChangedEventArgs(Func<UIElement> spriteVisualRecipe, bool add, bool craft = false)
            : base(add
                  ? (craft ? CraftsCollectionAddPropertyName : SpritesCollectionAddPropertyName)
                  : (craft ? CraftsCollectionRemovePropertyName : SpritesCollectionRemovePropertyName))
        {
            SpriteVisualRecipe = spriteVisualRecipe;
        }
    }
}
