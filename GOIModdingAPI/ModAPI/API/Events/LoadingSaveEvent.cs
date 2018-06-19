using System;

namespace ModAPI.API.Events
{
    public delegate void LoadingSaveEventHandler(LoadingSaveEventArgs args);

    public class LoadingSaveEventArgs : EventArgs
    {
        public SaveState State { get; }

        internal LoadingSaveEventArgs(SaveState saveState)
        {
            State = saveState;
        }
    }
}