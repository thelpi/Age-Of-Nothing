using System.ComponentModel;
using System.Windows;
using Age_Of_Nothing.Sprites.Attributes;

namespace Age_Of_Nothing.Sprites
{
    public abstract class Sprite : INotifyPropertyChanged
    {
        private Point _center;
        private int _lifePoints;

        public bool HasLifePoints => LifePoints > -1;

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
        public int LifePoints
        {
            get => _lifePoints;
            private set
            {
                if (_lifePoints != value)
                {
                    _lifePoints = value;
                    OnPropertyChanged(nameof(LifePoints));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected Sprite(Point basePoint, bool isCenter, bool canMove)
        {
            var size = GetSpriteSize(GetType());

            Surface = isCenter
                ? basePoint.ComputeSurfaceFromMiddlePoint(size)
                : new Rect(basePoint, size);
            CanMove = canMove;
            _center = isCenter
                ? basePoint
                : Surface.GetCenter();
            _lifePoints = GetDefaultLifePoints(GetType());
        }

        public void TakeDamage(int damagePoints)
        {
            if (LifePoints > 0)
                LifePoints -= LifePoints < damagePoints ? LifePoints : damagePoints;
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

        public (int gold, int wood, int rock) GetResourcesCost()
        {
            var r = GetType().GetAttribute<ResourcesCostAttribute>();
            return r == null ? (0, 0, 0) : (r.Gold, r.Wood, r.Rock);
        }

        public int GetCraftTime()
        {
            return GetType().GetAttribute<CraftTimeAttribute>()?.FramesCount ?? 0;
        }

        public static Size GetSpriteSize(System.Type t)
        {
            return t.GetAttribute<DimensionsAttribute>()?.Size ?? Size.Empty;
        }

        public static int GetDefaultLifePoints(System.Type t)
        {
            return t.GetAttribute<LifePointsAttribute>()?.LifePoints ?? -1;
        }
    }
}
