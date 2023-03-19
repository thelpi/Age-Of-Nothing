using System.ComponentModel;
using Age_Of_Nothing.Sprites;

namespace Age_Of_Nothing.Events
{
    public class SpritesCollectionChangedEventArgs : PropertyChangedEventArgs
    {
        public const string SpritesCollectionAddPropertyName = "SpritesCollectionAdd";
        public const string SpritesCollectionRemovePropertyName = "SpritesCollectionRemove";

        public Sprite Sprite { get; }

        public bool IsCraft { get; }

        public SpritesCollectionChangedEventArgs(Sprite sprite, bool add, bool isCraft)
            : base(add ? SpritesCollectionAddPropertyName : SpritesCollectionRemovePropertyName)
        {
            Sprite = sprite;
            IsCraft = isCraft;
        }
    }
}
