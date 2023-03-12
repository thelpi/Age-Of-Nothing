using System;

namespace Age_Of_Nothing.Sprites.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ResourcesAttribute : Attribute
    {
        public int Gold { get; }
        public int Wood { get; }
        public int Rock { get; }

        public ResourcesAttribute(int gold, int wood, int rock)
        {
            Gold = gold;
            Wood = wood;
            Rock = rock;
        }
    }
}
