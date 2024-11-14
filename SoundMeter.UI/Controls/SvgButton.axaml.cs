using Avalonia.Controls;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundMeter.UI.Controls
{
    public class SvgButton : TemplatedControl
    {
        public static readonly StyledProperty<string> SvgPathProperty =
            AvaloniaProperty.Register<SvgButton, string>(
            nameof(SvgPath),
            defaultValue: string.Empty,
            defaultBindingMode: BindingMode.TwoWay);

        public string SvgPath
        {
            get => GetValue(SvgPathProperty);
            set => SetValue(SvgPathProperty, value);
        }
    }
}
