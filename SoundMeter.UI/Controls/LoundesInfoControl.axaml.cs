using Avalonia.Controls.Primitives;
using SoundMeter.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundMeter.UI.Controls
{
    internal class LoundesInfoControl : TemplatedControl
    {
        public LoundesInfoControl()
        {
            DataContext = ServiceLocator.GetRequiredSerivce<LoundesInfoControlViewModel>();
        }
    }
}
