using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Age_Of_Nothing.Sprites;

namespace Age_Of_Nothing
{
    public class Controller
    {
        private readonly List<Unit> _units = new List<Unit>(10);
        private readonly List<Mine> _mines = new List<Mine>(10);
        private readonly List<Forest> _forest = new List<Forest>(10);
        private readonly Market _market;
        private readonly List<FocusableSprite> _focusableSprites = new List<FocusableSprite>(100);

        public int Population => _units.Count;
        public int WoodQuantity { get; private set; }
        public int RockQuantity { get; private set; }
        public int IronQuantity { get; private set; }

        public Controller()
        {
            WoodQuantity = 100;
            RockQuantity = 100;
            IronQuantity = 100;

            _units.Add(new Unit(new Point(200, 200), 4, 20, _focusableSprites));
            _units.Add(new Unit(new Point(100, 100), 3, 20, _focusableSprites));
            _units.Add(new Unit(new Point(300, 300), 3, 20, _focusableSprites));

            _mines.Add(new RockMine(100, new Point(400, 120), 1, _focusableSprites));
            _mines.Add(new IronMine(75, new Point(200, 600), 1, _focusableSprites));

            _forest.Add(new Forest(new Rect(700, 200, 300, 100)));

            _market = new Market(new Rect(600, 500, 128, 128));

            _focusableSprites.Add(_units[0]);
            _focusableSprites.Add(_units[1]);
            _focusableSprites.Add(_units[2]);
            _focusableSprites.Add(_mines[0]);
            _focusableSprites.Add(_mines[1]);
        }

        public IEnumerable<UIElement> GetVisualSprites()
        {
            foreach (var unit in _units)
                yield return unit.Visual;
            foreach (var forest in _forest)
                yield return forest.Visual;
            foreach (var mine in _mines)
                yield return mine.Visual;
            yield return _market.Visual;
        }

        public IEnumerable<Action> CheckForMovement()
        {
            foreach (var unit in _units)
            {
                if (unit.CheckForMovement())
                    yield return new Action(() => unit.RefreshPosition());
            }
        }

        public void ClearFocus()
        {
            _focusableSprites.ForEach(x => x.ChangeFocus(false, false));
        }

        public void FocusOnZone(Rect zone)
        {
            var hasUnitSelected = false;
            _units.ForEach(x =>
            {
                if (zone.Contains(x.Center))
                {
                    x.ChangeFocus(true, false);
                    hasUnitSelected = true;
                }
            });

            if (!hasUnitSelected)
            {
                // take the first focus
                foreach (var x in _focusableSprites.Where(x => !(x is Unit)))
                {
                    if (zone.IntersectsWith(x.Surface))
                    {
                        x.ChangeFocus(true, false);
                        break;
                    }
                }
            }
        }

        public void RefreshHover(Rect zone)
        {
            _focusableSprites.ForEach(x => x.RefreshVisual(zone.IntersectsWith(x.Surface)));
        }

        public void SetTargetPositionsOnFocused(Point clickPosition)
        {
            var marketCycle = false;
            var mine = _mines.FirstOrDefault(x => x.Surface.Contains(clickPosition));
            if (mine != null)
            {
                clickPosition = mine.Center;
                marketCycle = true;
            }
            else if (_market.Surface.Contains(clickPosition))
                clickPosition = _market.Center;

            foreach (var unit in _units.Where(x => x.Focused))
            {
                if (marketCycle)
                    unit.SetCycle(clickPosition, _market.Center);
                else
                    unit.SetCycle(clickPosition);
            }
        }
    }
}
