using SoundMeter.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundMeter.UI.Services
{
    internal interface IAudioInterfaceService
    {
        Task<IEnumerable<AudioInterface>> GetAudioInterfacesAsync(bool resetCache = false);
    }
}
