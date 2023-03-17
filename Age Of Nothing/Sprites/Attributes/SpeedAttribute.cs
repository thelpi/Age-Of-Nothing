using System;

namespace Age_Of_Nothing.Sprites.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SpeedAttribute : Attribute
    {
        public double PixelsByFrame { get; }

        public SpeedAttribute(double pixelsByFrame)
        {
            PixelsByFrame = pixelsByFrame;
        }
    }
}
