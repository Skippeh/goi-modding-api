using System;

namespace ModAPI.Plugins.Events
{
    public delegate void PluginDestroyingEventHandler(PluginDestroyingEventArgs args);

    public class PluginDestroyingEventArgs : EventArgs
    {
        public Plugin Plugin { get; }
        public PluginDestroyReason Reason { get; }

        public PluginDestroyingEventArgs(Plugin plugin, PluginDestroyReason reason)
        {
            Plugin = plugin;
            Reason = reason;
        }
    }
}