using SoundMeter.UI.Services;

namespace SoundMeter.UI.Messages
{
    internal record UpdateLufsMessage(double Value) : IEventMessage;
}
