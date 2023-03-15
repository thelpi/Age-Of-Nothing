using System.Windows;
using System.Windows.Input;

namespace Age_Of_Nothing.Sprites
{
    public abstract class Sprite
    {
        public Rect Surface { get; private set; }
        public bool CanMove { get; }
        protected virtual string Info { get; }

        protected MouseButtonEventHandler _mouseLeftButtonDownHandler;

        protected Sprite(Rect surface, bool canMove = false)
        {
            Surface = surface;
            CanMove = canMove;
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
