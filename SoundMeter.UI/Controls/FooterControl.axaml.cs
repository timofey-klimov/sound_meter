using Avalonia.Controls.Primitives;
using SoundMeter.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundMeter.UI.Controls
{
    internal class FooterControl : TemplatedControl
    {
        public FooterControl()
        {
            DataContext = ServiceLocator.GetRequiredSerivce<FooterViewModel>();
        }
    }
}
