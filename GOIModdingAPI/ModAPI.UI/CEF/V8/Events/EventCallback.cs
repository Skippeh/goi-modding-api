using Xilium.CefGlue;

namespace ModAPI.UI.CEF.V8.Events
{
    internal class EventCallback
    {
        public CefV8Value Context;
        public CefV8Value Callback;

        public EventCallback(CefV8Value context, CefV8Value callback)
        {
            Context = context;
            Callback = callback;
        }
    }
}