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
        private readonly List<CenteredSprite> _sprites = new List<CenteredSprite>(100);

        public Controller()
        {
            _units.Add(new Unit(new Point(200, 200), 4, 20, _sprites));
            _units.Add(new Unit(new Point(100, 100), 3, 20, _sprites));
            _units.Add(new Unit(new Point(300, 300), 3, 20, _sprites));

            _mines.Add(new Mine(100, new Point(400, 120), 1, false, _sprites));
            _mines.Add(new Mine(75, new Point(200, 600), 1, true, _sprites));

            _forest.Add(new Forest(new Rect(700, 200, 300, 100)));

            _market = new Market(new Rect(600, 500, 128, 128));

            _sprites.Add(_units[0]);
            _sprites.Add(_units[1]);
            _sprites.Add(_units[2]);
            _sprites.Add(_mines[0]);
            _sprites.Add(_mines[1]);
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
            _units.ForEach(x => x.ChangeFocus(false, false));
        }

        public void FocusOnZone(Rect zone)
        {
            _units.ForEach(x =>
            {
                if (zone.Contains(x.Position))
                    x.ChangeFocus(true, false);
            });
        }

        public void RefreshHover(Rect zone)
        {
            _units.ForEach(x => x.RefreshVisual(zone.Contains(x.Position)));
        }

        public void SetTargetPositionsOnFocused(Point clickPosition)
        {
            var mine = _mines.FirstOrDefault(x => x.Surface.Contains(clickPosition));
            if (mine != null)
                clickPosition = mine.Position;
            else if (_market.Surface.Contains(clickPosition))
                clickPosition = _market.Position;

            foreach (var unit in _units.Where(x => x.Focused))
                unit.TargetPosition = clickPosition;
        }
    }
}
