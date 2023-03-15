using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using Age_Of_Nothing.Sprites.Attributes;

namespace Age_Of_Nothing.Sprites
{
    [Size(Size)]
    [CraftTime(120)]
    public class Villager : Unit
    {
        private const double Size = 20;

        private const double _speed = 5;
        private static readonly IReadOnlyDictionary<ResourceTypes, int> _carryCapacity = new Dictionary<ResourceTypes, int>
        {
            { ResourceTypes.Gold, 10 },
            { ResourceTypes.Wood, 10 },
            { ResourceTypes.Rock, 10 }
        };

        public Villager(Point center, IEnumerable<FocusableSprite> sprites)
            : base(center, _speed, Size, sprites)
        { }

        private (ResourceTypes r, int v)? _carrying;

        public (ResourceTypes r, int v)? CheckCarry(Sprite tgt)
        {
            (ResourceTypes r, int v)? carry = null;
            if (tgt.Is<Market>())
            {
                carry = _carrying;
                _carrying = null;
                NotifyResources();
            }
            else if (tgt.Is<Resource>(out var rs))
            {
                var realQty = rs.ReduceQuantity(_carryCapacity[rs.ResourceType]);
                if (realQty > 0)
                {
                    _carrying = (rs.ResourceType, realQty);
                    NotifyResources();
                }
                else
                    SetCycle();
            }
            return carry;
        }

        public bool IsCarryingMax(ResourceTypes rsc)
        {
            return _carrying.HasValue && _carrying.Value.v >= _carryCapacity[rsc];
        }

        public ResourceTypes? IsCarrying()
        {
            return _carrying.HasValue ? _carrying.Value.r : default(ResourceTypes?);
        }
    }
}