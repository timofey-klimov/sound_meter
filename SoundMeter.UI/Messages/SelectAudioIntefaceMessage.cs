using SoundMeter.UI.Models;
using SoundMeter.UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundMeter.UI.Messages
{
    internal record SelectAudioIntefaceMessage(AudioInterface AudioInterface) : IEventMessage;
    
}
