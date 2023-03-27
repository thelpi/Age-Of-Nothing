namespace Age_Of_Nothing
{
    public class GameParameters
    {
        public int Width { get; } = 6400;
        public int Height { get; } = 3600;
        public int WallDimX { get; } = 24;
        public int WallDimY { get; } = 18;
        public int GoldPatchDensity { get; } = 10;
        public int RockPatchDensity { get; } = 10;
        public int ForestPatchDensity { get; } = 100;
        public int MaxForestPatchWidthCount { get; } = 16;
        public int MinForestPatchWidthCount { get; } = 3;
        public int MaxForestPatchHeightCount { get; } = 8;
        public int MinForestPatchHeightCount { get; } = 2;
        public int TeamsCount { get; } = 4;
        public bool IsTest { get; } = false;
        public int RockStartingValue { get; } = 400;
        public int GoldStartingValue { get; } = 400;
        public int WoodStartingValue { get; } = 400;
    }
}
