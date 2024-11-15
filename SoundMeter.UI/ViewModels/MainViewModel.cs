using Avalonia.Threading;
using SoundMeter.Core.Services;
using SoundMeter.UI.Messages;
using SoundMeter.UI.Models;
using SoundMeter.UI.Services;
using System;
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
        }

        private async Task LoadAudioInterfaces(bool resetCache = false)
        {
            var audioInterfaces = await _audioInterfaceService.GetAudioInterfacesAsync(resetCache);
            AudioInterfaces = new ObservableCollection<AudioInterface>(audioInterfaces.ToList());
        }

        private void SelectAudioInterface(object audioInterface)
        {
            var castAudioInterface = (AudioInterface)audioInterface;
            _eventBus.Publish(new SelectAudioIntefaceMessage(castAudioInterface));
            if (castAudioInterface.Index == _selectedAudioInterface?.Index)
                return;
            _selectedAudioInterface = castAudioInterface;
            _listener.ListenAsync(_selectedAudioInterface.Value.Index);
            
        }

        private async Task ProcessMessages()
        {
            while (true)
            {
                var message = await _messageProcessor.Processor.ReadAsync();
                var lufs = _loudnesService.Calculate(message.Data, message.SampleRate);
                _eventBus.Publish(new UpdateLufsMessage(lufs));
            }
        }
    }
}
