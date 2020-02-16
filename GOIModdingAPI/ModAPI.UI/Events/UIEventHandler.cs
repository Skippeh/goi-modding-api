using System;
using System.Collections.Generic;
using System.Linq;
using ModAPI.UI.CEF.Conversion;
using Newtonsoft.Json;
using Xilium.CefGlue;

namespace ModAPI.UI.Events
{
    public delegate void UICallbackDelegate<T>(IBrowserInstance browser, T argument);
    
    public sealed class UIEventHandler
    {
        internal readonly CefBrowser Browser;
        internal readonly IBrowserInstance BrowserInstance;

        private readonly Dictionary<string, List<Delegate>> eventCallbacks = new Dictionary<string, List<Delegate>>();
        
        internal UIEventHandler(CefBrowser browser, IBrowserInstance browserInstance)
        {
            Browser = browser ?? throw new ArgumentNullException(nameof(browser));
            BrowserInstance = browserInstance ?? throw new ArgumentNullException(nameof(browserInstance));
        }

        internal void FireEvent(string eventName, CefValue argument)
        {
            var callbacks = GetCallbacks(eventName, createIfNotFound: false);

            if (callbacks == null)
                return;

            Dictionary<Type, object> argumentTypeCache = new Dictionary<Type, object>();
            
            foreach (Delegate callbackObj in callbacks)
            {
                var managedArgumentType = callbackObj.GetType().GetGenericArguments()[0];

                if (!argumentTypeCache.TryGetValue(managedArgumentType, out var managedArgument))
                {
                    try
                    {
                        managedArgument = CefConvert.ConvertValue(argument, managedArgumentType);
                    }
                    catch (Exception ex) when (ex is InvalidCastException || ex is ArgumentException || ex is MissingMethodException)
                    {
                        Console.WriteLine($"Could not convert CefValue(type={argument.GetValueType()}) to {managedArgumentType.Name}. Make sure your types match.\n\n{ex}");
                        return;
                    }
                    
                    argumentTypeCache[managedArgumentType] = managedArgument;
                }
                
                callbackObj.DynamicInvoke(BrowserInstance, managedArgument);
            }
        }

        public void CallEvent(string eventName, object argument)
        {
            if (string.IsNullOrEmpty(eventName)) throw new ArgumentNullException(nameof(eventName));
            if (argument == null) throw new ArgumentNullException(nameof(argument));

            CefFrame frame = Browser.GetMainFrame();
            CefProcessMessage processMessage = CefProcessMessage.Create(nameof(ProcessEventType.FireEvent));
            CefValue argumentValue = CefConvert.ConvertObject(argument);

            processMessage.Arguments.SetString(0, eventName);
            processMessage.Arguments.SetValue(1, argumentValue);
            frame.SendProcessMessage(CefProcessId.Renderer, processMessage);
        }

        public void RegisterCallback<T>(string eventName, UICallbackDelegate<T> callback)
        {
            if (eventName == null) throw new ArgumentNullException(nameof(eventName));
            if (callback == null) throw new ArgumentNullException(nameof(callback));
            
            GetCallbacks(eventName, createIfNotFound: true).Add(callback);
        }

        public bool RemoveCallback<T>(string eventName, UICallbackDelegate<T> callback)
        {
            if (eventName == null) throw new ArgumentNullException(nameof(eventName));
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            return GetCallbacks(eventName, createIfNotFound: false)?.RemoveAll(cb => ReferenceEquals(cb, callback)) > 0;
        }

        private List<Delegate> GetCallbacks(string eventName, bool createIfNotFound)
        {
            if (eventCallbacks.TryGetValue(eventName, out var callbacks))
                return callbacks;

            if (!createIfNotFound)
                return null;

            callbacks = new List<Delegate>();
            eventCallbacks[eventName] = callbacks;
            return callbacks;
        }
    }
}