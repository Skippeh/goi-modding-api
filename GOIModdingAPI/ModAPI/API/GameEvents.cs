using ModAPI.API.Events;

namespace ModAPI.API
{
    public sealed class GameEvents
    {
        public event LoadingSaveEventHandler LoadingSave;

        internal void OnLoadingSave(SaveState saveState)
        {
            LoadingSave?.Invoke(new LoadingSaveEventArgs(saveState));
        }
    }
}