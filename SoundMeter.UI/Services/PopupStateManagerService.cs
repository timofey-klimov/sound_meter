using System;

namespace SoundMeter.UI.Services
{
    public enum PopupState
    {
        Open,Close
    }

    public struct PopupCommand
    {
        public PopupState State { get; }

        public string Name { get; }

        public PopupCommand(string name, PopupState state)
        {
            State = state;
            Name = name;
        }
    }

    public interface IPopupStateChanged
    {
        Action<PopupCommand> OnActionCallback { get; set; }

    }

    public interface IPopupStateManagerService
    {
        void Open(string name);
        void Close(string name);
    }
    internal class PopupStateManagerService : IPopupStateManagerService, IPopupStateChanged
    {
        public Action<PopupCommand> OnActionCallback { get; set; }

        public void Open(string name)
        {
            OnActionCallback?.Invoke(new PopupCommand(name, PopupState.Open));
        }

        public void Close(string name)
        {
            OnActionCallback?.Invoke(new PopupCommand(name, PopupState.Close));
        }
    }
}
