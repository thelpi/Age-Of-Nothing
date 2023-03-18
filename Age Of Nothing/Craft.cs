using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Age_Of_Nothing.Sprites;
using Age_Of_Nothing.Sprites.Structures;
using Age_Of_Nothing.Sprites.Units;

namespace Age_Of_Nothing
{
    public class Craft : INotifyPropertyChanged
    {
        private readonly List<Sprite> _sources;
        private readonly Type _sourceType;

        private int _startingFrame;
        private int _currentSources;
        // The number of frames required to perform the craft for a single source.
        private int _unitaryFramesToPerform;
        // The total number of frames to craft; might change mid-term depending on source count
        private int _totalFramesCount;
        private int _progression;

        public event PropertyChangedEventHandler PropertyChanged;

        public Sprite Target { get; }
        public bool Started { get; private set; }

        public int Progression
        {
            get => _progression;
            private set
            {
                if (_progression != value)
                {
                    _progression = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Progression)));
                }
            }
        }

        public Craft(Sprite source, Sprite target)
            : this(new List<Sprite> { source }, target)
        { }

        public Craft(List<Sprite> sources, Sprite target)
        {
            if (sources.Count == 0)
                throw new ArgumentException("Soruces collection is empty.", nameof(sources));

            if (sources.Select(x => x.GetType()).Distinct().Count() > 1)
                throw new ArgumentException("Each item of the source collection has to be of the same type.", nameof(sources));

            if (target.GetCraftTime() <= 0)
                throw new ArgumentException("The target is not craftable.", nameof(target));

            if (target.Is<Unit>() && sources.Count > 1)
                throw new InvalidOperationException("Units can only be crafted by a single source.");

            _sources = sources;
            Target = target;
            _unitaryFramesToPerform = target.GetCraftTime();
            _sourceType = sources.First().GetType();
        }

        public void AddSource(Sprite sprite)
        {
            if (sprite.GetType() != _sourceType)
                throw new ArgumentException("The sprite should be of the same type as existing sources.", nameof(sprite));

            if (!Target.Is<Structure>())
                throw new InvalidOperationException("Adding source is only allowed for structure.");

            _sources.Add(sprite);
        }

        public bool CheckForCancellation(IReadOnlyCollection<Sprite> sprites)
        {
            // Reasons for cancellation:
            // - there is no source remaining to complete the craft
            // - the surface for the structure is already occupied by another structure
            // - villagers lost focus on structure to craft
            // TODO: we should keep the craft pending in the last case
            var cancel = false;

            var lost = _sources
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
                var unfocused = _sources
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
                {
                    finish = popAvailable;
                }
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
                {
                    finish = true;
                }
                else
                {
                    var availableSources = _sources.Count(x => x.Is<Villager>(out var villager) && villager.Center == tgtStruct.Center);
                    if (!Started)
                    {
                        if (availableSources > 0)
                            SetStartingFrame(frames, availableSources);
                    }
                    else
                    {
                        if (availableSources != _currentSources)
                            UpdateSources(frames, availableSources);
                    }
                }
            }

            var elapsed = frames - _startingFrame;
            Progression = (int)Math.Round((decimal)elapsed / (_totalFramesCount + elapsed));

            return finish;
        }

        private void SetStartingFrame(int currentFrame, int availableSourcesCount = 1)
        {
            if (!Started)
            {
                _currentSources = availableSourcesCount;
                _startingFrame = currentFrame;
                _totalFramesCount = ComputeRemainingFramesToPerform(_unitaryFramesToPerform);
                Started = true;
            }
        }

        private void UpdateSources(int currentFrame, int availableSourcesCount)
        {
            _currentSources = availableSourcesCount;
            var framesElapsed = Started ? currentFrame - _startingFrame : 0;
            _totalFramesCount = framesElapsed + ComputeRemainingFramesToPerform(_unitaryFramesToPerform - framesElapsed);
        }

        private bool IsStartedWithCommonSource(Craft craft)
        {
            return craft != this && _sources.Any(_ => craft._sources.Contains(_)) && Started;
        }

        private bool HasFinished(int currentFrame)
        {
            return Started && currentFrame - _startingFrame >= _totalFramesCount;
        }

        private bool RemoveSource(Sprite sprite)
        {
            _sources.Remove(sprite);
            return _sources.Count == 0;
        }

        private int ComputeRemainingFramesToPerform(int unitaryFramesToPerformRemaining)
        {
            return (int)Math.Round(unitaryFramesToPerformRemaining / (double)_currentSources);
        }
    }
}
