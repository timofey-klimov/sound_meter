using Avalonia.Threading;
using SoundMeter.Core.Services;
using SoundMeter.UI.Messages;
using SoundMeter.UI.Models;
using SoundMeter.UI.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SoundMeter.UI.ViewModels
{
    internal class MainViewModel : ViewModelBase, IEventBusSubscriber
    {
        private readonly IAudioInterfaceService _audioInterfaceService;
        private readonly IEventBus _eventBus;
        private readonly IInputDeviceListener _listener;
        private readonly IInputDeviceMessageProcessor _messageProcessor;
        private readonly ILoudnesService _loudnesService;
        private ObservableCollection<AudioInterface> _audioInterfaces;
        private Queue<double> _volumes = new Queue<double>();
        private DispatcherTimer _timer;

        public ObservableCollection<AudioInterface> AudioInterfaces
        {
            get => _audioInterfaces;
            set
            {
                _audioInterfaces = value;
                RaiseEvent(nameof(AudioInterfaces));
            }
        }

        public ICommand SelectAudioInterfaceCommand { get; }
        private AudioInterface? _selectedAudioInterface;

        private string _lufs;
        public string Lufs
        {
            get => _lufs;
            set
            {
                _lufs = value;
                RaiseEvent(nameof(Lufs));
            }

        }

        private readonly float[] _buffer = new float[128];

        public MainViewModel(
            IAudioInterfaceService audioInterfaceService, 
            IEventBus eventBus, 
            IInputDeviceListener listener,
            IInputDeviceMessageProcessor messageProcessor,
            ILoudnesService loundnesService)
        {
            _audioInterfaceService = audioInterfaceService;
            _listener = listener;
            _messageProcessor = messageProcessor;
            _loudnesService = loundnesService;
            _eventBus = eventBus;
            SelectAudioInterfaceCommand = new Command(SelectAudioInterface);
            _eventBus.On<RefreshDevicesMessage>(this, (m) =>
            {
                LoadAudioInterfaces(true);
            });
            LoadAudioInterfaces();
            ProcessMessages();
            _timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(1 / 144)
            };
            _timer.Tick += (s, e) =>
            {
                Lufs = _volumes.Count > 5 ? _volumes.Average().ToString() : "0";
            };
        }

        private async Task LoadAudioInterfaces(bool resetCache = false)
        {
            var audioInterfaces = await _audioInterfaceService.GetAudioInterfacesAsync(resetCache);
            AudioInterfaces = new ObservableCollection<AudioInterface>(audioInterfaces.ToList());
        }

        private void SelectAudioInterface(object audioInterface)
        {
            var castAudioInterface = (AudioInterface)audioInterface;
            if (castAudioInterface.Index == _selectedAudioInterface?.Index)
                return;
            _selectedAudioInterface = castAudioInterface;
            _eventBus.Publish(new SelectAudioIntefaceMessage(_selectedAudioInterface.Value));
            _listener.ListenAsync(_selectedAudioInterface.Value.Index).ContinueWith(x => _timer.Start());
            
        }

        private async Task ProcessMessages()
        {
            while (true)
            {
                int rate = 0;
                for (int i = 0; i < 128; i++)
                {
                    var message = await _messageProcessor.Processor.ReadAsync();
                    float value = BitConverter.ToSingle(message.Data);
                    _buffer[i] = value;
                    if (i == 127)
                        rate = message.SampleRate;
                }
                var lufs = _loudnesService.Calculate(_buffer, rate);
                if (_volumes.Count < 70)
                    _volumes.Enqueue(double.IsInfinity(lufs) ? -80.0 : lufs);
                if (_volumes.Count > 25)
                    _volumes.Dequeue();
                await Dispatcher.UIThread.InvokeAsync(() => Lufs = lufs.ToString());
            }
        }
    }
}
