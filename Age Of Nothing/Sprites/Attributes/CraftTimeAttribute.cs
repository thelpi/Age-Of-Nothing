using System;

namespace Age_Of_Nothing.Sprites.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CraftTimeAttribute : Attribute
    {
        public int CraftTime { get; }

        public CraftTimeAttribute(int craftTime)
        {
            CraftTime = craftTime;
        }
    }
}
