using Avalonia.Threading;
using SoundMeter.UI.Messages;
using SoundMeter.UI.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoundMeter.UI.ViewModels
{
    internal class SoundVolumeScaleViewModel : ViewModelBase, IEventBusSubscriber
    {
        private const double MinimumVolume = -62;
        private const double VolumeLimit = -37;
        private readonly IEventBus _eventBus;
        private double _actualHeight;
        private DispatcherTimer _timer = new DispatcherTimer();
        private Queue<double> _lufsQueue = new Queue<double>();
        private double NormalLimitPercentage => VolumeLimit / MinimumVolume;
        public double NormalVolumeLimit => (_actualHeight * NormalLimitPercentage);
        private bool _start;

        #region CurrentVolume
        private double _currentVolume = 0;
        public double CurrentVolume
        {
            get
            {
                return _currentVolume;
            }
            set
            {
                _currentVolume = value;
                RaiseEvent(nameof(CurrentVolume));
            }
        }
        #endregion

        #region NormalVolume

        private double _normalVolume;
        public double NormalVolume
        {
            get => _normalVolume;
            set
            {
                _normalVolume = value;
                RaiseEvent(nameof(NormalVolume));
            }
        }
        #endregion

        #region HighVolume
        private double _highVolume;
        public double HighVolume
        {
            get => _highVolume;
            set
            {
                _highVolume = value;
                RaiseEvent(nameof(HighVolume));
            }
        }
        #endregion

        public SoundVolumeScaleViewModel(IEventBus eventBus)
        {
            _eventBus = eventBus;
            _timer.Interval = AppSettings.FrameRate;
            _timer.Tick += async (s, e) =>
            {
                if (!_start)
                    return;
                if (_lufsQueue.Count > 0)
                {
                    var average = _lufsQueue.Average();
                    var percentage = average / MinimumVolume;
                    var currentVolume = (_actualHeight * percentage).Round(1);
                    CurrentVolume = currentVolume - 15;
                    var volumeLitimPercentage = 1 - percentage;
                    if (volumeLitimPercentage <= NormalLimitPercentage)
                    {
                        var prevHigh = HighVolume;
                        HighVolume = 0.0;
                        NormalVolume = _actualHeight - currentVolume;
                    }
                    else
                    {
                        NormalVolume = NormalVolumeLimit;
                        var highVolume = (_actualHeight - currentVolume - NormalVolumeLimit);
                        HighVolume = highVolume;
                    }
                }
                else
                    CurrentVolume = _actualHeight;
            };
            _timer.Start();
            _eventBus.On<UpdateLufsMessage>(this, OnUpdateLufsMessageAsync);
        }

        public void UpdateActualHeight(double actualHeight)
        {
            _actualHeight = actualHeight;
            RaiseEvent(nameof(NormalVolumeLimit));
            if (!_start && actualHeight > 0)
            {
                Dispatcher.UIThread.Post(() => CurrentVolume = actualHeight);
            }
            
        }

        private async Task OnUpdateLufsMessageAsync(UpdateLufsMessage message)
        {
            if (!_start)
            {
                _timer.Start();
                _start = true;
            }
            var lufs = message.Value;
            if (lufs <= MinimumVolume)
                lufs = MinimumVolume;

            _lufsQueue.Enqueue(lufs.Round(2));

            if (_lufsQueue.Count == 100)
            {
                for (int i = 0; i < 10; i++)
                    _lufsQueue.Dequeue();
            }
        }
    }
}
