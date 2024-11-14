using Avalonia.Controls.Primitives;
using SoundMeter.UI.ViewModels;

namespace SoundMeter.UI.Controls
{
    internal class HeaderControl : TemplatedControl
    {
        public HeaderControl()
        {
            DataContext = ServiceLocator.GetRequiredSerivce<HeaderViewModel>();
        }
    }
}
