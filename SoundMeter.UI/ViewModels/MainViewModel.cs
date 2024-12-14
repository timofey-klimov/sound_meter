using SoundMeter.Core.Services;
using SoundMeter.UI.Messages;
using SoundMeter.UI.Models;
using SoundMeter.UI.Services;
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

        private AudioInterface? _selectedAudioInterface;
        public ICommand SelectAudioInterfaceCommand { get; }

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
            
            LoadAudioInterfaces();
            ProcessMessages();
        }

        private async Task LoadAudioInterfaces()
        {
            var audioInterfaces = (await _audioInterfaceService.GetAudioInterfacesAsync(true)).ToList();
            AudioInterfaces = new ObservableCollection<AudioInterface>(audioInterfaces);
        }

        private void SelectAudioInterface(object audioInterface)
        {
            var castAudioInterface = (AudioInterface)audioInterface;
            _eventBus.PublishAsync(new SelectAudioIntefaceMessage(castAudioInterface));
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
                await _eventBus.PublishAsync(new UpdateLufsMessage(lufs));
            }
        }
    }
}
