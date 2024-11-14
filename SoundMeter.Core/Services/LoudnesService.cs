using NWaves.Signals;
using NWaves.Utils;

namespace SoundMeter.Core.Services
{
    public interface ILoudnesService
    {
        double Calculate(float[] bytes, int sampleRate);
    }
    public class LoudnesService : ILoudnesService
    {
        public double Calculate(float[] bytes, int sampleRate)
        {
            var signal = new DiscreteSignal(sampleRate, bytes.Length);
            for (int i = 0; i < bytes.Length; i++)
            {
                signal[i] = bytes[i];
            }

           return Scale.ToDecibel(signal.Rms());
        }
    }
}
