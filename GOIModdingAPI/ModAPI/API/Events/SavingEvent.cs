using System;

namespace ModAPI.API.Events
{
    public delegate void SavingEventHandler(SavingEventArgs args);

    public class SavingEventArgs : EventArgs
    {
        public SaveState State { get; }

        internal SavingEventArgs(SaveState saveState)
        {
            State = saveState;
        }
    }
}