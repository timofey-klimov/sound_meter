using Avalonia.Threading;
using SoundMeter.UI.Messages;
using SoundMeter.UI.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoundMeter.UI.ViewModels
{
    internal class LoundesInfoControlViewModel : ViewModelBase, IEventBusSubscriber
    {
        private readonly DispatcherTimer _dispatcherTimer;
        private Queue<double> _lufsQueue = new Queue<double>();
        private Queue<double> _maxLufsQueue = new Queue<double>();

        #region Lufs
        private string _lufs = "-100";
        public string Lufs
        {
            get => _lufs;
            set
            {
                _lufs = value;
                RaiseEvent(nameof(Lufs));
            }
        }
        #endregion

        #region MomentaryMax
        private string _momentaryMax = "-100";
        public string MomentaryMax
        {
            get => _momentaryMax;
            set
            {
                _momentaryMax = value;
                RaiseEvent(nameof(MomentaryMax));
            }
        }
        #endregion

        #region ShortTermMax
        private string _shortTermMax = "-100";
        public string ShortTermMax
        {
            get => _shortTermMax;
            set
            {
                _shortTermMax = value;
                RaiseEvent(nameof(ShortTermMax));
            }
        }

        #endregion

        private string _loudnesRange;
        public string LoundesRange
        {
            get => _loudnesRange;
            set
            {
                _loudnesRange = value;
                RaiseEvent(nameof(LoundesRange));
            }
        }

        public LoundesInfoControlViewModel(IEventBus eventBus)
        {
            _dispatcherTimer = new DispatcherTimer()
            {
                Interval = AppSettings.FrameRate
            };
            _dispatcherTimer.Tick += (s, e) =>
            {
                if (_lufsQueue.Count > 0)
                {
                    var currentMax = _lufsQueue.Max().Round(1);
                    var currentMin = _lufsQueue.Min().Round(1);
                    Lufs = _lufsQueue.Average().Round(1).ToString();
                    MomentaryMax = currentMax.ToString();
                    ShortTermMax = _maxLufsQueue.Max().Round(1).ToString();
                    LoundesRange = $"{currentMin}  {currentMax}";
                }
                else
                {
                    Lufs = "-100";
                    MomentaryMax = "-100";
                    ShortTermMax = "-100";
                    LoundesRange = "-100";
                }    
            };
            _dispatcherTimer.Start();
            eventBus.On<UpdateLufsMessage>(this, OnUpdateLufsMessageAsync);
        }

        private async Task OnUpdateLufsMessageAsync(UpdateLufsMessage message)
        {
            _lufsQueue.Enqueue(message.Value);
            if (_lufsQueue.Count > 0)
            {
                _maxLufsQueue.Enqueue(_lufsQueue.Max());
            }

            if (_maxLufsQueue.Count > 400)
            {
                for (int i = 0; i < 70; i++)
                    _maxLufsQueue.Dequeue();
            }

            if (_lufsQueue.Count >= 100)
            {
                for (int i = 0; i < 10; i++)
                    _lufsQueue.Dequeue();
            }
        }
    }
}
