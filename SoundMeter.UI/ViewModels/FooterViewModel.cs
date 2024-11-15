using SoundMeter.UI.Messages;
using SoundMeter.UI.Models;
using SoundMeter.UI.Services;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SoundMeter.UI.ViewModels
{
    internal class FooterViewModel : ViewModelBase, IEventBusSubscriber
    {
        private const string PopupName = "AudioInterfaceSelectorPopup";
        private readonly IEventBus _eventBus;
        public ICommand ToggleChannelConfigurationCommand { get; set; }
        public ICommand RefreshCommand { get; set; }

        private AudioInterface? _selectedAudioInterface;

        public string SelectedAudioInterfaceButtonName
        {
            get => _selectedAudioInterface?.FullName ??  "Выберите аудио интерфейс";
        }

        private readonly IPopupStateManagerService _popupStateManager;
        public FooterViewModel(IPopupStateManagerService popupStateManager, IEventBus eventBus)
        {
            _popupStateManager = popupStateManager;
            _eventBus = eventBus;
            ToggleChannelConfigurationCommand = new Command(ToggleChannelConfiguration);
            RefreshCommand = new Command(Refresh);
            _eventBus.On<SelectAudioIntefaceMessage>(this, OnAudioInterfaceSelectedAsync);
        }

        private void ToggleChannelConfiguration(object x) => _popupStateManager.Open(PopupName);
        private void Refresh(object o) => _eventBus.PublishAsync(new RefreshDevicesMessage());
        private async Task OnAudioInterfaceSelectedAsync(SelectAudioIntefaceMessage message)
        {
            if (_selectedAudioInterface?.Index != message.AudioInterface.Index)
                _selectedAudioInterface = message.AudioInterface;
            RaiseEvent(nameof(SelectedAudioInterfaceButtonName));
            _popupStateManager.Close(PopupName);
        }
    }
}
