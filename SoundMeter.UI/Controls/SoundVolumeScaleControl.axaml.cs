using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Threading;
using SoundMeter.UI.ViewModels;
using System;

namespace SoundMeter.UI.Controls
{
    public enum ScaleNextState { Up, Dowm }
    public record ScaleNextStateContext(ScaleNextState NextState, double NewValue, double OldValue, Rectangle ControlToChange);
    internal class SoundVolumeScaleControl : TemplatedControl
    {
        private Canvas? _control;
        private Rectangle? _highVolumeContainer;
        private Rectangle? _normalVolumeContainer;
        private readonly SoundVolumeScaleViewModel _viewModel;
        private double _prevActualHeight;
        private double _prevHighVolumeHeight;
        private double _prevNormalVolumeHeight;
        private double _currentTicks = 0.0;
        private ScaleNextStateContext _context;


        private TimeSpan _animationDuration = TimeSpan.FromMilliseconds(300);
        private TimeSpan FrameRate => AppSettings.FrameRate;
        private double HighVolumeHeight => _highVolumeContainer.Height;
        private double NormalVolumeHeight => _normalVolumeContainer.Height;

        private bool IsHighVolumeContainerChanges => HighVolumeHeight != _prevHighVolumeHeight;
        private bool IsNornmalVolumeContainerChanges => NormalVolumeHeight != _prevNormalVolumeHeight;
        private bool HasChages => IsHighVolumeContainerChanges || IsNornmalVolumeContainerChanges;

        private DoubleTransition _transition;

        public SoundVolumeScaleControl()
        {
            _transition = new DoubleTransition();
            _viewModel = ServiceLocator.GetRequiredSerivce<SoundVolumeScaleViewModel>();
            DataContext = _viewModel;
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            _control = e.NameScope?.Get<Canvas>("VolumeContainer");
            _highVolumeContainer = e.NameScope?.Get<Rectangle>("HighVolumeContainer");
            _normalVolumeContainer = e.NameScope?.Get<Rectangle>("NormalVolumeContainer");
        }

        public override void Render(DrawingContext context)
        {
            if (_prevActualHeight != _control.Bounds.Height)
            {
                _viewModel.UpdateActualHeight(_control.Bounds.Height);
                _prevActualHeight = _control.Bounds.Height;
            }

            base.Render(context);
        }
    }
}
