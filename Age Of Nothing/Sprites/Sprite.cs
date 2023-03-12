using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Age_Of_Nothing.Sprites
{
    public abstract class Sprite
    {
        protected abstract Color DefaultFill { get; }
        protected abstract Color HoverFill { get; }
        public Rect Surface { get; private set; }
        public bool CanMove { get; }
        protected virtual string Info { get; }

        protected MouseButtonEventHandler _mouseLeftButtonDownHandler;

        protected Sprite(Rect surface, Func<Shape> shaper, int zIndex = 1, bool canMove = false)
        {
            Surface = surface;
            CanMove = canMove;
            _shaper = shaper;
            _indexZ = zIndex;
        }

        public void RefreshPosition()
        {
            GetVisual().SetValue(Canvas.LeftProperty, Surface.Left);
            GetVisual().SetValue(Canvas.TopProperty, Surface.Top);
        }

        private Shape _visual = null;
        private Shape _shadowVisual = null;

        private readonly Func<Shape> _shaper;
        private readonly int _indexZ;

        public Shape GetVisual()
        {
            if (_visual == null)
            {
                _visual = _shaper();
                _visual.Width = Surface.Width;
                _visual.Height = Surface.Height;
                _visual.MouseEnter += (a, b) => RefreshVisual(true);
                _visual.MouseLeave += (a, b) => RefreshVisual(false);
                if (_mouseLeftButtonDownHandler != null)
                    _visual.MouseLeftButtonDown += _mouseLeftButtonDownHandler;

                RefreshVisual(false);
                RefreshPosition();
                _visual.SetValue(Panel.ZIndexProperty, _indexZ);
            }
            return _visual;
        }

        public Shape GetShadowVisual()
        {
            if (_shadowVisual == null)
            {
                _shadowVisual = _shaper();
                _shadowVisual.Width = Surface.Width;
                _shadowVisual.Height = Surface.Height;
                _shadowVisual.Fill = new SolidColorBrush(DefaultFill);
                _shadowVisual.SetValue(Canvas.LeftProperty, Surface.Left);
                _shadowVisual.SetValue(Canvas.TopProperty, Surface.Top);
                _shadowVisual.SetValue(Panel.ZIndexProperty, _indexZ);
                _shadowVisual.Opacity = 0.5;
            }
            return _shadowVisual;
        }

        public virtual void RefreshVisual(bool hover)
        {
            RefreshVisual(new SolidColorBrush(hover ? HoverFill : DefaultFill));
        }

        protected void RefreshVisual(Brush brush)
        {
            GetVisual().Fill = brush;
            RefreshToolTip();
        }

        protected void RefreshToolTip()
        {
            if (Info != null)
                GetVisual().ToolTip = Info;
        }

        protected bool Move(Point topLeftPoint)
        {
            if (!CanMove)
                return false;

            Surface = new Rect(topLeftPoint, Surface.Size);
            return true;
        }

        public bool Is<T>() where T : class
        {
            return Is<T>(out _);
        }

        public bool Is<T>(out T data) where T : class
        {
            var isType = typeof(T).IsAssignableFrom(GetType());
            data = isType ? this as T : default;
            return isType;
        }
    }
}
