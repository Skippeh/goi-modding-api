using ModAPI.Plugins.Events;
using ModAPI.Types;

namespace ModAPI.Plugins
{
    public abstract class Plugin
    {
        public abstract bool ShouldTick { get; }

        protected event SceneChangedEventHandler SceneChanged;
        
        public virtual void Initialize()
        {
        }

        internal void OnNewScene(SceneType oldSceneType, SceneType sceneType)
        {
            SceneChanged?.Invoke(new SceneChangedEventArgs(oldSceneType, sceneType));
        }

        public virtual void Tick()
        {
        }
    }
}