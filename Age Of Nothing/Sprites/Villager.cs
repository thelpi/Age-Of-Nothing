﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Age_Of_Nothing.Sprites.Attributes;

namespace Age_Of_Nothing.Sprites
{
    [Size(_size)]
    [CraftTime(120)]
    public class Villager : Unit
    {
        private const double _size = 20;
        private const double _speed = 5;

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

        public Villager(Point center, IEnumerable<FocusableSprite> sprites)
            : base(center, _speed, _size, sprites)
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
                    Carry = (rs.ResourceType, realQty);
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
        public override void ComputeCycle(Point originalPoint, Sprite target)
        {
            if (target == null)
            {
                SetPathCycle((originalPoint, null));
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
                    SetPathCycle((target.Center, target), (closestMarket.Center, closestMarket));
                }
                else
                {
                    SetPathCycle((target.Center, target));
                }
            }
        }
    }
}