﻿using System;
namespace SoundMeter.Core.Models
{
	public struct SoundIOSampleRateRange
	{
		internal SoundIOSampleRateRange (int min, int max)
		{
			Min = min;
			Max = max;
		}

		public readonly int Min;
		public readonly int Max;
	}
}