using System;

namespace Age_Of_Nothing.Sprites.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CraftTimeAttribute : Attribute
    {
        public int FramesCount { get; }

        public CraftTimeAttribute(int framesCount)
        {
            FramesCount = framesCount;
        }
    }
}
