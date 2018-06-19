using System;
using ModAPI.API.Events;

namespace ModAPI.API
{
    public sealed class GameEvents
    {
        public event LoadingSaveEventHandler LoadingSave;
        public event SavingEventHandler Saving;

        internal void OnLoadingSave(SaveState saveState)
        {
            LoadingSave?.Invoke(new LoadingSaveEventArgs(saveState));
        }

        public void OnSaving(SaveState saveState)
        {
            Saving?.Invoke(new SavingEventArgs(saveState));
        }
    }
}