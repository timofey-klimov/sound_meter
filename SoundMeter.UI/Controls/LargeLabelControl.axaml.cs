using Avalonia;
using Avalonia.Controls.Primitives;

namespace SoundMeter.UI.Controls;

public class LargeLabelControl : TemplatedControl
{
    public static readonly StyledProperty<string> LargeTextProperty =
        AvaloniaProperty.Register<LargeLabelControl, string>(nameof(LargeText), defaultValue: string.Empty);

    public string LargeText
    {
        get => this.GetValue(LargeTextProperty);
        set => SetValue(LargeTextProperty, value);
    }


    public static readonly StyledProperty<string> SmallTextProperty =
        AvaloniaProperty.Register<LargeLabelControl, string>(nameof(SmallText), defaultValue: string.Empty);

    public string SmallText
    {
        get => this.GetValue(SmallTextProperty);
        set => SetValue(SmallTextProperty, value);
    }

}