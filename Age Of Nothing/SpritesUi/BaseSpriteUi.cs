using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Age_Of_Nothing.Sprites;

namespace Age_Of_Nothing.SpritesUi
{
    public abstract class BaseSpriteUi<T> : UserControl
        where T : Sprite
    {
        public T Sprite { get; }

        protected BaseSpriteUi(T sprite)
        {
            Sprite = sprite;
        }

        protected static Brush GetImageFill(Brush backgroundBrush, string imageName)
        {
            var oGrid = new Grid();
            oGrid.SetBinding(WidthProperty, new Binding(nameof(ActualWidth))
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor)
                {
                    AncestorType = typeof(Shape)
                }
            });
            oGrid.SetBinding(HeightProperty, new Binding(nameof(ActualHeight))
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor)
                {
                    AncestorType = typeof(Shape)
                }
            });

            var oRectangle = new Rectangle
            {
                Fill = backgroundBrush
            };
            oGrid.Children.Add(oRectangle);

            var img = new Image
            {
                Source = new BitmapImage(new Uri($@"Resources/Images/{imageName}.png", UriKind.Relative))
            };
            oGrid.Children.Add(img);

            return new VisualBrush
            {
                Visual = oGrid
            };
        }
    }
}
