using System;

namespace Age_Of_Nothing.Sprites.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class LifePointsAttribute : Attribute
    {
        public int LifePoints { get; }

        public LifePointsAttribute(int lifePoints)
        {
            LifePoints = lifePoints;
        }
    }
}
