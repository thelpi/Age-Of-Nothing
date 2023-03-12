﻿using System.Collections.Generic;
using System.Windows;
using Age_Of_Nothing.Sprites.Attributes;

namespace Age_Of_Nothing.Sprites
{
    [Size(Size)]
    [CraftTime(120)]
    public class Villager : Unit
    {
        const double Size = 20;

        private const double _speed = 5;
        private static readonly IReadOnlyDictionary<PrimaryResources, int> _carryCapacity = new Dictionary<PrimaryResources, int>
        {
            { PrimaryResources.Gold, 10 },
            { PrimaryResources.Wood, 10 },
            { PrimaryResources.Rock, 10 }
        };

        public Villager(Point center, IEnumerable<FocusableSprite> sprites)
            : base(center, _speed, Size, sprites)
        { }

        private (PrimaryResources r, int v)? _carrying;

        public (PrimaryResources r, int v)? CheckCarry(Sprite tgt)
        {
            (PrimaryResources r, int v)? carry = null;
            if (tgt.Is<Market>())
            {
                carry = _carrying;
                _carrying = null;
            }
            else if (tgt.Is<IResourceSprite>(out var rs))
            {
                var realQty = rs.ReduceQuantity(_carryCapacity[rs.Resource]);
                if (realQty > 0)
                    _carrying = (rs.Resource, realQty);
                else
                    SetCycle();
            }
            return carry;
        }
    }
}