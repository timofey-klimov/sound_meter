using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundMeter.Core.SoundModels
{
    public struct InputDevice
    {
        public int Index { get; }

        public string FullName { get; }

        public InputDevice(int index, string fullName)
        {
            Index = index;
            FullName = fullName;
        }
    }
}
