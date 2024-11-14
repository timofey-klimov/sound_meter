using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SoundMeter.Core.Models;
using SoundMeter.Core.Services;
using SoundMeter.UI.Services;
using SoundMeter.UI.ViewModels;
using System;

namespace SoundMeter.UI
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void RegisterServices()
        {
            ServiceLocator.Init();
            base.RegisterServices();
        }
        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow()
                {
                    DataContext = ServiceLocator.GetRequiredSerivce<MainViewModel>()
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}