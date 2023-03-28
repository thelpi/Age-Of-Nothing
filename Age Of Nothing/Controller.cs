using System;
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
        private readonly Dictionary<int, Dictionary<ResourceTypes, int>> _resourcesQty = new Dictionary<int, Dictionary<ResourceTypes, int>>();
        private readonly ObservableCollection<Craft> _craftQueue = new ObservableCollection<Craft>();
        private readonly List<List<Forest>> _forestPatchs = new List<List<Forest>>();
        private readonly Random _rdm = new Random();
        private readonly GameParameters _parameters;

        private int _frames;

        public int Width => _parameters.Width;
        public int Height => _parameters.Height;
        public IReadOnlyCollection<Sprite> Sprites => _sprites;
        public IReadOnlyCollection<Craft> CraftQueue => _craftQueue;

        private IEnumerable<Sprite> NonUnits => _sprites.Except(Units);
        private IEnumerable<Unit> Units => _sprites.OfType<Unit>();
        private IEnumerable<Villager> Villagers => _sprites.OfType<Villager>();
        private IEnumerable<Market> Markets => _sprites.OfType<Market>();
        private IEnumerable<Structure> Structures => _sprites.OfType<Structure>();
        public int PotentialPopulation => Structures.Sum(x => x.GetUnitsStorage());
        public int Population => Units.Count();
        public int WoodQuantity => _resourcesQty[1][ResourceTypes.Wood];
        public int RockQuantity => _resourcesQty[1][ResourceTypes.Rock];
        public int GoldQuantity => _resourcesQty[1][ResourceTypes.Gold];

        public event PropertyChangedEventHandler PropertyChanged;

        public Controller(GameParameters parameters)
        {
            _parameters = parameters;

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

        public Point Initialize()
        {
            var goldMineSize = Sprite.GetSpriteSize(typeof(GoldMine));
            var rockMinSize = Sprite.GetSpriteSize(typeof(RockMine));
            var forestSize = Sprite.GetSpriteSize(typeof(Forest));

            var camps = new List<Rect>(_parameters.TeamsCount);

            for (var iTeam = 1; iTeam <= _parameters.TeamsCount; iTeam++)
            {
                var camp = GenerateTeamCamp(iTeam);
                camps.Add(camp);
            }

            for (var i = 0; i < _parameters.RockPatchDensity; i++)
                InstanciateAndAddRandomSprite(() => new RockMine(100, new Point(_rdm.Next((int)rockMinSize.Width, Width - (int)rockMinSize.Width), _rdm.Next((int)rockMinSize.Height, Height - (int)rockMinSize.Height)), this), camps);
            for (var i = 0; i < _parameters.GoldPatchDensity; i++)
                InstanciateAndAddRandomSprite(() => new GoldMine(75, new Point(_rdm.Next((int)goldMineSize.Width, Width - (int)goldMineSize.Width), _rdm.Next((int)goldMineSize.Height, Height - (int)goldMineSize.Height)), this), camps);

            for (var i = 0; i < _parameters.ForestPatchDensity; i++)
            {
                var patchX = _rdm.Next(_parameters.MinForestPatchWidthCount, _parameters.MaxForestPatchWidthCount) * (int)forestSize.Width;
                var patchY = _rdm.Next(_parameters.MinForestPatchHeightCount, _parameters.MaxForestPatchHeightCount) * (int)forestSize.Height;
                var forests = Forest.GenerateForestPatch(new Rect(_rdm.Next(0, Width - patchX), _rdm.Next(0, Height - patchY), patchX, patchY), this, i);
                _forestPatchs.Add(new List<Forest>(50));
                foreach (var forest in forests)
                {
                    if (!_sprites.Any(x => x.Surface.RealIntersectsWith(forest.Surface))
                        && !camps.Any(x => x.RealIntersectsWith(forest.Surface)))
                    {
                        _forestPatchs.Last().Add(forest);
                        _sprites.Add(forest);
                    }
                }
            }

            return camps.First().GetCenter();
        }

        private Rect GenerateTeamCamp(int iTeam)
        {
            _resourcesQty.Add(iTeam, new Dictionary<ResourceTypes, int>
            {
                { ResourceTypes.Gold, _parameters.GoldStartingValue },
                { ResourceTypes.Rock, _parameters.RockStartingValue },
                { ResourceTypes.Wood, _parameters.WoodStartingValue }
            });

            var wallSize = Sprite.GetSpriteSize(typeof(Wall));

            const int campMargin = 250;

            var campWith = wallSize.Width * _parameters.WallDimX;
            var campHeight = wallSize.Height * _parameters.WallDimY;

            // 1 5 3
            // 7 X 8
            // 4 6 2
            double campX;
            double campY;
            switch (iTeam)
            {
                case 1: campX = campMargin; campY = campMargin; break;
                case 4: campX = campMargin; campY = Height - campHeight - campMargin; break;
                case 7: campX = campMargin; campY = Height / 2 - (campHeight / 2); break;
                case 5: campX = Width / 2 - (campWith / 2); campY = campMargin; break;
                case 6: campX = Width / 2 - (campWith / 2); campY = Height - campHeight - campMargin; break;
                case 3: campX = Width - campWith - campMargin; campY = campMargin; break;
                case 8: campX = Width - campWith - campMargin; campY = Height / 2 - (campHeight / 2); break;
                case 2: campX = Width - campWith - campMargin; campY = Height - campHeight - campMargin; break;
                default: throw new NotImplementedException();
            }

            var camp = new Rect(campX, campY, campWith, campHeight);
            for (var i = 0; i < _parameters.WallDimX; i++)
            {
                for (var j = 0; j < _parameters.WallDimY; j++)
                {
                    var x = campX + (i * wallSize.Width);
                    var y = campY + (j * wallSize.Height);

                    var isBoundX = i == 0 || i == _parameters.WallDimX - 1;
                    var isBoundY = j == 0 || j == _parameters.WallDimY - 1;
                    if (isBoundX || isBoundY)
                    {
                        // openings
                        if (isBoundX && j > (_parameters.WallDimY / 2) - 2 && j < (_parameters.WallDimY / 2) + 2)
                            continue;
                        if (isBoundY && i > (_parameters.WallDimX / 2) - 2 && i < (_parameters.WallDimX / 2) + 2)
                            continue;

                        var wall = new Wall(new Point(x, y), this, iTeam);
                        _sprites.Add(wall);
                    }
                    else if (i == _parameters.WallDimX / 2 && j == _parameters.WallDimY / 2)
                    {
                        var market = new Market(new Point(x, y), this, iTeam);
                        _sprites.Add(market);
                    }
                    else if ((i == _parameters.WallDimX / 4 && j == _parameters.WallDimY / 4)
                        || (i == _parameters.WallDimX / 4 * 3 && j == _parameters.WallDimY / 4)
                        || (i == _parameters.WallDimX / 4 && j == _parameters.WallDimY / 4 * 3)
                        || (i == _parameters.WallDimX / 4 * 3 && j == _parameters.WallDimY / 4 * 3))
                    {
                        var villager = new Villager(new Point(x, y), this, iTeam);
                        _sprites.Add(villager);
                    }
                }
            }

            return camp;
        }

        private void InstanciateAndAddRandomSprite(Func<Sprite> builder, List<Rect> camps)
        {
            Sprite sprite;
            do
            {
                sprite = builder();
                if (_sprites.Any(x => x.Surface.RealIntersectsWith(sprite.Surface))
                    || camps.Any(x => x.RealIntersectsWith(sprite.Surface)))
                {
                    sprite = null;
                }
            }
            while (sprite == null);
            _sprites.Add(sprite);
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
                    // TODO: per team
                    var sprite = Unit.Instanciate<T>(focusedStructure.Center, this, 1);
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

        public void BuildStructure(Type type, IReadOnlyCollection<Point> centers)
        {
            if (type.IsAbstract || !type.IsSubclassOf(typeof(Structure)))
                throw new ArgumentException($"The type should be concrete and inherits from {nameof(Structure)}", nameof(type));

            var villagerFocused = Villagers.Where(x => x.Focused);
            if (!villagerFocused.Any())
                return;

            var newCrafts = new List<Craft>(centers.Count);
            lock (_craftQueue)
            {
                foreach (var center in centers)
                {
                    var surface = center.ComputeSurfaceFromMiddlePoint(Sprite.GetSpriteSize(type));
                    if (!SurfaceIsEngaged(surface) && IsInBound(surface))
                    {
                        // TODO: per team
                        var sprite = (Sprite)type
                            .GetConstructor(new[] { typeof(Point), typeof(Controller), typeof(int) })
                            .Invoke(new object[] { surface.TopLeft, this, 1 });
                        if (CheckStructureResources(sprite))
                        {
                            var craft = new Craft(villagerFocused.Cast<Sprite>().ToList(), sprite, true);
                            newCrafts.Add(craft);
                            _craftQueue.Add(craft);
                        }
                    }
                }
            }

            if (newCrafts.Count > 0)
            {
                foreach (var unit in villagerFocused)
                {
                    var craftTarget = newCrafts.Select(x => x.Target).GetClosestSprite(unit.Center);
                    unit.SetPathCycle(new MoveTarget(craftTarget));
                    newCrafts.First(x => x.Target == craftTarget).AddSource(unit);
                }
            }
        }

        public bool IsInBound(Rect surface)
        {
            return new Rect(0, 0, Width, Height).Contains(surface);
        }

        private bool SurfaceIsEngaged(Rect surface)
        {
            return NonUnits.Any(x => x.Surface.RealIntersectsWith(surface));
        }

        private bool CheckStructureResources(Sprite sprite)
        {
            var (gold, wood, rock) = sprite.GetResourcesCost();
            if (wood <= _resourcesQty[sprite.Team][ResourceTypes.Wood]
                && gold <= _resourcesQty[sprite.Team][ResourceTypes.Gold]
                && rock <= _resourcesQty[sprite.Team][ResourceTypes.Rock])
            {
                UpdateQuantity(sprite.Team, ResourceTypes.Wood, -wood);
                UpdateQuantity(sprite.Team, ResourceTypes.Gold, -gold);
                UpdateQuantity(sprite.Team, ResourceTypes.Rock, -rock);
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
                    UpdateQuantity(villager.Team, carry.Value.r, carry.Value.v);
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
                if (craft.CheckForCompletion(Population < PotentialPopulation, _craftQueue))
                {
                    _sprites.Add(craft.Target);
                    finishedCrafts.Add(craft);
                }
            }

            var structuresToFinish = _craftQueue
                .Where(x => x.Target.Is<Structure>() && !finishedCrafts.Contains(x))
                .ToList();

            var craftsToRemove = _craftQueue.Where(x => finishedCrafts.Contains(x)).ToList();
            foreach (var craft in craftsToRemove)
            {
                _craftQueue.Remove(craft);
                if (craft.Target.Is<Structure>() && structuresToFinish.Count > 0)
                {
                    foreach (var unit in craft.Sources.OfType<Villager>())
                    {
                        var craftTarget = structuresToFinish.Select(x => x.Target).GetClosestSprite(unit.Center);
                        unit.SetPathCycle(new MoveTarget(craftTarget));
                        _craftQueue.First(x => x.Target == craftTarget).AddSource(unit);
                    }
                }
            }
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
                    RefundResources(craft.Target.Team, craft.Target.GetType());
                _craftQueue.Remove(craft);
            }
        }

        private void RefundResources(int team, Type type)
        {
            var attrValue = type.GetAttribute<ResourcesCostAttribute>();
            if (attrValue == null)
                return;

            UpdateQuantity(team, ResourceTypes.Gold, attrValue.Gold);
            UpdateQuantity(team, ResourceTypes.Wood, attrValue.Wood);
            UpdateQuantity(team, ResourceTypes.Rock, attrValue.Rock);
        }

        private void UpdateQuantity(int team, ResourceTypes rt, int minusValue)
        {
            if (minusValue != 0)
            {
                _resourcesQty[team][rt] += minusValue > 0
                    ? minusValue
                    : (_resourcesQty[team][rt] < minusValue
                        ? _resourcesQty[team][rt]
                        : minusValue);
                if (team == 1)
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"{rt}Quantity"));
            }
        }
    }
}
