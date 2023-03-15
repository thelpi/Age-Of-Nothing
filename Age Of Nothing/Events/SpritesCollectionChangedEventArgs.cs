using System;
using System.ComponentModel;
using System.Windows;
using Age_Of_Nothing.Sprites;

namespace Age_Of_Nothing.Events
{
    public class SpritesCollectionChangedEventArgs : PropertyChangedEventArgs
    {
        public const string SpritesCollectionAddPropertyName = "SpritesCollectionAdd";
        public const string SpritesCollectionRemovePropertyName = "SpritesCollectionRemove";
        public const string CraftsCollectionAddPropertyName = "CraftsCollectionAdd";
        public const string CraftsCollectionRemovePropertyName = "CraftsCollectionRemove";

        public Func<UIElement> SpriteVisualRecipe { get; }

        public Sprite Sprite { get; }

        public bool IsBlueprint { get; set; }

        public SpritesCollectionChangedEventArgs(Func<UIElement> spriteVisualRecipe, bool add, bool isBlueprint = false)
            : base(add
                  ? (isBlueprint ? CraftsCollectionAddPropertyName : SpritesCollectionAddPropertyName)
                  : (isBlueprint ? CraftsCollectionRemovePropertyName : SpritesCollectionRemovePropertyName))
        {
            SpriteVisualRecipe = spriteVisualRecipe;
            IsBlueprint = isBlueprint;
        }

        public SpritesCollectionChangedEventArgs(Sprite sprite, bool add, bool isBlueprint = false)
            : base(add
                  ? (isBlueprint ? CraftsCollectionAddPropertyName : SpritesCollectionAddPropertyName)
                  : (isBlueprint ? CraftsCollectionRemovePropertyName : SpritesCollectionRemovePropertyName))
        {
            Sprite = sprite;
            IsBlueprint = isBlueprint;
        }
    }
}
