using NWaves.Signals;
using NWaves.Utils;

namespace SoundMeter.Core.Services
{
    public interface ILoudnesService
    {
        double Calculate(IEnumerable<float> bytes, int sampleRate);
    }
    public class LoudnesService : ILoudnesService
    {
        public double Calculate(IEnumerable<float> bytes, int sampleRate)
        {
            var length = 0;
            if(!bytes.TryGetNonEnumeratedCount(out length))
            {
                length = bytes.Count();
            }
            var signal = new DiscreteSignal(sampleRate, length);
            for (int i = 0; i < length; i++)
            {
                signal[i] = bytes.ElementAt(i);
            }

           return Math.Round(Scale.ToDecibel(signal.Rms()), 2);
        }
    }
}
