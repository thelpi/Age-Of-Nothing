﻿using System.Windows;
using Age_Of_Nothing.Sprites.Attributes;

namespace Age_Of_Nothing.Sprites.Units
{
    [Dimensions(10)]
    [CraftTime(240)]
    [Speed(8)]
    [LifePoints(80)]
    [ResourcesCost(50, 0, 0)]
    [CraftIn(typeof(Structures.Barracks))]
    public class Knight : Unit
    {
        public Knight(Point center, Controller parent, int team)
            : base(center, parent, team)
        { }
    }
}
