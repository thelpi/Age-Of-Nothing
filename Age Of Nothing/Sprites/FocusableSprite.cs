using System.Collections.Generic;
using System.Windows;

namespace Age_Of_Nothing.Sprites
{
    public abstract class FocusableSprite : Sprite
    {
        private bool _focused;

        protected IEnumerable<FocusableSprite> Sprites { get; }

        public bool Focused
        {
            get => _focused;
            set
            {
                if (_focused != value)
                {
                    _focused = value;
                    OnPropertyChanged(nameof(Focused));
                }
            }
        }

        protected FocusableSprite(Rect surface, IEnumerable<FocusableSprite> sprites, bool canMove, int lifePoints = -1)
            : base(surface, canMove, lifePoints)
        {
            Sprites = sprites;
        }

        /// <summary>
        /// Enable / Disable the focus and disable focus on other sprites if this one is focused.
        /// </summary>
        public void ToggleFocus()
        {
            Focused = !Focused;
            if (Focused)
            {
                foreach (var x in Sprites)
                {
                    if (x != this)
                        x.Focused = false;
                }
            }
        }

        /// <summary>
        /// Simulates the sprite is hovered
        /// </summary>
        public void ForceHover(bool forceHover)
        {
            OnPropertyChanged(forceHover ? HoverPropertyName : UnhoverPropertyName);
        }

        public const string HoverPropertyName = "Hover";
        public const string UnhoverPropertyName = "Unhover";
    }
}
