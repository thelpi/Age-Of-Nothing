using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Age_Of_Nothing.Events;
using Age_Of_Nothing.Sprites;
using Age_Of_Nothing.Sprites.Attributes;

namespace Age_Of_Nothing
{
    public class Controller : INotifyPropertyChanged
    {
        private readonly ObservableCollection<Sprite> _sprites = new ObservableCollection<Sprite>();
        private readonly Dictionary<ResourceTypes, int> _resourcesQty;
        private readonly ObservableCollection<Craft> _craftQueue = new ObservableCollection<Craft>();
        private readonly List<List<Forest>> _forestPatchs = new List<List<Forest>>();

        private int _frames;

        private IEnumerable<Sprite> _nonUnits => _sprites.Except(_units);
        private IEnumerable<Unit> _units => _sprites.OfType<Unit>();
        private IEnumerable<Villager> _villagers => _sprites.OfType<Villager>();
        private IEnumerable<Market> _markets => _sprites.OfType<Market>();
        private IEnumerable<Structure> _structures => _sprites.OfType<Structure>();
        private IEnumerable<FocusableSprite> _focusables => _sprites.OfType<FocusableSprite>();

        public int PotentialPopulation => _structures.Sum(x => x.GetUnitsStorage());
        public int Population => _units.Count();
        public int WoodQuantity => _resourcesQty[ResourceTypes.Wood];
        public int RockQuantity => _resourcesQty[ResourceTypes.Rock];
        public int GoldQuantity => _resourcesQty[ResourceTypes.Gold];

        public event PropertyChangedEventHandler PropertyChanged;

        public Controller()
        {
            _resourcesQty = new Dictionary<ResourceTypes, int>
            {
                { ResourceTypes.Gold, 10000 },
                { ResourceTypes.Rock, 10000 },
                { ResourceTypes.Wood, 10000 }
            };

            _sprites.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (var sprite in e.NewItems.OfType<Sprite>())
                    {
                        PropertyChanged?.Invoke(this, new SpritesCollectionChangedEventArgs(sprite, true, false));
                        sprite.PropertyChanged += (s, e) =>
                        {
                            if (e.PropertyName == nameof(sprite.LifePoints))
                            {
                                if (sprite.LifePoints == 0)
                                    _sprites.Remove(sprite);
                            }
                            // To be clear: when a sprite trigger a property changed, the controller propagates the same event
                            PropertyChanged?.Invoke(this, e);
                        };
                    }
                }

                if (e.OldItems != null)
                {
                    foreach (var item in e.OldItems.OfType<Sprite>())
                        PropertyChanged?.Invoke(this, new SpritesCollectionChangedEventArgs(item, false, false));
                }

                // in case of dwellings or units count has changed
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Population)));
            };

            _craftQueue.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (var craft in e.NewItems.OfType<Craft>())
                        PropertyChanged?.Invoke(this, new SpritesCollectionChangedEventArgs(craft.Target, true, true));
                }

                if (e.OldItems != null)
                {
                    foreach (var craft in e.OldItems.OfType<Craft>())
                        PropertyChanged?.Invoke(this, new SpritesCollectionChangedEventArgs(craft.Target, false, true));
                }
            };
        }

        public void Initialize()
        {
            _sprites.Add(new Villager(new Point(200, 200), _focusables));
            _sprites.Add(new Villager(new Point(100, 100), _focusables));
            _sprites.Add(new Villager(new Point(300, 300), _focusables));
            _sprites.Add(new RockMine(100, new Point(400, 120), _focusables));
            _sprites.Add(new GoldMine(75, new Point(200, 600), _focusables));
            _sprites.Add(new Market(new Point(600, 500), _focusables));
            _sprites.Add(new Dwelling(new Point(1100, 10), _focusables));
            _sprites.Add(new Dwelling(new Point(1100, 90), _focusables));

            var forests = Forest.GenerateForestPatch(new Rect(700, 200, 300, 100), _focusables, 0);
            _forestPatchs.Add(forests.ToList());
            foreach (var forest in _forestPatchs.Last())
                _sprites.Add(forest);
        }

        public bool HasVillagerFocus => _villagers.Any(x => x.Focused);

        public bool HasMarketFocus => _markets.Any(x => x.Focused);

        public bool HasBarracksFocus => _structures.Any(x => x.Is<Barracks>() && x.Focused);

        public void BuildDwelling(Point center)
        {
            BuildStructure(center, (a, b) => new Dwelling(a, b));
        }

        public void BuildMarket(Point center)
        {
            BuildStructure(center, (a, b) => new Market(a, b));
        }

        public void BuildBarracks(Point center)
        {
            BuildStructure(center, (a, b) => new Barracks(a, b));
        }

        public void AddVillagerCreationToStack()
        {
            lock (_craftQueue)
            {
                if (_markets.FirstIfNotNull(x => x.Focused, out var focusMarket))
                {
                    var v = new Villager(focusMarket.Center, _focusables);
                    _craftQueue.Add(new Craft(focusMarket, v, v.GetCraftTime()));
                }
            }
        }

        public void AddSwordsmanCreationToStack()
        {
            lock (_craftQueue)
            {
                if (_structures.FirstIfNotNull(x => x.Is<Barracks>() && x.Focused, out var focusBarracks))
                {
                    var s = new Swordsman(focusBarracks.Center, _focusables);
                    _craftQueue.Add(new Craft(focusBarracks, s, s.GetCraftTime()));
                }
            }
        }

        public void AddArcherCreationToStack()
        {
            lock (_craftQueue)
            {
                if (_structures.FirstIfNotNull(x => x.Is<Barracks>() && x.Focused, out var focusBarracks))
                {
                    var s = new Archer(focusBarracks.Center, _focusables);
                    _craftQueue.Add(new Craft(focusBarracks, s, s.GetCraftTime()));
                }
            }
        }

        public void AddKnightCreationToStack()
        {
            lock (_craftQueue)
            {
                if (_structures.FirstIfNotNull(x => x.Is<Barracks>() && x.Focused, out var focusBarracks))
                {
                    var s = new Knight(focusBarracks.Center, _focusables);
                    _craftQueue.Add(new Craft(focusBarracks, s, s.GetCraftTime()));
                }
            }
        }

        public void CheckForDeletion()
        {
            lock (_sprites)
            {
                var sprite = _focusables.FirstOrDefault(x => x.Focused && x.HasLifePoints);
                if (sprite != null)
                    sprite.TakeDamage(int.MaxValue);
            }
        }

        public void NewFrameCheck()
        {
            _frames++;
            lock (_sprites)
            {
                ManageUnitsMovements();
                lock (_craftQueue)
                {
                    ManageCraftsToCancel();
                    ManageCraftsInProgress();
                }
            }
        }

        public void ClearFocus()
        {
            foreach (var sp in _focusables)
                sp.Focused = false;
        }

        public void FocusOnZone(Rect zone)
        {
            var hasUnitSelected = false;
            foreach (var unit in _units)
            {
                if (zone.Contains(unit.Center))
                {
                    unit.Focused = true;
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
                        x.Focused = true;
                        break;
                    }
                }
            }
        }

        public void RefreshHover(Rect zone)
        {
            foreach (var sp in _focusables)
                sp.ForceHover(zone.IntersectsWith(sp.Surface));
        }

        public void SetTargetPositionsOnFocused(Point clickPosition)
        {
            var targets = _sprites.Where(x => x.Surface.Contains(clickPosition));

            foreach (var unit in _units.Where(x => x.Focused))
                unit.ComputeCycle(clickPosition, targets);
        }

        private void BuildStructure<T>(Point center, System.Func<Point, IEnumerable<FocusableSprite>, T> ctor)
            where T : Structure
        {
            lock (_craftQueue)
            {
                var surface = center.ComputeSurfaceFromMiddlePoint(Sprite.GetSpriteSize(typeof(T)));
                // TODO: surface should be inside the game area entirely
                if (!SurfaceIsEngaged(surface))
                {
                    var villagerFocused = _villagers.Where(x => x.Focused);
                    if (villagerFocused.Any())
                    {
                        var sprite = ctor(surface.TopLeft, _focusables);
                        if (CheckStructureResources(sprite))
                        {
                            _craftQueue.Add(new Craft(villagerFocused.Cast<Sprite>().ToList(), sprite, sprite.GetCraftTime()));
                            foreach (var unit in villagerFocused)
                                unit.SetPathCycle((center, sprite));
                        }
                    }
                }
            }
        }

        private bool SurfaceIsEngaged(Rect surface)
        {
            return _nonUnits.Any(x => x.Surface.IntersectsWith(surface));
        }

        private bool CheckStructureResources(Sprite sprite)
        {
            var (gold, wood, rock) = sprite.GetResourcesCost();
            if (wood <= _resourcesQty[ResourceTypes.Wood]
                && gold <= _resourcesQty[ResourceTypes.Gold]
                && rock <= _resourcesQty[ResourceTypes.Rock])
            {
                UpdateQuantity(ResourceTypes.Wood, -wood);
                UpdateQuantity(ResourceTypes.Gold, -gold);
                UpdateQuantity(ResourceTypes.Rock, -rock);
                return true;
            }

            return false;
        }

        private void ManageUnitsMovements()
        {
            var emptyResources = new List<Sprite>(5);
            foreach (var unit in _units)
            {
                var emptyResourceSprite = ManageUnitMovement(unit);
                if (emptyResourceSprite != null)
                    emptyResources.Add(emptyResourceSprite);
            }

            emptyResources.ForEach(x => _sprites.Remove(x));
        }

        private Sprite ManageUnitMovement(Unit unit)
        {
            Sprite emptyResourceSprite = null;

            var tgt = unit.CheckForMovement();
            if (tgt != null && unit.Is<Villager>(out var villager))
            {
                var carry = villager.CheckCarry(tgt);
                if (carry.HasValue)
                {
                    UpdateQuantity(carry.Value.r, carry.Value.v);
                    if (villager.GetNextSpriteOnPath<Forest>() is var f && f != null)
                        villager.ComputeCycleOnForestPatch(_forestPatchs[f.ForestPatchIndex]);
                }

                if (tgt.Is<Resource>(out var rs) && rs.Quantity == 0)
                {
                    ManageEmptyResourceSprite(rs, villager);
                    emptyResourceSprite = rs;
                }
            }

            return emptyResourceSprite;
        }

        private void ManageEmptyResourceSprite(Resource tgt, Villager villager)
        {
            if (tgt.Is<Forest>(out var f))
            {
                var patch = _forestPatchs[f.ForestPatchIndex];
                if (patch.Contains(f))
                    patch.Remove(f);
                villager.ComputeCycleOnForestPatch(patch);
            }
        }

        private void ManageCraftsInProgress()
        {
            var finishedCrafts = new List<Craft>(10);
            foreach (var craft in _craftQueue)
            {
                if (craft.CheckForCompletion(_frames, Population < PotentialPopulation, _craftQueue))
                {
                    _sprites.Add(craft.Target);
                    finishedCrafts.Add(craft);
                }
            }

            var craftsToRemove = _craftQueue.Where(x => finishedCrafts.Contains(x)).ToList();
            foreach (var craft in craftsToRemove)
                _craftQueue.Remove(craft);
        }

        private void ManageCraftsToCancel()
        {
            var cancelledCrafts = new List<Craft>(10);

            foreach (var craft in _craftQueue)
            {
                if (craft.CheckForCancellation(_sprites))
                    cancelledCrafts.Add(craft);
            }

            foreach (var craft in cancelledCrafts)
            {
                if (!craft.Started)
                    RefundResources(craft.Target.GetType());
                _craftQueue.Remove(craft);
            }
        }

        private void RefundResources(System.Type type)
        {
            var attrValue = type.GetAttribute<ResourcesCostAttribute>();
            if (attrValue == null)
                return;

            UpdateQuantity(ResourceTypes.Gold, attrValue.Gold);
            UpdateQuantity(ResourceTypes.Wood, attrValue.Wood);
            UpdateQuantity(ResourceTypes.Rock, attrValue.Rock);
        }

        private void UpdateQuantity(ResourceTypes rt, int minusValue)
        {
            if (minusValue != 0)
            {
                _resourcesQty[rt] += _resourcesQty[rt] < minusValue
                    ? _resourcesQty[rt]
                    : minusValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"{rt}Quantity"));
            }
        }
    }
}
