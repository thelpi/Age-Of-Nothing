using System;
using System.ComponentModel;

namespace Age_Of_Nothing.Events
{
    public class SpritePositionChangedEventArgs : PropertyChangedEventArgs
    {
        public const string SpritePositionPropertyName = "SpritePosition";

        public Action PositionCallback { get; }

        public SpritePositionChangedEventArgs(Action positionCallback)
            : base(SpritePositionPropertyName)
        {
            PositionCallback = positionCallback;
        }
    }
}
