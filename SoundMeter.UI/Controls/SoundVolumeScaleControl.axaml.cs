using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Threading;
using SoundMeter.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundMeter.UI.Controls
{
    internal class SoundVolumeScaleControl : TemplatedControl
    {
        private Canvas? _control;
        private readonly SoundVolumeScaleViewModel _viewModel;
        public SoundVolumeScaleControl()
        {
            _viewModel = ServiceLocator.GetRequiredSerivce<SoundVolumeScaleViewModel>();
            DataContext = _viewModel;
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            var control = e.NameScope.Get<Canvas>("VolumeContainer");
            if (control != null)
            {
                _control = control;
            }
        }

        public override void Render(DrawingContext context)
        {
            _viewModel.UpdateActualHeight(_control.Bounds.Height);
            base.Render(context);
        }
    }
}
