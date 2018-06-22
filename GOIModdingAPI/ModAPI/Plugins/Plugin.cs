using ModAPI.API;
using ModAPI.Plugins.Events;

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

        public event PluginDestroyingEventHandler PluginDestroying;
        
        protected virtual void Initialize()
        {
        }

        protected virtual void Destroy()
        {
        }

        protected virtual void Tick()
        {
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

        internal void OnTick()
        {
            Tick();
        }
    }
}