﻿using System;
using System.ComponentModel;
using System.Windows;
using Age_Of_Nothing.Sprites.Attributes;

namespace Age_Of_Nothing.Sprites
{
    public abstract class Sprite : INotifyPropertyChanged
    {
        private bool _focused;
        private Point _center;
        private int _lifePoints;

        protected readonly Controller Parent;

        public bool HasLifePoints => LifePoints > -1;

        public int Team { get; }
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected Sprite(Point basePoint, bool isCenter, bool canMove, Controller parent, int team)
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

            Parent = parent;
            Team = team;
        }

        public void TakeDamage(int damagePoints)
        {
            if (LifePoints > 0)
                LifePoints -= LifePoints < damagePoints ? LifePoints : damagePoints;
        }

        protected bool Move(Rect surface)
        {
            if (!CanMove)
                return false;

            Surface = surface;
            Center = surface.GetCenter();
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

        public static Size GetSpriteSize(Type t)
        {
            return t.GetAttribute<DimensionsAttribute>()?.Size ?? Size.Empty;
        }

        public static int GetDefaultLifePoints(Type t)
        {
            return t.GetAttribute<LifePointsAttribute>()?.LifePoints ?? -1;
        }

        /// <summary>
        /// Enable / Disable the focus and disable focus on other sprites if this one is focused.
        /// </summary>
        public void ToggleFocus()
        {
            Focused = !Focused;
            if (Focused)
            {
                foreach (var x in Parent.Sprites)
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
