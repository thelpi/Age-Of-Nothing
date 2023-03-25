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
        // The number of frames required to perform the craft for a single source.
        private readonly int _unitaryFramesToPerform;

        private int _elapsedFrames;
        private int _currentSources;
        private double _progression;
        private bool _cancelationPending;
        private bool _stuck;

        public event PropertyChangedEventHandler PropertyChanged;

        public Sprite Target { get; }
        public bool Started { get; private set; }

        public IReadOnlyCollection<Sprite> Sources => _sources;

        public double Progression
        {
            get => Math.Round(_progression * 100);
            private set
            {
                if (_progression != value)
                {
                    _progression = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Progression)));
                }
            }
        }

        public bool Stuck
        {
            get => _stuck;
            private set
            {
                if (_stuck != value)
                {
                    _stuck = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Stuck)));
                }
            }
        }

        public Craft(Sprite source, Sprite target)
            : this(new List<Sprite> { source }, target, false)
        { }

        public Craft(List<Sprite> sources, Sprite target, bool sourcePrebooking)
        {
            if (sources.Count == 0)
                throw new ArgumentException("Soruces collection is empty.", nameof(sources));

            if (sources.Select(x => x.GetType()).Distinct().Count() > 1)
                throw new ArgumentException("Each item of the source collection has to be of the same type.", nameof(sources));

            if (target.GetCraftTime() <= 0)
                throw new ArgumentException("The target is not craftable.", nameof(target));

            if (target.Is<Unit>() && sources.Count > 1)
                throw new InvalidOperationException("Units can only be crafted by a single source.");

            if (sourcePrebooking && target.Is<Unit>())
                throw new InvalidOperationException("Can't prebooking a unit craft.");

            _sources = sourcePrebooking ? new List<Sprite>() : sources;
            Target = target;
            _unitaryFramesToPerform = target.GetCraftTime();
            _sourceType = sources.First().GetType();
            _progression = 0;
        }

        public void AddSource(Sprite sprite)
        {
            if (sprite.GetType() != _sourceType)
                throw new ArgumentException("The sprite should be of the same type as existing sources.", nameof(sprite));

            if (!Target.Is<Structure>())
                throw new InvalidOperationException("Adding source is only allowed for structure.");

            _sources.Add(sprite);
        }

        public void Cancel()
        {
            _cancelationPending = true;
        }

        public bool CheckForCancelation(IReadOnlyCollection<Sprite> sprites, IReadOnlyCollection<Craft> crafts)
        {
            var cancel = false;

            if (_cancelationPending)
            {
                cancel = true;
            }
            else if (Target.Is<Structure>())
            {
                if (IntersectWithExistingStructure(sprites))
                    cancel = true;
                else if (IntersectWithStartedCraft(crafts))
                    cancel = true;
                else
                    CheckObsoleteSources(sprites); // this does not cancel the craft
            }
            else if (Target.Is<Unit>())
            {
                // the hosting structure does not exist anymore
                if (CheckObsoleteSources(sprites))
                    cancel = true;
            }

            return cancel;
        }

        public bool CheckForCompletion(bool popAvailable, IReadOnlyCollection<Craft> craftQueue)
        {
            var finish = false;
            var stuck = false;

            if (Target.Is<Unit>())
            {
                if (HasFinished())
                {
                    if (popAvailable)
                        finish = true;
                    else
                        stuck = true;
                }
                else if (!Started)
                {
                    if (popAvailable && !SourceIsBusy(craftQueue))
                        SetStartingFrame();
                    else
                        stuck = true;
                }
                else
                {
                    if (popAvailable)
                        _elapsedFrames++;
                    else
                        stuck = true;
                }
            }
            else if (Target.Is<Structure>())
            {
                if (HasFinished())
                {
                    finish = true;
                }
                else if (!Started)
                {
                    var availableSources = ComputeAvailableSources();
                    if (availableSources > 0)
                        SetStartingFrame(availableSources);
                    else
                        stuck = true;
                }
                else
                {
                    var availableSources = ComputeAvailableSources();
                    if (availableSources != _currentSources)
                        UpdateSources(availableSources);

                    if (availableSources > 0)
                        _elapsedFrames += availableSources;
                    else
                        stuck = true;
                }
            }

            if (Started)
                Progression = _elapsedFrames / (double)_unitaryFramesToPerform;
            Stuck = stuck;

            return finish;
        }

        private int ComputeAvailableSources()
        {
            return _sources.Count(x => x.Is<Villager>(out var villager) && villager.Center == Target.Center);
        }

        private bool SourceIsBusy(IReadOnlyCollection<Craft> craftQueue)
        {
            return craftQueue.Any(x => x.IsStartedWithCommonSource(this));
        }

        private bool IntersectWithExistingStructure(IReadOnlyCollection<Sprite> sprites)
        {
            return sprites.Any(x => x != Target && x.Is<Structure>() && x.Surface.RealIntersectsWith(Target.Surface));
        }

        private bool CheckObsoleteSources(IReadOnlyCollection<Sprite> sprites)
        {
            _sources.RemoveAll(x => !sprites.Contains(x));
            return _sources.Count == 0;
        }

        private void SetStartingFrame(int availableSourcesCount = 1)
        {
            if (!Started)
            {
                _currentSources = availableSourcesCount;
                _elapsedFrames++;
                Started = true;
            }
        }

        private void UpdateSources(int availableSourcesCount)
        {
            _currentSources = availableSourcesCount;
        }

        private bool IsStartedWithCommonSource(Craft craft)
        {
            return craft != this && _sources.Any(_ => craft._sources.Contains(_)) && Started;
        }

        private bool HasFinished()
        {
            return Started && _elapsedFrames >= _unitaryFramesToPerform;
        }

        private bool IntersectWithStartedCraft(IReadOnlyCollection<Craft> crafts)
        {
            return crafts.Any(x => x != this && x.Started && x.Target.Is<Structure>() && x.Target.Surface.RealIntersectsWith(Target.Surface));
        }
    }
}
