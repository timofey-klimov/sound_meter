using Avalonia.Threading;
using SoundMeter.Core.Services;
using SoundMeter.UI.Messages;
using SoundMeter.UI.Models;
using SoundMeter.UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoundMeter.UI.ViewModels
{
    internal class SoundVolumeScaleViewModel : ViewModelBase, IEventBusSubscriber
    {
        private readonly IInputDeviceListener _listener;
        private readonly IInputDeviceMessageProcessor _messageProcessor;
        private readonly ILoudnesService _loudnesService;
        private AudioInterface? _selectedAudioInterface;


        public SoundVolumeScaleViewModel(
            IInputDeviceListener listener,
            IInputDeviceMessageProcessor messageProcessor,
            IEventBus eventBus,
            ILoudnesService loundnesService)
        {
            _listener = listener;
            _messageProcessor = messageProcessor;
            _loudnesService = loundnesService;
        }
    }
}
