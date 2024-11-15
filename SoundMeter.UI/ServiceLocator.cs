using Microsoft.Extensions.DependencyInjection;
using SoundMeter.Core.Models;
using SoundMeter.Core.Services;
using SoundMeter.UI.Services;
using SoundMeter.UI.ViewModels;
using System;

namespace SoundMeter.UI
{
    public static class ServiceLocator
    {
        private static IServiceProvider _serviceProvider;
        public static void Init()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<MainViewModel>();
            serviceCollection.AddSingleton<IAudioInterfaceService, SoundIoAudioInterfaceService>();
            serviceCollection.AddSingleton<ISoundIoClient, SoundIoClient>(sp =>
            {
                var soundIo = new SoundIO();
                soundIo.Connect();
                return new SoundIoClient(soundIo);
            });
            serviceCollection.AddSingleton<InputDeviceService>();
            serviceCollection.AddSingleton<IInputDeviceListener, InputDeviceService>(sp =>
            {
                var service = sp.GetRequiredService<InputDeviceService>();
                return service;
            });
            serviceCollection.AddSingleton<IInputDeviceMessageProcessor, InputDeviceService>(sp =>
            {
                var service = sp.GetRequiredService<InputDeviceService>();
                return service;
            });

            serviceCollection.AddSingleton<PopupStateManagerService>();
            serviceCollection.AddSingleton<IPopupStateManagerService>(sp => sp.GetRequiredService<PopupStateManagerService>());
            serviceCollection.AddSingleton<IPopupStateChanged>(sp => sp.GetRequiredService<PopupStateManagerService>());

            serviceCollection.AddSingleton<HeaderViewModel>();
            serviceCollection.AddSingleton<SoundVolumeScaleViewModel>();
            serviceCollection.AddSingleton<FooterViewModel>();
            serviceCollection.AddSingleton<LoundesInfoControlViewModel>();
            serviceCollection.AddSingleton<IEventBus, EventBus>();
            serviceCollection.AddSingleton<ILoudnesService, LoudnesService>();
            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        public static T GetRequiredSerivce<T>() => _serviceProvider.GetRequiredService<T>();
    }
}
