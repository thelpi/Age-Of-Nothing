using System;

namespace Age_Of_Nothing.Sprites.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class UnitsStorageAttribute : Attribute
    {
        public int UnitsStorage { get; }

        public UnitsStorageAttribute(int unitsStorage)
        {
            UnitsStorage = unitsStorage;
        }
    }
}
