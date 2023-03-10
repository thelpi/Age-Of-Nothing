using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Age_Of_Nothing.Sprites;

namespace Age_Of_Nothing
{
    public class Controller : INotifyPropertyChanged
    {
        private const int MaxVillagerCreationStack = 20;

        private readonly ObservableCollection<Sprite> _sprites = new ObservableCollection<Sprite>();
        private readonly Dictionary<PrimaryResources, int> _resourcesQty;

        private int _villagerCreationStack;
        private int _currentVillagerCreationTicks;
        private string _populationInformation;

        private IEnumerable<Unit> _units => _sprites.OfType<Unit>();
        private IEnumerable<Villager> _villagers => _sprites.OfType<Villager>();
        private IEnumerable<Mine> _mines => _sprites.OfType<Mine>();
        private IEnumerable<Forest> _forests => _sprites.OfType<Forest>();
        private Market _market => _sprites.OfType<Market>().FirstOrDefault();
        private IEnumerable<Dwelling> _dwellings => _sprites.OfType<Dwelling>();
        private IEnumerable<FocusableSprite> _focusables => _sprites.OfType<FocusableSprite>();

        public string PopulationInformation
        {
            get { return _populationInformation; }
            private set
            {
                _populationInformation = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PopulationInformation)));
            }
        }
        public int PotentialPopulation => _dwellings.Count() * Dwelling.VillagerCapacity;
        public int Population => _units.Count();
        public int WoodQuantity => _resourcesQty[PrimaryResources.Wood];
        public int RockQuantity => _resourcesQty[PrimaryResources.Rock];
        public int IronQuantity => _resourcesQty[PrimaryResources.Iron];

        public event PropertyChangedEventHandler PropertyChanged;

        public Controller()
        {
            _resourcesQty = new Dictionary<PrimaryResources, int>
            {
                { PrimaryResources.Iron, 100 },
                { PrimaryResources.Rock, 100 },
                { PrimaryResources.Wood, 100 }
            };

            _sprites.CollectionChanged += (x, y) =>
            {
                // TODO: we should do this logic into view with a converter
                PopulationInformation = $"{Population} / {PotentialPopulation}";
            };

            _sprites.Add(new Villager(new Point(200, 200), _focusables));
            _sprites.Add(new Villager(new Point(100, 100), _focusables));
            _sprites.Add(new Villager(new Point(300, 300), _focusables));
            _sprites.Add(new RockMine(100, new Point(400, 120), 1, _focusables));
            _sprites.Add(new IronMine(75, new Point(200, 600), 1, _focusables));
            _sprites.Add(new Forest(new Rect(700, 200, 300, 100)));
            _sprites.Add(new Market(new Point(600, 500), _focusables));
            _sprites.Add(new Dwelling(new Point(1100, 10), _focusables));
            _sprites.Add(new Dwelling(new Point(1100, 90), _focusables));

            foreach (var fs in _focusables)
                fs.PropertyChanged += (s, e) => PropertyChanged?.Invoke(this, e);
        }

        public bool HasVillagerFocus()
        {
            return _villagers.Any(x => x.Focused);
        }

        public bool HasMarketFocus()
        {
            return _market?.Focused == true;
        }

        public IEnumerable<UIElement> GetVisualSprites()
        {
            foreach (var sprite in _sprites)
                yield return sprite.GetVisual();
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

            var v = new Villager(_market.Center, _focusables);

            _sprites.Add(v);

            return v.GetVisual;
        }

        public UIElement CheckForDeletion()
        {
            var sprite = _focusables.FirstOrDefault(x => x.Focused && x.IsHomeMade);
            if (sprite == null)
                return null;

            _sprites.Remove(sprite);

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
            foreach (var sp in _focusables)
                sp.ChangeFocus(false, false);
        }

        public void FocusOnZone(Rect zone)
        {
            var hasUnitSelected = false;
            foreach (var unit in _units)
            {
                if (zone.Contains(unit.Center))
                {
                    unit.ChangeFocus(true, false);
                    hasUnitSelected = true;
                }
            }

            if (!hasUnitSelected)
            {
                // take the first focus
                foreach (var x in _focusables.Where(x => !(x is Unit)))
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
            foreach (var sp in _focusables)
                sp.RefreshVisual(zone.IntersectsWith(sp.Surface));
        }

        public void SetTargetPositionsOnFocused(Point clickPosition)
        {
            Point? villagerOverrideClickPosition = null;
            Sprite tgt = null;
            var marketCycle = false;
            var mine = _mines.FirstOrDefault(x => x.Surface.Contains(clickPosition));
            if (mine != null)
            {
                villagerOverrideClickPosition = mine.Center;
                marketCycle = true;
                tgt = mine;
            }
            else if (_market != null && _market.Surface.Contains(clickPosition))
            {
                villagerOverrideClickPosition = _market.Center;
                tgt = _market;
            }
            else
            {
                var forest = _forests.FirstOrDefault(x => x.Surface.Contains(clickPosition));
                if (forest != null)
                {
                    marketCycle = true;
                    tgt = forest;
                    // TODO: find the proper target to be on the border of the forest (closest to current position)
                }
            }

            foreach (var unit in _units.Where(x => x.Focused))
            {
                if (unit.Is<Villager>())
                {
                    if (marketCycle && _market != null)
                        unit.SetCycle((villagerOverrideClickPosition.GetValueOrDefault(clickPosition), tgt), (_market.Center, _market));
                    else
                        unit.SetCycle((villagerOverrideClickPosition.GetValueOrDefault(clickPosition), tgt));
                }
                else
                    unit.SetCycle((clickPosition, tgt));
            }
        }
    }
}
