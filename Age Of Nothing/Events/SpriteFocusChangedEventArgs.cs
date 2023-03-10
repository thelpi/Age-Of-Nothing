using System.ComponentModel;

namespace Age_Of_Nothing.Events
{
    public class SpriteFocusChangedEventArgs : PropertyChangedEventArgs
    {
        public const string SpriteFocusPropertyName = "SpriteFocus";

        public SpriteFocusChangedEventArgs()
            : base(SpriteFocusPropertyName)
        { }
    }
}
