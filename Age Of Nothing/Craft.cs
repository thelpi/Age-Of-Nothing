using System;
using System.Collections.Generic;
using System.Linq;
using Age_Of_Nothing.Sprites;

namespace Age_Of_Nothing
{
    public class Craft
    {
        private int _startingFrame;
        private List<Sprite> _sources;

        // Assumes all sources are the same
        public IReadOnlyList<Sprite> Sources => _sources;
        public FocusableSprite Target { get; }
        // The total number of frames to craft; might change mid-term depending on source count
        public int TotalFramesCount { get; private set; }
        // The number of frames required to perform the craft for a single source.
        public int UnitaryFramesToPerform { get; }
        public bool Started { get; private set; }

        public Craft(Sprite source, FocusableSprite target, int framesToPerform)
        {
            _sources = new List<Sprite> { source };
            Target = target;
            TotalFramesCount = framesToPerform;
            UnitaryFramesToPerform = framesToPerform;
        }

        public Craft(List<Sprite> sources, FocusableSprite target, int singleCompleteFramesCount)
        {
            _sources = sources;
            Target = target;
            TotalFramesCount = ComputeRemainingFramesToPerform(singleCompleteFramesCount);
            UnitaryFramesToPerform = singleCompleteFramesCount;
        }

        public bool HasFinished(int currentFrame)
        {
            return Started && currentFrame - _startingFrame >= TotalFramesCount;
        }

        public void SetStartingFrame(int currentFrame)
        {
            if (!Started)
            {
                _startingFrame = currentFrame;
                Started = true;
            }
        }

        public bool RemoveSource(Sprite sprite, int currentFrame)
        {
            _sources.Remove(sprite);

            if (_sources.Count == 0)
                return true;

            var framesElapsed = Started ? currentFrame - _startingFrame : 0;
            TotalFramesCount = framesElapsed + ComputeRemainingFramesToPerform(UnitaryFramesToPerform - framesElapsed);
            return false;
        }

        private int ComputeRemainingFramesToPerform(int unitaryFramesToPerformRemaining)
        {
            return (int)Math.Round(unitaryFramesToPerformRemaining / (double)_sources.Count);
        }

        public bool IsStartedWithCommonSource(Craft craft)
        {
            return craft != this && Sources.Any(_ => craft.Sources.Contains(_)) && Started;
        }
    }
}
