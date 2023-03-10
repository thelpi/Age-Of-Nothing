using Age_Of_Nothing.Sprites;

namespace Age_Of_Nothing
{
    public class Craft
    {
        private int _startingFrame;

        public Sprite Source { get; }
        public FocusableSprite Target { get; }
        public int CompleteFramesCount { get; }
        public bool Started { get; private set; }

        public Craft(Sprite source, FocusableSprite target, int completeFramesCount)
        {
            Source = source;
            Target = target;
            CompleteFramesCount = completeFramesCount;
        }

        public bool HasFinished(int currentFrame)
        {
            return Started && currentFrame - _startingFrame >= CompleteFramesCount;
        }

        public void SetStartingFrame(int currentFrame)
        {
            if (!Started)
            {
                _startingFrame = currentFrame;
                Started = true;
            }
        }
    }
}
