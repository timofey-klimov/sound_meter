using SoundMeter.Core.Models;
using System.Buffers;
using System.Threading.Channels;

namespace SoundMeter.Core.Services
{
    public struct InputDeviceStreamData
    {
        public ArraySegment<byte> Data { get; }
        public int SampleRate { get; }

        public InputDeviceStreamData(ArraySegment<byte> data, int sampleRate)
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
        private Lazy<ArrayPool<byte>> _arrayPoolFactory = new Lazy<ArrayPool<byte>>(() => ArrayPool<byte>.Shared);
        private CancellationTokenSource? _cancellationTokenSource;
        public ChannelReader<InputDeviceStreamData> Processor => _channel.Reader;

        public InputDeviceService(ISoundIoClient soundIoClient)
        {
            _channel = Channel.CreateUnbounded<InputDeviceStreamData>();
            _soundIoClient = soundIoClient;
        }

        public async Task ListenAsync(int deviceId)
        {
            _currentStream?.Dispose();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
            _currentStream = await _soundIoClient.CreateDeviceStreamAsync(deviceId, .6);
            var sampleRate = _currentStream.SampleRate;
            _currentStream.ReadCallback = (min, max) => Process(min, max, sampleRate, _cancellationTokenSource.Token);
            
            _currentStream.Open();
            _currentStream.Start();

            if (_deviceId is null)
            {
                var task = new Task(() =>
                {
                    while (true)
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

                if (areas.HasValue && !areas.Value.IsEmpty)
                {
                    int copySize = _currentStream!.BytesPerSample;
                    for (int frame = 0; frame < frame_count; frame += 1)
                    {
                        if (token.IsCancellationRequested)
                            break;

                       
                        var area = areas.Value.GetArea(0);
                        var buffer = _arrayPoolFactory.Value.Rent(copySize);
                        fixed (byte* ptr = buffer)
                        {
                            Buffer.MemoryCopy((void*)area.Pointer, ptr, copySize, copySize);
                        }

                        var arraySegment = new ArraySegment<byte>(buffer, 0, copySize);
                            
                        _channel.Writer.WriteAsync(new InputDeviceStreamData(arraySegment, sampleRate));
                        area.Pointer += area.Step;

                        _arrayPoolFactory.Value.Return(buffer);
                    }

                    frames_left -= frame_count;
                    _currentStream.EndRead();
                }
            }
        }
    }
}
