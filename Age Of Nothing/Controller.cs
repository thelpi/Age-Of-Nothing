using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Age_Of_Nothing.Events;
using Age_Of_Nothing.Sprites;
using Age_Of_Nothing.Sprites.Attributes;
using Age_Of_Nothing.Sprites.Resources;
using Age_Of_Nothing.Sprites.Structures;
using Age_Of_Nothing.Sprites.Units;

namespace Age_Of_Nothing
{
    public class Controller : INotifyPropertyChanged
    {
        private readonly ObservableCollection<Sprite> _sprites = new ObservableCollection<Sprite>();
        private readonly Dictionary<ResourceTypes, int> _resourcesQty;
        private readonly ObservableCollection<Craft> _craftQueue = new ObservableCollection<Craft>();
        private readonly List<List<Forest>> _forestPatchs = new List<List<Forest>>();

        private readonly Rect _surface;

        private int _frames;

        public IReadOnlyCollection<Sprite> Sprites => _sprites;
        public IReadOnlyCollection<Craft> CraftQueue => _craftQueue;

        private IEnumerable<Sprite> NonUnits => _sprites.Except(Units);
        private IEnumerable<Unit> Units => _sprites.OfType<Unit>();
        private IEnumerable<Villager> Villagers => _sprites.OfType<Villager>();
        private IEnumerable<Market> Markets => _sprites.OfType<Market>();
        private IEnumerable<Structure> Structures => _sprites.OfType<Structure>();
        public int PotentialPopulation => Structures.Sum(x => x.GetUnitsStorage());
        public int Population => Units.Count();
        public int WoodQuantity => _resourcesQty[ResourceTypes.Wood];
        public int RockQuantity => _resourcesQty[ResourceTypes.Rock];
        public int GoldQuantity => _resourcesQty[ResourceTypes.Gold];

        public event PropertyChangedEventHandler PropertyChanged;

        public Controller(double width, double height)
        {
            _surface = new Rect(0, 0, width, height);

            _resourcesQty = SystemExtensions.GetEnum<ResourceTypes>()
                .ToDictionary(x => x, x => 0);

            _sprites.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (var sprite in e.NewItems.OfType<Sprite>())
                    {
                        PropertyChanged?.Invoke(this, new SpritesCollectionChangedEventArgs(sprite, true));
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
                        PropertyChanged?.Invoke(this, new SpritesCollectionChangedEventArgs(item, false));
                }

                // in case of dwellings or units count has changed
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Population)));
            };

            _craftQueue.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (var craft in e.NewItems.OfType<Craft>())
                    {
                        PropertyChanged?.Invoke(this, new SpritesCollectionChangedEventArgs(craft, true));
                        // propagate the event
                        craft.PropertyChanged += (_, eSub) => PropertyChanged?.Invoke(craft, eSub);
                    }
                }

                if (e.OldItems != null)
                {
                    foreach (var craft in e.OldItems.OfType<Craft>())
                        PropertyChanged?.Invoke(this, new SpritesCollectionChangedEventArgs(craft, false));
                }
            };
        }

        public void Initialize()
        {
            _resourcesQty[ResourceTypes.Gold] = 10000;
            _resourcesQty[ResourceTypes.Rock] = 10000;
            _resourcesQty[ResourceTypes.Wood] = 10000;

            _sprites.Add(new Villager(new Point(200, 200), this));
            _sprites.Add(new Villager(new Point(100, 100), this));
            _sprites.Add(new Villager(new Point(300, 300), this));
            _sprites.Add(new RockMine(100, new Point(400, 120), this));
            _sprites.Add(new GoldMine(75, new Point(200, 600), this));
            _sprites.Add(new Market(new Point(600, 500), this));
            _sprites.Add(new Dwelling(new Point(1100, 10), this));
            _sprites.Add(new Dwelling(new Point(1100, 90), this));
            _sprites.Add(new Wall(new Point(335, 335), this));
            _sprites.Add(new Wall(new Point(335, 365), this));
            _sprites.Add(new Wall(new Point(335, 395), this));
            _sprites.Add(new Wall(new Point(365, 395), this));
            _sprites.Add(new Wall(new Point(395, 395), this));

            var forests = Forest.GenerateForestPatch(new Rect(700, 200, 300, 100), this, 0);
            _forestPatchs.Add(forests.ToList());
            foreach (var forest in _forestPatchs.Last())
                _sprites.Add(forest);
        }

        public IEnumerable<T> FocusedSprites<T>() where T : Sprite
        {
            return _sprites.Where(x => x.Focused).OfType<T>();
        }

        public void AddUnitToStack<T>() where T : Unit
        {
            lock (_craftQueue)
            {
                if (Structures.FirstIfNotNull(x => x.CanBuild<T>() && x.Focused, out var focusedStructure))
                {
                    var sprite = Unit.Instanciate<T>(focusedStructure.Center, this);
                    if (CheckStructureResources(sprite))
                        _craftQueue.Add(new Craft(focusedStructure, sprite));
                }
            }
        }

        public void CheckForDeletion()
        {
            lock (_sprites)
            {
                var sprite = _sprites.FirstOrDefault(x => x.Focused && x.HasLifePoints);
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
            foreach (var sp in _sprites)
                sp.Focused = false;
        }

        public void FocusOnZone(Rect zone)
        {
            ClearHover();

            var hasUnitSelected = false;
            foreach (var unit in Units)
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
                foreach (var x in _sprites.Where(x => !(x is Unit)))
                {
                    if (zone.RealIntersectsWith(x.Surface))
                    {
                        x.Focused = true;
                        break;
                    }
                }
            }
        }

        public void RefreshHover(Rect zone)
        {
            foreach (var sp in _sprites)
                sp.ForceHover(zone.RealIntersectsWith(sp.Surface));
        }

        public void ClearHover()
        {
            foreach (var sp in _sprites)
                sp.ForceHover(false);
        }

        public void SetTargetPositionsOnFocused(Point clickPosition)
        {
            var targets = _sprites.Where(x => x.Surface.Contains(clickPosition));
            foreach (var unit in Units.Where(x => x.Focused))
                unit.ComputeCycle(clickPosition, targets);
        }

        public void BuildStructure(System.Type type, IReadOnlyCollection<Point> centers)
        {
            if (type.IsAbstract || !type.IsSubclassOf(typeof(Structure)))
                throw new System.ArgumentException($"The type should be concrete and inherits from {nameof(Structure)}", nameof(type));

            var villagerFocused = Villagers.Where(x => x.Focused);
            if (!villagerFocused.Any())
                return;

            var newCrafts = new List<Craft>(centers.Count);
            lock (_craftQueue)
            {
                foreach (var rawCenter in centers)
                {
                    var center = rawCenter.RescaleBase10();

                    var surface = center.ComputeSurfaceFromMiddlePoint(Sprite.GetSpriteSize(type));
                    if (!SurfaceIsEngaged(surface) && IsInBound(surface))
                    {
                        var sprite = (Sprite)type
                            .GetConstructor(new[] { typeof(Point), typeof(Controller) })
                            .Invoke(new object[] { surface.TopLeft, this });
                        if (CheckStructureResources(sprite))
                        {
                            var craft = new Craft(villagerFocused.Cast<Sprite>().ToList(), sprite, true);
                            newCrafts.Add(craft);
                            _craftQueue.Add(craft);
                        }
                    }
                }
            }

            foreach (var unit in villagerFocused)
            {
                var craftTarget = newCrafts.Select(x => x.Target).GetClosestSprite(unit.Center);
                unit.SetPathCycle(new MoveTarget(craftTarget));
                newCrafts.First(x => x.Target == craftTarget).AddSource(unit);
            }
        }

        public bool IsInBound(Rect surface)
        {
            return _surface.Contains(surface);
        }

        private bool SurfaceIsEngaged(Rect surface)
        {
            return NonUnits.Any(x => x.Surface.RealIntersectsWith(surface));
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
            var pendingStructure = _craftQueue
                .Where(x => x.Started && x.Target.Is<Structure>())
                .Select(x => (Structure)x.Target);

            var emptyResources = new List<Sprite>(5);
            foreach (var unit in Units)
            {
                var emptyResourceSprite = ManageUnitMovement(unit, pendingStructure);
                if (emptyResourceSprite != null)
                    emptyResources.Add(emptyResourceSprite);
            }

            emptyResources.ForEach(x => _sprites.Remove(x));
        }

        private Sprite ManageUnitMovement(Unit unit, IEnumerable<Structure> pendingStructure)
        {
            Sprite emptyResourceSprite = null;

            var tgt = unit.CheckForMovement(pendingStructure);
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
            var canceledCrafts = new List<Craft>(10);

            foreach (var craft in _craftQueue)
            {
                if (craft.CheckForCancelation(_sprites, _craftQueue))
                    canceledCrafts.Add(craft);
            }

            foreach (var craft in canceledCrafts)
            {
                // note: we don't refund units when started; maybe we should?
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
