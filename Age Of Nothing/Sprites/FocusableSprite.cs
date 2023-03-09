using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Age_Of_Nothing.Sprites
{
    public abstract class FocusableSprite : Sprite, INotifyPropertyChanged
    {
        protected IReadOnlyList<FocusableSprite> Sprites { get; }

        private readonly double _hoverBorderRate;

        public event PropertyChangedEventHandler PropertyChanged;

        protected FocusableSprite(Rect surface, Func<Shape> shaper, double hoverBorderRate, IReadOnlyList<FocusableSprite> sprites, int zIndex = 1, bool canMove = false)
            : base(surface, shaper, zIndex, canMove)
        {
            Sprites = sprites;
            _hoverBorderRate = hoverBorderRate;
            Visual.MouseLeftButtonDown += (a, b) =>
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
            get { return _focused; }
            private set
            {
                _focused = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Focused)));
            }
        }

        public override void RefreshVisual(bool hover)
        {
            if (Focused)
            {
                Visual.Fill = hover
                    ? GetFocusBrush(HoverFill)
                    : GetFocusBrush(DefaultFill);
            }
            else
            {
                // TODO: refacto?
                Visual.Fill = new SolidColorBrush(hover ? HoverFill : DefaultFill);
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
