using ModAPI.Plugins.Events;
using ModAPI.Types;

namespace ModAPI.Plugins
{
    public abstract class Plugin
    {
        public abstract bool ShouldTick { get; }

        public event SceneChangedEventHandler SceneChanged;
        public event PluginDestroyingEventHandler PluginDestroying;
        
        protected virtual void Initialize()
        {
        }

        protected virtual void Destroy()
        {
        }

        internal void OnNewScene(SceneType oldSceneType, SceneType sceneType)
        {
            SceneChanged?.Invoke(new SceneChangedEventArgs(oldSceneType, sceneType));
        }

        internal void OnPluginDestroying(PluginDestroyReason reason)
        {
            PluginDestroying?.Invoke(new PluginDestroyingEventArgs(this, reason));
            Destroy();
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