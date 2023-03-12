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
        private readonly Dictionary<PrimaryResources, int> _resourcesQty;
        private readonly List<Craft> _craftQueue = new List<Craft>(1000);

        private string _populationInformation;
        private int _frames;

        private IEnumerable<Sprite> _nonUnits => _sprites.Except(_units);
        private IEnumerable<Unit> _units => _sprites.OfType<Unit>();
        private IEnumerable<Villager> _villagers => _sprites.OfType<Villager>();
        private IEnumerable<Mine> _mines => _sprites.OfType<Mine>();
        private IEnumerable<Forest> _forests => _sprites.OfType<Forest>();
        private IEnumerable<Market> _markets => _sprites.OfType<Market>();
        private IEnumerable<Dwelling> _dwellings => _sprites.OfType<Dwelling>();
        private IEnumerable<FocusableSprite> _focusables => _sprites.OfType<FocusableSprite>();

        public string PopulationInformation
        {
            get => _populationInformation;
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
        public int GoldQuantity => _resourcesQty[PrimaryResources.Gold];

        public event PropertyChangedEventHandler PropertyChanged;

        public Controller()
        {
            _resourcesQty = new Dictionary<PrimaryResources, int>
            {
                { PrimaryResources.Gold, 100 },
                { PrimaryResources.Rock, 100 },
                { PrimaryResources.Wood, 100 }
            };

            _sprites.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (var item in e.NewItems)
                    {
                        var sprite = item as Sprite;
                        PropertyChanged?.Invoke(this, new SpritesCollectionChangedEventArgs(sprite.GetVisual, true));
                        if (sprite.Is<FocusableSprite>(out var fs))
                            fs.PropertyChanged += (s, e) => PropertyChanged?.Invoke(this, e);
                    }
                }

                if (e.OldItems != null)
                {
                    foreach (var item in e.OldItems)
                        PropertyChanged?.Invoke(this, new SpritesCollectionChangedEventArgs((item as Sprite).GetVisual, false));
                }

                // TODO: we should do this logic into view with a converter
                PopulationInformation = $"{Population} / {PotentialPopulation}";
            };
        }

        public void Initialize()
        {
            _sprites.Add(new Villager(new Point(200, 200), _focusables));
            _sprites.Add(new Villager(new Point(100, 100), _focusables));
            _sprites.Add(new Villager(new Point(300, 300), _focusables));
            _sprites.Add(new RockMine(100, new Point(400, 120), 1, _focusables));
            _sprites.Add(new GoldMine(75, new Point(200, 600), 1, _focusables));
            _sprites.Add(new Forest(new Rect(700, 200, 300, 100)));
            _sprites.Add(new Market(new Point(600, 500), _focusables));
            _sprites.Add(new Dwelling(new Point(1100, 10), _focusables));
            _sprites.Add(new Dwelling(new Point(1100, 90), _focusables));
        }

        public bool HasVillagerFocus()
        {
            return _villagers.Any(x => x.Focused);
        }

        public bool HasMarketFocus()
        {
            return _markets.Any(x => x.Focused);
        }

        public void BuildDwelling(Point center)
        {
            BuildStructure(center, (a, b) => new Dwelling(a, b));
        }

        private void BuildStructure<T>(Point center, System.Func<Point, IEnumerable<FocusableSprite>, T> ctor)
            where T : Structure
        {
            lock (_craftQueue)
            {
                var surface = center.ComputeSurfaceFromMiddlePoint(GetSpriteSize<T>());
                // TODO: surface should be inside the game area entirely
                if (!SurfaceIsEngaged(surface))
                {
                    var villagerFocused = _villagers.Where(x => x.Focused);
                    if (villagerFocused.Any())
                    {
                        if (CheckStructureResources<T>())
                        {
                            var sprite = ctor(surface.TopLeft, _focusables);
                            _craftQueue.Add(new Craft(villagerFocused.Cast<Sprite>().ToList(), sprite, GetCraftTime<T>()));
                            foreach (var unit in villagerFocused)
                                unit.SetCycle((center, sprite));
                        }
                    }
                }
            }
        }

        private bool SurfaceIsEngaged(Rect surface)
        {
            return _nonUnits.Any(x => x.Surface.IntersectsWith(surface));
        }

        private bool CheckStructureResources<T>() where T : Structure
        {
            var (gold, wood, rock) = GetResources<T>();
            if (wood <= _resourcesQty[PrimaryResources.Wood]
                && gold <= _resourcesQty[PrimaryResources.Gold]
                && rock <= _resourcesQty[PrimaryResources.Rock])
            {
                _resourcesQty[PrimaryResources.Wood] -= wood;
                _resourcesQty[PrimaryResources.Gold] -= gold;
                _resourcesQty[PrimaryResources.Rock] -= rock;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WoodQuantity)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RockQuantity)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GoldQuantity)));
                return true;
            }

            return false;
        }

        public void AddVillagerCreationToStack()
        {
            lock (_craftQueue)
            {
                var focusMarket = _markets.FirstOrDefault(x => x.Focused);
                if (focusMarket != null)
                    _craftQueue.Add(new Craft(focusMarket, new Villager(focusMarket.Center, _focusables), GetCraftTime<Villager>()));
            }
        }

        public void CheckForDeletion()
        {
            lock (_sprites)
            {
                var sprite = _focusables.FirstOrDefault(x => x.Focused && x.IsCraft);
                if (sprite != null)
                    _sprites.Remove(sprite);
            }
        }

        public void NewFrameCheck()
        {
            _frames++;
            ManageUnitsMovements();

            lock (_craftQueue)
            {
                lock (_sprites)
                {
                    ManageCraftsToCancel();
                    ManageCraftsInProgress();
                }
            }
        }

        private void ManageUnitsMovements()
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
                    PropertyChanged?.Invoke(this, new SpritePositionChangedEventArgs(unit.RefreshPosition));
            }
        }

        private void ManageCraftsInProgress()
        {
            var finishedCrafts = new List<Craft>(10);
            foreach (var craft in _craftQueue)
            {
                if (craft.Target.Is<Unit>())
                {
                    if (craft.HasFinished(_frames))
                    {
                        // if the max pop. is reached, we keep the craft pending
                        if (Population < PotentialPopulation)
                        {
                            _sprites.Add(craft.Target);
                            finishedCrafts.Add(craft);
                        }
                    }
                    // to start the craft, any of the sources should not have another craft already started
                    else if (!craft.Started && !_craftQueue.Any(x => x.IsStartedWithCommonSource(craft)))
                    {
                        if (Population < PotentialPopulation)
                            craft.SetStartingFrame(_frames);
                    }
                }
                else if (craft.Target.Is<Structure>(out var tgtStruct))
                {
                    if (craft.HasFinished(_frames))
                    {
                        _sprites.Add(craft.Target);
                        finishedCrafts.Add(craft);
                    }
                    else
                    {
                        var availableSources = craft.Sources.Count(x => x.Is<Villager>(out var villager) && villager.Center == tgtStruct.Center);
                        if (!craft.Started)
                        {
                            if (availableSources > 0)
                                craft.SetStartingFrame(_frames, availableSources);
                        }
                        else
                        {
                            if (availableSources != craft.CurrentSources)
                                craft.UpdateSources(_frames, availableSources);
                        }
                    }
                }
            }

            _craftQueue.RemoveAll(x => finishedCrafts.Contains(x));
        }

        private void ManageCraftsToCancel()
        {
            var cancelledCrafts = new List<Craft>(10);

            // Reasons for cancellation:
            // - there is no source remaining to complete the craft
            // - the surface for the structure is already occupied by another structure
            // - villagers lost focus on structure to craft 
            foreach (var craft in _craftQueue)
            {
                var lost = craft.Sources
                    .Where(x => !_sprites.Contains(x))
                    .ToList();
                foreach (var ms in lost)
                {
                    if (craft.RemoveSource(ms))
                        cancelledCrafts.Add(craft);
                }

                if (craft.Target.Is<Structure>())
                {
                    if (SurfaceIsEngaged(craft.Target.Surface))
                        cancelledCrafts.Add(craft);
                    var unfocused = craft.Sources
                        .Where(x => x.Is<Villager>(out var villager) && !villager.FocusedOn(craft.Target))
                        .ToList();
                    foreach (var ms in unfocused)
                    {
                        if (craft.RemoveSource(ms))
                            cancelledCrafts.Add(craft);
                    }
                }
            }

            // "Distinct" because the craft can have several reasons for cancellation at the same time
            foreach (var craft in cancelledCrafts.Distinct())
            {
                if (!craft.Started)
                    RefundResources(craft.Target.GetType());
                _craftQueue.Remove(craft);
            }
        }

        private void RefundResources(System.Type type)
        {
            var attrValue = GetAttribute<ResourcesAttribute>(type);
            if (attrValue == null)
                return;

            _resourcesQty[PrimaryResources.Gold] += attrValue.Gold;
            _resourcesQty[PrimaryResources.Wood] += attrValue.Wood;
            _resourcesQty[PrimaryResources.Rock] += attrValue.Rock;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WoodQuantity)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RockQuantity)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GoldQuantity)));
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
            else
            {
                var market = _markets.FirstOrDefault(x => x.Surface.Contains(clickPosition));
                if (market != null)
                {
                    villagerOverrideClickPosition = market.Center;
                    tgt = market;
                }
                else
                {
                    var forest = _forests.FirstOrDefault(x => x.Surface.Contains(clickPosition));
                    if (forest != null)
                    {
                        marketCycle = true;
                        tgt = forest;
                    }
                }
            }

            foreach (var unit in _units.Where(x => x.Focused))
            {
                if (unit.Is<Villager>())
                {
                    if (marketCycle && _markets.Any())
                    {
                        var tgtPoint = villagerOverrideClickPosition.GetValueOrDefault(clickPosition);
                        var closestMarket = _markets.OrderBy(x => Point.Subtract(tgtPoint, x.Center).LengthSquared).First();
                        unit.SetCycle((tgtPoint, tgt), (closestMarket.Center, closestMarket));
                    }
                    else
                    {
                        unit.SetCycle((villagerOverrideClickPosition.GetValueOrDefault(clickPosition), tgt));
                    }
                }
                else
                {
                    unit.SetCycle((clickPosition, tgt));
                }
            }
        }

        public (int gold, int wood, int rock) GetResources<T>() where T : Sprite
        {
            var r = GetAttribute<ResourcesAttribute>(typeof(T));
            return (r.Gold, r.Wood, r.Rock);
        }

        public Size GetSpriteSize<T>() where T : Sprite
        {
            return GetAttribute<SizeAttribute>(typeof(T)).Size;
        }

        public int GetCraftTime<T>() where T : Sprite
        {
            return GetAttribute<CraftTimeAttribute>(typeof(T)).CraftTime;
        }

        private static TAttr GetAttribute<TAttr>(System.Type targetType)
            where TAttr : System.Attribute
        {
            return System.Attribute.GetCustomAttribute(targetType, typeof(TAttr)) as TAttr;
        }
    }
}
