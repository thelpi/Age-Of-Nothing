namespace Age_Of_Nothing
{
    public static class EnumExtensions
    {
        public static PrimaryResources? ToResource(this TargetType targetType)
        {
            switch (targetType)
            {
                case TargetType.Forest:
                    return PrimaryResources.Wood;
                case TargetType.IronMine:
                    return PrimaryResources.Iron;
                case TargetType.RockMine:
                    return PrimaryResources.Rock;
            }
            return null;
        }
    }
}
