using System;
using System.Windows;

namespace Age_Of_Nothing
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SizeAttribute : Attribute
    {
        public Size Size { get; }

        public SizeAttribute(double size)
            : this(size, size)
        { }

        public SizeAttribute(double width, double height)
        {
            Size = new Size(width, height);
        }
    }
}
