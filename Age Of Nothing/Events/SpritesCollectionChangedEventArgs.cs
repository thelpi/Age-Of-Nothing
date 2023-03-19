using System.ComponentModel;
using Age_Of_Nothing.Sprites;

namespace Age_Of_Nothing.Events
{
    public class SpritesCollectionChangedEventArgs : PropertyChangedEventArgs
    {
        public const string SpritesCollectionAddPropertyName = "SpritesCollectionAdd";
        public const string SpritesCollectionRemovePropertyName = "SpritesCollectionRemove";

        public Sprite Sprite { get; }

        public Craft Craft { get; }

        public bool IsCraft => Craft != null;

        public SpritesCollectionChangedEventArgs(Sprite sprite, bool add)
            : base(add ? SpritesCollectionAddPropertyName : SpritesCollectionRemovePropertyName)
        {
            Sprite = sprite;
            Craft = null;
        }

        public SpritesCollectionChangedEventArgs(Craft craft, bool add)
            : base(add ? SpritesCollectionAddPropertyName : SpritesCollectionRemovePropertyName)
        {
            Sprite = craft.Target;
            Craft = craft;
        }
    }
}
