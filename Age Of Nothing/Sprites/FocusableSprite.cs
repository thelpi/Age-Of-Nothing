using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using Age_Of_Nothing.Events;

namespace Age_Of_Nothing.Sprites
{
    public abstract class FocusableSprite : Sprite, INotifyPropertyChanged
    {
        protected IEnumerable<FocusableSprite> Sprites { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsCraft { get; }

        protected FocusableSprite(Rect surface, IEnumerable<FocusableSprite> sprites, bool canMove = false, bool isCraft = true)
            : base(surface, canMove)
        {
            IsCraft = isCraft;
            Sprites = sprites;
            _mouseLeftButtonDownHandler = (a, b) =>
            {
                ToggleFocus();
            };
        }

        public void ToggleFocus()
        {
            Focused = !Focused;
            foreach (var x in Sprites)
            {
                if (x != this)
                    x.Focused = false;
            }
        }

        private bool _focused;
        public bool Focused
        {
            get => _focused;
            set
            {
                if (_focused != value)
                {
                    _focused = value;
                    PropertyChanged?.Invoke(this, new SpriteFocusChangedEventArgs());
                }
            }
        }

        protected void NotifyMove()
        {
            PropertyChanged?.Invoke(this, new SpritePositionChangedEventArgs(null));
        }

        protected void NotifyResources()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(ResourcesChanged));
        }

        public const string ResourcesChanged = "ResourcesChanged";
    }
}
