using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Threading;
using System;

namespace SoundMeter.UI.Controls
{
    internal class AnimatedScaleControl : TemplatedControl
    {
        #region Properties
        public static readonly DirectProperty<AnimatedScaleControl, double> NormalVolumeProperty =
            AvaloniaProperty.RegisterDirect<AnimatedScaleControl, double>(nameof(NormalVolume), x => x.NormalVolume, (o, v) => o.NormalVolume = v);

        private double _normalVolume;
        public double NormalVolume
        {
            get => _normalVolume;
            set
            {
                SetAndRaise(NormalVolumeProperty, ref _normalVolume, value);
                if (_prevNormalVolume != _normalVolume && !_isAnimationInProgress)
                    StartAnimation();
            }
        }

        public static readonly DirectProperty<AnimatedScaleControl, double> HighVolumeProperty =
            AvaloniaProperty.RegisterDirect<AnimatedScaleControl, double>(nameof(HighVolume), x => x.HighVolume, (o,v) => o.HighVolume = v );

        private double _highVolume;
        public double HighVolume
        {
            get => _highVolume;
            set
            {
                SetAndRaise(HighVolumeProperty, ref _highVolume, value);
                if (_prevHighVolume != _highVolume && !_isAnimationInProgress)
                    StartAnimation();
            }
        }

        public static DirectProperty<AnimatedScaleControl, double> NormalVolumeLimitProperty =
            AvaloniaProperty.RegisterDirect<AnimatedScaleControl, double>(nameof(NormalVolumeLimit), x => x.NormalVolume, (o, v) => o.NormalVolumeLimit = v);

        private double _normalVolumeLimit;
        public double NormalVolumeLimit
        {
            get => _normalVolumeLimit.Round();
            set
            {
                SetAndRaise(NormalVolumeLimitProperty, ref _normalVolumeLimit, value);
            }
        }

        #endregion

        private Rectangle _normalVolumeContainer;
        private Rectangle _highVolumeContainer;
        private double _currentTicks = 0;
        private double _prevNormalVolume;
        private double _prevHighVolume;
        private double _normalVolumeToChange;
        private double _highVolumeToChange;
        private bool _isAnimationInProgress = false;
        private readonly DispatcherTimer _timer;

        private TimeSpan _animationDuration = TimeSpan.FromMilliseconds(300);
        private TimeSpan FrameRate => AppSettings.FrameRate;
        private double TotalTicks => Math.Ceiling(_animationDuration.TotalSeconds / FrameRate.TotalSeconds);
        private double Progress => _currentTicks / TotalTicks;

        public AnimatedScaleControl()
        {
            _timer = new DispatcherTimer()
            {
                Interval = FrameRate
            };
            _timer.Tick += AnimationTick;
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            _normalVolumeContainer = e.NameScope.Get<Rectangle>("NormalVolumeContainer");
            _highVolumeContainer = e.NameScope.Get<Rectangle>("HighVolumeContainer");
            base.OnApplyTemplate(e);
        }

        private void AnimationTick(object sender, EventArgs e)
        {
            if (_currentTicks >= TotalTicks)
            {
                _timer.Stop();
                _currentTicks = 0;
                _prevNormalVolume = _normalVolumeToChange.Round();
                _prevHighVolume = _highVolumeToChange.Round();
                _isAnimationInProgress = false;
            }

            if (_prevHighVolume == 0 && _highVolumeToChange == 0)
            {
                _normalVolumeContainer.Height = _prevNormalVolume + ((_normalVolumeToChange - _prevNormalVolume) * Progress);
                _highVolumeContainer.Height = 0;
            }
            else if(_prevNormalVolume < NormalVolumeLimit && _prevHighVolume == 0 && _highVolumeToChange > 0)
            {
                var valueToGo = (NormalVolumeLimit - _prevNormalVolume) + _highVolumeToChange;
                var valueInProgress = Progress * valueToGo;
                if ((_prevNormalVolume + valueInProgress) <= NormalVolumeLimit)
                {
                    _normalVolumeContainer.Height = _prevNormalVolume + valueInProgress;
                }
                else
                {
                    _normalVolumeContainer.Height = NormalVolumeLimit;
                    var normalArea = (NormalVolumeLimit - _prevNormalVolume);
                    _highVolumeContainer.Height = valueInProgress - normalArea;
                }
            } 
            else if(_prevNormalVolume >= NormalVolumeLimit && _prevHighVolume == 0 && _highVolumeToChange > 0)
            {
                var valueToGo = _highVolumeToChange;
                var valueInProgress = Progress * valueToGo;
                _normalVolumeContainer.Height = NormalVolumeLimit;
                _highVolumeContainer.Height = valueInProgress;
            }
            else if(_prevNormalVolume >= NormalVolumeLimit && _prevHighVolume < _highVolumeToChange)
            {
                var valueToTo = _highVolumeToChange - _prevHighVolume;
                var valueInProgress = valueToTo * Progress;
                _highVolumeContainer.Height = _prevHighVolume + valueInProgress;
            }
            else if (_prevHighVolume > _highVolumeToChange)
            {
                var valueToGo = (NormalVolumeLimit - _normalVolumeToChange) + _prevHighVolume;
                var valueInProgress = Progress * valueToGo;
                //Изменяем и высокую громкость и нормальную
                if (_normalVolumeToChange < NormalVolumeLimit)
                {
                    if (valueInProgress >= (_prevHighVolume - _highVolumeToChange))
                    {
                        _highVolumeContainer.Height = 0;
                        _normalVolumeContainer.Height = _normalVolumeToChange + (_prevHighVolume + ((NormalVolumeLimit - _normalVolumeToChange) - valueInProgress));
                    }
                    else
                    {
                        _highVolumeContainer.Height = _prevHighVolume - valueInProgress;
                    }
                }
                //Изменяем только высокую
                else
                {
                    _highVolumeContainer.Height = _prevHighVolume - ((_prevHighVolume - _highVolumeToChange) * Progress);
                }
            }

            _currentTicks++;
        }

        private void StartAnimation()
        {
            _isAnimationInProgress = true;
            _normalVolumeToChange = NormalVolume.Round();
            _highVolumeToChange = HighVolume.Round();
            _timer.Start();
        }
    }
}
