using System.ComponentModel;
using Age_Of_Nothing.Sprites;

namespace Age_Of_Nothing.Events
{
    public class SpritesCollectionChangedEventArgs : PropertyChangedEventArgs
    {
        public const string SpritesCollectionAddPropertyName = "SpritesCollectionAdd";
        public const string SpritesCollectionRemovePropertyName = "SpritesCollectionRemove";

        public Sprite Sprite { get; }

        public bool IsBlueprint { get; }

        public SpritesCollectionChangedEventArgs(Sprite sprite, bool add, bool isBlueprint)
            : base(add ? SpritesCollectionAddPropertyName : SpritesCollectionRemovePropertyName)
        {
            Sprite = sprite;
            IsBlueprint = isBlueprint;
        }
    }
}
