using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SoundMeter.UI.ViewModels
{
    internal class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public void RaiseEvent([CallerMemberName] string? memberName = default) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberName));
    }
}
