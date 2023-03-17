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

        public bool CheckForCancellation(IReadOnlyCollection<Sprite> sprites)
        {
            // Reasons for cancellation:
            // - there is no source remaining to complete the craft
            // - the surface for the structure is already occupied by another structure
            // - villagers lost focus on structure to craft
            // TODO: we should keep the craft pending in the last case
            var cancel = false;

            var lost = Sources
                .Where(x => !sprites.Contains(x))
                .ToList();

            foreach (var ms in lost)
            {
                if (RemoveSource(ms))
                    cancel = true;
            }

            if (Target.Is<Structure>())
            {
                if (sprites.Where(x => !x.Is<Unit>()).Any(x => x.Surface.IntersectsWith(Target.Surface)))
                    cancel = true;
                var unfocused = Sources
                    .Where(x => x.Is<Villager>(out var villager) && !villager.IsSpriteOnPath(Target))
                    .ToList();
                foreach (var ms in unfocused)
                {
                    if (RemoveSource(ms))
                        cancel = true;
                }
            }

            return cancel;
        }

        public bool CheckForCompletion(int frames, bool popAvailable, IReadOnlyCollection<Craft> craftQueue)
        {
            var finish = false;

            if (Target.Is<Unit>())
            {
                // if the max pop. is reached, we keep the craft pending
                if (HasFinished(frames))
                    finish = popAvailable;
                // to start the craft, any of the sources should not have another craft already started
                else if (!Started && !craftQueue.Any(x => x.IsStartedWithCommonSource(this)))
                {
                    if (popAvailable)
                        SetStartingFrame(frames);
                }
            }
            else if (Target.Is<Structure>(out var tgtStruct))
            {
                if (HasFinished(frames))
                    finish = true;
                else
                {
                    var availableSources = Sources.Count(x => x.Is<Villager>(out var villager) && villager.Center == tgtStruct.Center);
                    if (!Started)
                    {
                        if (availableSources > 0)
                            SetStartingFrame(frames, availableSources);
                    }
                    else
                    {
                        if (availableSources != CurrentSources)
                            UpdateSources(frames, availableSources);
                    }
                }
            }

            return finish;
        }
    }
}
