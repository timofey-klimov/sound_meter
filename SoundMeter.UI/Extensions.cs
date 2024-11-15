using System;

namespace SoundMeter.UI
{
    internal static class Extensions
    {
        public static double Round(this double value, int nums = 1) => Math.Round(value, nums);
    }
}
