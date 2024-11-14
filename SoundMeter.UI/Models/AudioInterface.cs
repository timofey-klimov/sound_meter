namespace SoundMeter.UI.Models
{
    internal struct AudioInterface
    {
        public AudioInterface(int index, string fullName)
        {
            Index = index;
            FullName = fullName;
        }
        
        public int Index { get; }

        public string FullName { get; }
    }
}
