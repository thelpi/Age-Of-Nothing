using System.ComponentModel;
using System.Windows;

namespace Age_Of_Nothing.Sprites
{
    public abstract class Sprite : INotifyPropertyChanged
    {
        private Point _center;

        public Rect Surface { get; private set; }
        public bool CanMove { get; }
        public Point Center
        {
            get => _center;
            private set
            {
                if (_center != value)
                {
                    _center = value;
                    OnPropertyChanged(nameof(Center));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected Sprite(Rect surface, bool canMove)
        {
            Surface = surface;
            CanMove = canMove;
            _center = Surface.GetCenter();
        }

        protected bool Move(Point middlePoint)
        {
            if (!CanMove)
                return false;

            Surface = middlePoint.ComputeSurfaceFromMiddlePoint(Surface.Size);
            Center = middlePoint;
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

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
