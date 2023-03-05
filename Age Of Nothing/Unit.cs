using System.Windows;
using System.Windows.Media;

namespace Age_Of_Nothing
{
    public class Unit
    {
        public bool Selected { get; set; }
        public Point? TargetPosition { get; set; }
        public Point CurrentPosition { get; set; }

        // pixels by frame
        public double Speed { get; set; }

        public Brush Fill(bool hover)
        {
            return Selected
                ? (Brush)new RadialGradientBrush(hover ? Colors.CornflowerBlue : Colors.Blue, Colors.Red)
                : hover ? Brushes.CornflowerBlue : Brushes.Blue;
        }
    }
}
