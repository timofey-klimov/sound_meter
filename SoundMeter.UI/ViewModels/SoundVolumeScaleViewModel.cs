using Avalonia.Threading;
using SoundMeter.UI.Messages;
using SoundMeter.UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoundMeter.UI.ViewModels
{
    internal class SoundVolumeScaleViewModel : ViewModelBase, IEventBusSubscriber
    {
        private const double MinimumVolume = -65;
        private readonly IEventBus _eventBus;
        private double _actualHeight;
        private double _currentVolume = 0;
        private DispatcherTimer _timer = new DispatcherTimer();
        private Queue<double> _volumeQueue = new Queue<double>();
        private double _prevVolume;
        public double CurrentVolume
        {
            get => _currentVolume;
            set
            {
                _currentVolume = value;
                RaiseEvent(nameof(CurrentVolume));
            }
        }

        public SoundVolumeScaleViewModel(IEventBus eventBus)
        {
            _eventBus = eventBus;
            _timer.Interval = TimeSpan.FromSeconds(1.0 / 144.0);
            _timer.Tick += (s, e) =>
            {
                if (_volumeQueue.Count > 0)
                    CurrentVolume = _volumeQueue.Average();
                else
                    CurrentVolume = _actualHeight;
            };
            //FIXME
            _timer.Start();
            _eventBus.On<UpdateLufsMessage>(this, message =>
            {
                var lufs = message.Value;
                if (lufs <= MinimumVolume)
                    lufs = MinimumVolume;
                var percentage = lufs / MinimumVolume;
                _volumeQueue.Enqueue(Math.Round((_actualHeight * percentage), 1));
                if (_volumeQueue.Count == 100)
                {
                    for (int i = 0; i < 10; i++)
                        _volumeQueue.Dequeue();
                }
            });
        }

        public void UpdateActualHeight(double actualHeight) => _actualHeight = actualHeight;
    }
}
