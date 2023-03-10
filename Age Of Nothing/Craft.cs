using System;
using System.Collections.Generic;
using System.Linq;
using Age_Of_Nothing.Sprites;

namespace Age_Of_Nothing
{
    public class Craft
    {
        private int _startingFrame;
        private readonly List<Sprite> _sources;

        // Assumes all sources are the same
        public IReadOnlyList<Sprite> Sources => _sources;
        public FocusableSprite Target { get; }
        // The total number of frames to craft; might change mid-term depending on source count
        public int TotalFramesCount { get; private set; }
        // The number of frames required to perform the craft for a single source.
        public int UnitaryFramesToPerform { get; }
        public bool Started { get; private set; }

        public int CurrentSources { get; private set; }

        public Craft(Sprite source, FocusableSprite target, int framesToPerform)
            : this(new List<Sprite> { source }, target, framesToPerform)
        { }

        public Craft(List<Sprite> sources, FocusableSprite target, int singleCompleteFramesCount)
        {
            _sources = sources;
            Target = target;
            UnitaryFramesToPerform = singleCompleteFramesCount;
        }

        public bool HasFinished(int currentFrame)
        {
            return Started && currentFrame - _startingFrame >= TotalFramesCount;
        }

        public void SetStartingFrame(int currentFrame, int availableSourcesCount = 1)
        {
            if (!Started)
            {
                CurrentSources = availableSourcesCount;
                _startingFrame = currentFrame;
                TotalFramesCount = ComputeRemainingFramesToPerform(UnitaryFramesToPerform);
                Started = true;
            }
        }

        public void UpdateSources(int currentFrame, int availableSourcesCount)
        {
            CurrentSources = availableSourcesCount;
            var framesElapsed = Started ? currentFrame - _startingFrame : 0;
            TotalFramesCount = framesElapsed + ComputeRemainingFramesToPerform(UnitaryFramesToPerform - framesElapsed);
        }

        public bool RemoveSource(Sprite sprite)
        {
            _sources.Remove(sprite);
            return _sources.Count == 0;
        }

        private int ComputeRemainingFramesToPerform(int unitaryFramesToPerformRemaining)
        {
            return (int)Math.Round(unitaryFramesToPerformRemaining / (double)CurrentSources);
        }

        public bool IsStartedWithCommonSource(Craft craft)
        {
            return craft != this && Sources.Any(_ => craft.Sources.Contains(_)) && Started;
        }
    }
}
