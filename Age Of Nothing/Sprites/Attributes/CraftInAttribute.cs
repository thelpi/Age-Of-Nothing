using System;
using System.Collections.Generic;

namespace Age_Of_Nothing.Sprites.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CraftInAttribute : Attribute
    {
        public IReadOnlyCollection<Type> CraftIn { get; }

        public CraftInAttribute(params Type[] craftIn)
        {
            CraftIn = craftIn;
        }
    }
}
