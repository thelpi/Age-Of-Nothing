namespace Age_Of_Nothing.Sprites
{
    public interface IResourceSprite
    {
        int Quantity { get; }

        PrimaryResources Resource { get; }

        void ReduceQuantity(int qtyLost);
    }
}
