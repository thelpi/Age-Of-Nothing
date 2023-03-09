using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Age_Of_Nothing.Sprites;

namespace Age_Of_Nothing
{
    public class Controller : INotifyPropertyChanged
    {
        private readonly Dictionary<PrimaryResources, int> _resourcesQty;
        private readonly List<Unit> _units = new List<Unit>(10);
        private readonly List<Mine> _mines = new List<Mine>(10);
        private readonly List<Forest> _forest = new List<Forest>(10);
        private Market _market;
        private readonly List<FocusableSprite> _focusableSprites = new List<FocusableSprite>(100);
        private readonly List<Dwelling> _dwellings = new List<Dwelling>();

        private const int MaxVillagerCreationStack = 20;

        private int _villagerCreationStack;
        private int _currentVillagerCreationTicks;

        public event PropertyChangedEventHandler PropertyChanged;

        private string _populationInformation;

        // TODO: everytime a dwelling or unit is added or removed!
        public string PopulationInformation
        {
            get { return _populationInformation; }
            private set
            {
                _populationInformation = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PopulationInformation)));
            }
        }

        public int PotentialPopulation => _dwellings.Count * Dwelling.VillagerCapacity;
        public int Population => _units.Count;
        public int WoodQuantity => _resourcesQty[PrimaryResources.Wood];
        public int RockQuantity => _resourcesQty[PrimaryResources.Rock];
        public int IronQuantity => _resourcesQty[PrimaryResources.Iron];

        public bool HasVillagerFocus()
        {
            return _units.Any(x => x.Is<Villager>() && x.Focused);
        }

        public bool HasMarketFocus()
        {
            return _market?.Focused == true;
        }

        public Controller()
        {
            _resourcesQty = new Dictionary<PrimaryResources, int>
            {
                { PrimaryResources.Iron, 100 },
                { PrimaryResources.Rock, 100 },
                { PrimaryResources.Wood, 100 }
            };

            _units.Add(new Villager(new Point(200, 200), _focusableSprites));
            _units.Add(new Villager(new Point(100, 100), _focusableSprites));
            _units.Add(new Villager(new Point(300, 300), _focusableSprites));
            SetPopulationInformation();

            _mines.Add(new RockMine(100, new Point(400, 120), 1, _focusableSprites));
            _mines.Add(new IronMine(75, new Point(200, 600), 1, _focusableSprites));

            _forest.Add(new Forest(new Rect(700, 200, 300, 100)));

            _market = new Market(new Point(600, 500), _focusableSprites);

            _dwellings.Add(new Dwelling(new Point(1100, 10), _focusableSprites));
            _dwellings.Add(new Dwelling(new Point(1100, 90), _focusableSprites));
            SetPopulationInformation();

            _focusableSprites.Add(_units[0]);
            _focusableSprites.Add(_units[1]);
            _focusableSprites.Add(_units[2]);
            _focusableSprites.Add(_mines[0]);
            _focusableSprites.Add(_mines[1]);
            _focusableSprites.Add(_market);
            _focusableSprites.Add(_dwellings[0]);
            _focusableSprites.Add(_dwellings[1]);

            _focusableSprites.ForEach(fs => fs.PropertyChanged += (s, e) => PropertyChanged?.Invoke(this, e));
        }

        private void SetPopulationInformation()
        {
            PopulationInformation = $"{Population} / {PotentialPopulation}";
        }

        public IEnumerable<UIElement> GetVisualSprites()
        {
            foreach (var focusableSprite in _focusableSprites)
                yield return focusableSprite.GetVisual();
            foreach (var forest in _forest)
                yield return forest.GetVisual();
        }

        internal void AddVillagerCreationToStack()
        {
            if (_villagerCreationStack < MaxVillagerCreationStack)
                _villagerCreationStack++;
        }

        public Func<UIElement> CheckForVillagerCreation()
        {
            if (Population >= PotentialPopulation || _market == null)
            {
                // TODO: notify the game of population limit
                return null;
            }

            if (_villagerCreationStack == 0)
            {
                return null;
            }

            _currentVillagerCreationTicks++;
            if (_currentVillagerCreationTicks < Villager.BuildTimeTick)
                return null;

            _villagerCreationStack--;
            _currentVillagerCreationTicks = 0;

            var v = new Villager(_market.Center, _focusableSprites);

            _units.Add(v);
            SetPopulationInformation();

            _focusableSprites.Add(v);

            return v.GetVisual;
        }

        public UIElement CheckForDeletion()
        {
            var sprite = _focusableSprites.FirstOrDefault(x => x.Focused && x.IsHomeMade);
            if (sprite == null)
                return null;

            _focusableSprites.Remove(sprite);
            sprite.ChangeFocus(false, false);
            if (sprite.Is<Unit>(out var unit))
                _units.Remove(unit);
            else if (sprite.Is<Market>())
                _market = null;
            else if (sprite.Is<Dwelling>(out var dwl))
                _dwellings.Remove(dwl);
            SetPopulationInformation();

            return sprite.GetVisual();
        }

        public IEnumerable<Action> CheckForMovement()
        {
            foreach (var unit in _units)
            {
                var (move, tgt) = unit.CheckForMovement();
                if (tgt != null && unit.Is<Villager>(out var villager))
                {
                    var carry = villager.CheckCarry(tgt);
                    if (carry.HasValue)
                    {
                        _resourcesQty[carry.Value.r] += carry.Value.v;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"{carry.Value.r}Quantity"));
                    }
                }
                if (move)
                    yield return unit.RefreshPosition;
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
            Sprite tgt = null;
            var marketCycle = false;
            var mine = _mines.FirstOrDefault(x => x.Surface.Contains(clickPosition));
            if (mine != null)
            {
                clickPosition = mine.Center;
                marketCycle = true;
                tgt = mine;
            }
            else if (_market != null && _market.Surface.Contains(clickPosition))
            {
                clickPosition = _market.Center;
                tgt = _market;
            }
            else
            {
                var forest = _forest.FirstOrDefault(x => x.Surface.Contains(clickPosition));
                if (forest != null)
                {
                    marketCycle = true;
                    tgt = forest;
                    // TODO: find the proper target to be on the border of the forest (closest to current position)
                }
            }

            foreach (var unit in _units.Where(x => x.Focused))
            {
                if (marketCycle && _market != null)
                    unit.SetCycle((clickPosition, tgt), (_market.Center, _market));
                else
                    unit.SetCycle((clickPosition, tgt));
            }
        }
    }
}
