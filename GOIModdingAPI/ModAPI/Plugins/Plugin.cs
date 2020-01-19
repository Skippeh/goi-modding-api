﻿using System;
using System.Collections.Generic;
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

        private readonly List<Action> unInitializeEventHandlers = new List<Action>();
        private bool initializingPlugin;

        public void InitializeHook(Func<Action> eventHandlers)
        {
            if (!initializingPlugin)
                throw new InvalidOperationException("InitializeHook can only be called from the Initialize method.");
            
            unInitializeEventHandlers.Add(eventHandlers());
        }
        
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
            foreach (var unInitializeAction in unInitializeEventHandlers)
            {
                unInitializeAction();
            }
            
            PluginDestroying?.Invoke(new PluginDestroyingEventArgs(this, reason));
            Destroy();
        }

        internal void OnInitialize()
        {
            initializingPlugin = true;
            Initialize();
            initializingPlugin = false;
        }

        internal void OnTick()
        {
            Tick();
        }
    }
}