using NWaves.Signals;
using NWaves.Utils;

namespace SoundMeter.Core.Services
{
    public interface ILoudnesService
    {
        double Calculate(ArraySegment<float> bytes, int sampleRate);
    }
    public class LoudnesService : ILoudnesService
    {
        public double Calculate(ArraySegment<float> bytes, int sampleRate)
        {
            var signal = new DiscreteSignal(sampleRate, bytes.Count);
            for (int i = 0; i < bytes.Count; i++)
            {
                signal[i] = bytes[i];
            }

           return Math.Round(Scale.ToDecibel(signal.Rms()), 2);
        }
    }
}
