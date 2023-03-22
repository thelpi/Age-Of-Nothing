using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Age_Of_Nothing.Sprites.Attributes;
using Age_Of_Nothing.Sprites.Resources;
using Age_Of_Nothing.Sprites.Structures;

namespace Age_Of_Nothing.Sprites.Units
{
    [Dimensions(10)]
    [CraftTime(120)]
    [Speed(5)]
    [LifePoints(20)]
    [CraftIn(typeof(Market))]
    public class Villager : Unit
    {
        private static readonly IReadOnlyDictionary<ResourceTypes, int> _carryCapacity = new Dictionary<ResourceTypes, int>
        {
            { ResourceTypes.Gold, 10 },
            { ResourceTypes.Wood, 10 },
            { ResourceTypes.Rock, 10 }
        };

        private (ResourceTypes r, int v)? _carry;

        public (ResourceTypes r, int v)? Carry
        {
            get => _carry;
            set
            {
                if (_carry != value)
                {
                    _carry = value;
                    OnPropertyChanged(nameof(Carry));
                }
            }
        }

        public Villager(Point center, IEnumerable<Sprite> sprites)
            : base(center, sprites)
        { }

        /// <summary>
        /// Check what to do with the carry for this frame
        /// </summary>
        /// <param name="onTo">The sprite the villager is on.</param>
        /// <returns>If villager on the market, the carry to unload.</returns>
        public (ResourceTypes r, int v)? CheckCarry(Sprite onTo)
        {
            (ResourceTypes r, int v)? carryToDump = null;

            if (onTo.Is<Market>())
            {
                carryToDump = _carry;
                Carry = null;
            }
            else if (onTo.Is<Resource>(out var rs))
            {
                var realQty = rs.ReduceQuantity(_carryCapacity[rs.ResourceType]);
                if (realQty > 0)
                {
                    Carry = (rs.ResourceType, realQty);
                }
                else
                {
                    // the villager is on the resource, but it's empty
                    // path need to be recompute
                    SetPathCycle();
                }
            }

            return carryToDump;
        }

        /// <summary>
        /// Gets if the villager carry the maximal capacity for the resource.
        /// </summary>
        /// <param name="rsc"></param>
        /// <returns></returns>
        public bool IsMaxCarrying(ResourceTypes rsc)
        {
            return _carry.HasValue && _carry.Value.v >= _carryCapacity[rsc];
        }

        /// <inheritdoc />
        public override void ComputeCycle(Point originalPoint,
            IEnumerable<Sprite> targets,
            IEnumerable<Craft> inProgressCrafts)
        {
            // For now, the choice is to only care about structure and resource target
            // AKA no attack from villager on other units

            var target = targets.FirstOrDefault(x => x.Is<Resource>() || x.Is<Structure>());
            if (target == null)
            {
                if (inProgressCrafts.FirstIfNotNull(x =>
                    x.Target.Is<Structure>() && x.Target.Surface.Contains(originalPoint), out var craft))
                {
                    SetPathCycle(new MoveTarget(craft.Target));
                    craft.AddSource(this);
                }
                else
                {
                    SetPathCycle(new MoveTarget(originalPoint));
                }
            }
            else
            {
                if (target.Is<Forest>(out var forest))
                {
                    // finds the closest forest sprite in the patch from the villager position
                    target = Sprites
                        .OfType<Forest>()
                        .Where(x => x.ForestPatchIndex == forest.ForestPatchIndex)
                        .GetClosestSprite(Center);
                }

                if (target.Is<Resource>() && Sprites.OfType<Market>().Any())
                {
                    var closestMarket = Sprites.OfType<Market>().GetClosestSprite(target.Center);
                    SetPathCycle(new MoveTarget(target), new MoveTarget(closestMarket));
                }
                else
                {
                    SetPathCycle(new MoveTarget(target));
                }
            }
        }

        public void ComputeCycleOnForestPatch(List<Forest> patch)
        {
            if (patch.Count > 0 && Sprites.Any(x => x.Is<Market>()))
            {
                var fpOk = patch.GetClosestSprite(Center);
                var closestMarket = Sprites.OfType<Market>().GetClosestSprite(fpOk.Center);
                if (IsMaxCarrying(ResourceTypes.Wood))
                    SetPathCycle(new MoveTarget(closestMarket), new MoveTarget(fpOk));
                else
                    SetPathCycle(new MoveTarget(fpOk), new MoveTarget(closestMarket));
            }
        }
    }
}