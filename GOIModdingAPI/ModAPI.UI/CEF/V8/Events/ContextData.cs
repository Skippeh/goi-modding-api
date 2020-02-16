using System;
using System.Collections.Generic;
using System.Linq;
using Xilium.CefGlue;

namespace ModAPI.UI.CEF.V8.Events
{
    internal class ContextData
    {
        public CefV8Context Context;
        public readonly Dictionary<string, List<EventCallback>> RegisteredCallbacks = new Dictionary<string, List<EventCallback>>();
        
        public void AddCallback(string eventName, CefV8Value callback, CefV8Value context = null)
        {
            if (!callback.IsFunction)
                throw new ArgumentException("Callback value is not a function.");

            if (!RegisteredCallbacks.TryGetValue(eventName, out var callbacks))
            {
                callbacks = new List<EventCallback>();
                RegisteredCallbacks[eventName] = callbacks;
            }

            callbacks.Add(new EventCallback(context, callback));
        }

        public bool RemoveCallback(string eventName, CefV8Value callback, CefV8Value context = null)
        {
            if (!callback.IsFunction)
                throw new ArgumentException("Callback is not a function.");

            if (!RegisteredCallbacks.ContainsKey(eventName))
                return false;

            var callbacks = RegisteredCallbacks[eventName];
            var eventCallbacks = callbacks.Where(ec => ec.Callback == callback && ec.Context == context).ToList();

            if (!eventCallbacks.Any())
                return false;

            foreach (var eventCallback in eventCallbacks)
            {
                callbacks.Remove(eventCallback);
            }

            return true;
        }

        public IEnumerable<EventCallback> GetCallbacks(string eventName)
        {
            if (!RegisteredCallbacks.ContainsKey(eventName))
                yield break;
            
            foreach (var callback in RegisteredCallbacks[eventName])
            {
                yield return callback;
            }
        }
    }
}