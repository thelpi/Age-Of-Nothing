﻿using System.Collections.Generic;
using System.Windows;
using Age_Of_Nothing.Sprites.Attributes;

namespace Age_Of_Nothing.Sprites.Units
{
    [Dimensions(20)]
    [CraftTime(150)]
    [Speed(5)]
    [LifePoints(40)]
    [ResourcesCost(10, 0, 0)]
    [CraftIn(typeof(Structures.Barracks))]
    public class Swordsman : Unit
    {
        public Swordsman(Point center, IEnumerable<Sprite> sprites)
            : base(center, sprites)
        { }
    }
}
