﻿using System.Windows;
using Age_Of_Nothing.Sprites.Attributes;

namespace Age_Of_Nothing.Sprites.Structures
{
    [CraftTime(400)]
    [ResourcesCost(10, 200, 50)]
    [Dimensions(100)]
    [UnitsStorage(5)]
    [LifePoints(1000)]
    public class Market : Structure
    {
        public Market(Point topLeft, Controller parent, int team)
            : base(topLeft, parent, false, team)
        { }
    }
}
