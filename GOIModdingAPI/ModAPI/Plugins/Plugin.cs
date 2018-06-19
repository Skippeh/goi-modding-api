using ModAPI.API;
using ModAPI.Plugins.Events;
using ModAPI.Types;

namespace ModAPI.Plugins
{
    public abstract class Plugin
    {
        private bool shouldTick = false;
        
        public bool ShouldTick
        {
            get => shouldTick;
            protected set
            {
                if (value == shouldTick)
                    return;
                
                if (value)
                    APIHost.Plugins.EnableTicking(this);
                else
                    APIHost.Plugins.DisableTicking(this);
                
                shouldTick = value;
            }
        }

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