using SoundMeter.Core.Models;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace SoundMeter.Core.Services
{
    public struct InputDeviceStreamData
    {
        public ArraySegment<float> Data { get; }
        public int SampleRate { get; }

        public InputDeviceStreamData(ArraySegment<float> data, int sampleRate)
        {
            Data = data;
            SampleRate = sampleRate;
        }
    }
    public interface IInputDeviceListener
    {
        Task ListenAsync(int deviceId);
    }

    public interface IInputDeviceMessageProcessor
    {
        ChannelReader<InputDeviceStreamData> Processor { get; }
    }
    public class InputDeviceService : IInputDeviceListener, IInputDeviceMessageProcessor
    {
        private int? _deviceId;
        private readonly Channel<InputDeviceStreamData> _channel;
        private readonly ISoundIoClient _soundIoClient;
        private SoundIOInStream? _currentStream;
        private Lazy<ArrayPool<float>> _arrayPoolFactory = new Lazy<ArrayPool<float>>(() => ArrayPool<float>.Shared);
        private CancellationTokenSource? _cancellationTokenSource;
        public ChannelReader<InputDeviceStreamData> Processor => _channel.Reader;
        private AutoResetEvent _autoResetEvent = new AutoResetEvent(true);

        public InputDeviceService(ISoundIoClient soundIoClient)
        {
            _channel = Channel.CreateUnbounded<InputDeviceStreamData>();
            _soundIoClient = soundIoClient;
        }

        public async Task ListenAsync(int deviceId)
        {
            _cancellationTokenSource?.Cancel();
            _currentStream?.Dispose();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
            _currentStream = await _soundIoClient.CreateDeviceStreamAsync(deviceId, .2);
            var sampleRate = _currentStream.SampleRate;
            _currentStream.ReadCallback = (min, max) =>
            {
                Process(min, max, sampleRate, _cancellationTokenSource.Token);
                _autoResetEvent.WaitOne();
            }; 
            
            _currentStream.Open();
            _currentStream.Start();

            if (_deviceId is null)
            {
                var task = new Task(() =>
                {
                    _soundIoClient.WaitEvents();
                }, TaskCreationOptions.LongRunning);
                task.Start();
            }
            _deviceId = deviceId;
        }

        private unsafe void Process(int min, int max, int sampleRate, CancellationToken token)
        {
            int frames_left = max;

            while (frames_left > 0)
            {
                int frame_count = frames_left;

                var areas = _currentStream?.BeginRead(ref frame_count);
                var bufferlength = frame_count;
                var buffer = _arrayPoolFactory.Value.Rent(bufferlength);
                if (areas.HasValue && !areas.Value.IsEmpty)
                {
                    nint copySize = _currentStream!.BytesPerSample;
                    uint counter = 0;
                    for (int frame = 0; frame < frame_count; frame += 1)
                    {
                        if (token.IsCancellationRequested)
                            break;
                        var area = areas.Value.GetArea(0);
                        var data = Unsafe.ReadUnaligned<float>((void*)area.Pointer);
                        
                        buffer[counter] = data;    
                        area.Pointer += copySize;

                        if (counter == bufferlength - 1)
                        {
                            counter = 0;
                            _channel.Writer.WriteAsync(
                                new InputDeviceStreamData(
                                    new ArraySegment<float>(buffer, 0, bufferlength), sampleRate));
                        }
                        counter++;
                    }

                    frames_left -= frame_count;
                    _arrayPoolFactory.Value.Return(buffer);
                    _currentStream.EndRead();
                }
            }
            _autoResetEvent.Set();
        }
    }
}
