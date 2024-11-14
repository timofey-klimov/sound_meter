using System;
using System.Runtime.InteropServices;

namespace SoundMeter.Core.Models
{
	public class SoundIOException : Exception
	{
		internal SoundIOException (SoundIoError errorCode)
			: base (Marshal.PtrToStringAnsi (Natives.soundio_strerror ((int) errorCode)))
		{
		}
	}
}
