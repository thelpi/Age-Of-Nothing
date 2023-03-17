using System;

namespace Age_Of_Nothing.Sprites.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ResourcesCostAttribute : Attribute
    {
        public int Gold { get; }
        public int Wood { get; }
        public int Rock { get; }

        public ResourcesCostAttribute(int gold, int wood, int rock)
        {
            Gold = gold;
            Wood = wood;
            Rock = rock;
        }
    }
}
