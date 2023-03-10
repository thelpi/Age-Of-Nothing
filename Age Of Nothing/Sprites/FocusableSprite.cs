using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Age_Of_Nothing.Events;

namespace Age_Of_Nothing.Sprites
{
    public abstract class FocusableSprite : Sprite, INotifyPropertyChanged
    {
        protected IEnumerable<FocusableSprite> Sprites { get; }

        private readonly double _hoverBorderRate;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsCraft { get; }

        protected FocusableSprite(Rect surface, Func<Shape> shaper, double hoverBorderRate, IEnumerable<FocusableSprite> sprites, int zIndex = 1, bool canMove = false, bool isCraft = true)
            : base(surface, shaper, zIndex, canMove)
        {
            IsCraft = isCraft;
            Sprites = sprites;
            _hoverBorderRate = hoverBorderRate;
            _mouseLeftButtonDownHandler = (a, b) =>
            {
                ChangeFocus(!Focused, true);
                foreach (var x in Sprites)
                {
                    if (x != this)
                        x.ChangeFocus(false, false);
                }
            };
        }

        private bool _focused;
        public bool Focused
        {
            get => _focused;
            private set
            {
                _focused = value;
                PropertyChanged?.Invoke(this, new SpriteFocusChangedEventArgs());
            }
        }

        public override void RefreshVisual(bool hover)
        {
            if (Focused)
            {
                GetVisual().Fill = hover
                    ? GetFocusBrush(HoverFill)
                    : GetFocusBrush(DefaultFill);
            }
            else
            {
                // TODO: refacto?
                GetVisual().Fill = new SolidColorBrush(hover ? HoverFill : DefaultFill);
            }
            RefreshToolTip();
        }

        public void ChangeFocus(bool focus, bool hover)
        {
            Focused = focus;
            RefreshVisual(hover);
        }

        private Brush GetFocusBrush(Color color)
        {
            return new RadialGradientBrush(
                new GradientStopCollection(new List<GradientStop>
                {
                    new GradientStop(color, _hoverBorderRate),
                    new GradientStop(Colors.PaleVioletRed, 1)
                }));
        }
    }
}
