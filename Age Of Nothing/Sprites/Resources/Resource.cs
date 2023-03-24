using System.Windows;

namespace Age_Of_Nothing.Sprites.Resources
{
    public abstract class Resource : Sprite
    {
        private int _quantity;

        public int Quantity
        {
            get => _quantity;
            private set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged(nameof(Quantity));
                }
            }
        }

        public abstract ResourceTypes ResourceType { get; }

        protected Resource(Point center, int quantity, Controller parent)
            : base(center, true, false, parent)
        {
            _quantity = quantity;
        }

        /// <summary>
        /// Reduces the quantity available for the resource; mnimal is 0
        /// </summary>
        /// <param name="qtyLost"></param>
        /// <returns>
        /// Same as <paramref name="qtyLost"/> except if 0 is reached
        /// (in that case it's what remained)
        /// </returns>
        public int ReduceQuantity(int qtyLost)
        {
            if (qtyLost > Quantity)
            {
                var qtyReturned = Quantity;
                Quantity = 0;
                return qtyReturned;
            }
            else
            {
                Quantity -= qtyLost;
                return qtyLost;
            }
        }
    }
}
