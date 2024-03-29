﻿using System.Windows;
using Age_Of_Nothing.Sprites.Attributes;

namespace Age_Of_Nothing.Sprites.Resources
{
    [Dimensions(80)]
    public class RockMine : Resource
    {
        public override ResourceTypes ResourceType => ResourceTypes.Rock;

        public RockMine(int quantity, Point position, Controller parent)
            : base(position, quantity, parent)
        { }
    }
}
