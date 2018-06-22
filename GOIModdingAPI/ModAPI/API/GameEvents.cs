using System;
using ModAPI.API.Events;
using ModAPI.Types;

namespace ModAPI.API
{
    public sealed class GameEvents
    {
        public event LoadingSaveEventHandler LoadingSave;
        public event SavingEventHandler Saving;
        public event SceneChangedEventHandler SceneChanged;

        internal void OnLoadingSave(SaveState saveState)
        {
            LoadingSave?.Invoke(new LoadingSaveEventArgs(saveState));
        }

        internal void OnSaving(SaveState saveState)
        {
            Saving?.Invoke(new SavingEventArgs(saveState));
        }

        internal void OnNewScene(SceneType? oldSceneType, SceneType sceneType)
        {
            SceneChanged?.Invoke(new SceneChangedEventArgs(oldSceneType, sceneType));
        }
    }
}