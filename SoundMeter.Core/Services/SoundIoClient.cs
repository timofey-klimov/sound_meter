using SoundMeter.Core.Models;
using SoundMeter.Core.SoundModels;

namespace SoundMeter.Core.Services
{
    public interface ISoundIoClient
    {
        Task<SoundIOInStream> CreateDeviceStreamAsync(int deviceId, double latency);
        Task<IEnumerable<InputDevice>> GetDevicesAsync(bool resetCache = false);
        void WaitEvents();
    }

    public class SoundIoClient : ISoundIoClient
    {
        static SoundIOFormat[] prioritized_formats = {
            SoundIODevice.Float32LE,
            SoundIODevice.Float32FE,
            SoundIODevice.S32NE,
            SoundIODevice.S32FE,
            SoundIODevice.S24NE,
            SoundIODevice.S24FE,
            SoundIODevice.S16NE,
            SoundIODevice.S16FE,
            SoundIODevice.Float64LE,
            SoundIODevice.Float64FE,
            SoundIODevice.U32NE,
            SoundIODevice.U32FE,
            SoundIODevice.U24NE,
            SoundIODevice.U24FE,
            SoundIODevice.U16NE,
            SoundIODevice.U16FE,
            SoundIOFormat.S8,
            SoundIOFormat.U8,
            SoundIOFormat.Invalid,
        };

        static readonly int[] prioritized_sample_rates = {
            48000,
            44100,
            96000,
            24000,
            0,
        };
        private readonly SoundIO _soundIo;
        private Dictionary<int, SoundIODevice> _devices;
        public SoundIoClient(SoundIO soundIO)
        {
            _soundIo = soundIO;
            _devices = new Dictionary<int, SoundIODevice>();
        }

        public async Task<SoundIOInStream> CreateDeviceStreamAsync(int deviceId, double latency)
        {
            return await Task.Run(() =>
            {
                var device = _devices[deviceId];
                var fmt = prioritized_formats.FirstOrDefault(device.SupportsFormat);
                var instream = device.CreateInStream();
                var sample_rate = prioritized_sample_rates.FirstOrDefault(device.SupportsSampleRate);
                instream.Format = fmt;
                instream.SampleRate = sample_rate;
                instream.SoftwareLatency = latency;
                return instream;
            });
        }

        public void WaitEvents() => _soundIo.WaitEvents();

        public async Task<IEnumerable<InputDevice>> GetDevicesAsync(bool resetCache = false)
        {
            if (_devices.Count == 0 || resetCache)
            {
                _devices = await CreateDeviceTableAsync();
            }

            return _devices.Select(pair => new InputDevice(pair.Key, pair.Value.Name));
        }
        public void Dispose()
        {
            _soundIo.Dispose();
            GC.SuppressFinalize(this);
        }

        private async Task<Dictionary<int, SoundIODevice>> CreateDeviceTableAsync()
        {
            return await Task.Run(() =>
            {
                _soundIo.FlushEvents();
                var dict = new Dictionary<int, SoundIODevice>();

                for (int i = 0; i < _soundIo.InputDeviceCount; i++)
                {
                    var device = _soundIo.GetInputDevice(i);
                    if (!device.IsRaw)
                        dict.Add(i, device);
                }
                return dict;
                    
            });
        }
    }
}
