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
    internal class SoundVolumeScaleControl : TemplatedControl
    {
        private Canvas? _control;
        private readonly SoundVolumeScaleViewModel _viewModel;
        private double _prevActualHeight;

        public SoundVolumeScaleControl()
        {
            _viewModel = ServiceLocator.GetRequiredSerivce<SoundVolumeScaleViewModel>();
            DataContext = _viewModel;
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            _control = e.NameScope?.Get<Canvas>("VolumeContainer");
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
