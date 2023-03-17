using System;

namespace Age_Of_Nothing.Sprites.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MilitaryAttribute : Attribute
    {
        public int Attack { get; }
        public int Defense { get; }
        public int Reach { get; }
        public int Delay { get; }

        public MilitaryAttribute(int attack, int defense, int reach, int delay)
        {
            Attack = attack;
            Defense = defense;
            Reach = reach;
            Delay = delay;
        }
    }
}
