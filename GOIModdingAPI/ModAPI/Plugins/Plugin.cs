using ModAPI.Plugins.Events;
using ModAPI.Types;

namespace ModAPI.Plugins
{
    public abstract class Plugin
    {
        public abstract bool ShouldTick { get; }

        public event SceneChangedEventHandler SceneChanged;
        
        protected virtual void Initialize()
        {
        }

        internal void OnNewScene(SceneType oldSceneType, SceneType sceneType)
        {
            SceneChanged?.Invoke(new SceneChangedEventArgs(oldSceneType, sceneType));
        }

        internal void OnInitialize()
        {
            Initialize();
        }

        public virtual void Tick()
        {
        }
    }
}