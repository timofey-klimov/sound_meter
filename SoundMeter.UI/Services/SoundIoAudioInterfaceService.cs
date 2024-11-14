using SoundMeter.Core.Services;
using SoundMeter.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundMeter.UI.Services
{
    internal class SoundIoAudioInterfaceService : IAudioInterfaceService
    {
        private readonly ISoundIoClient _client;
        public SoundIoAudioInterfaceService(ISoundIoClient client)
        {
            _client = client;
        }
        public async Task<IEnumerable<AudioInterface>> GetAudioInterfacesAsync(bool resetCache = false)
        {
            var inputDevices = await _client.GetDevicesAsync(resetCache);

            return inputDevices.Select(x => new AudioInterface(x.Index, x.FullName));
        }
    }
}
