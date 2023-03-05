using System.Windows;

namespace Age_Of_Nothing
{
    public class Unit
    {
        public bool Selected { get; set; }
        public Point? TargetPosition { get; set; }
        public Point CurrentPosition { get; set; }

        // pixels by frame
        public double Speed { get; set; }
    }
}
