using System;
using System.Windows;

namespace Age_Of_Nothing.Sprites.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DimensionsAttribute : Attribute
    {
        public Size Size { get; }

        public DimensionsAttribute(double size)
            : this(size, size)
        { }

        public DimensionsAttribute(double width, double height)
        {
            Size = new Size(width, height);
        }
    }
}
