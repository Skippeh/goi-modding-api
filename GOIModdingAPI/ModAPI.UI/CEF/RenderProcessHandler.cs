using System;
using ModAPI.UI.CEF.V8;
using ModAPI.UI.CEF.V8.Events;
using ModAPI.UI.Events;
using Xilium.CefGlue;

namespace ModAPI.UI.CEF
{
    public class RenderProcessHandler : CefRenderProcessHandler
    {
        protected override void OnContextCreated(CefBrowser browser, CefFrame frame, CefV8Context context)
        {
            if (!frame.IsMain)
                return; // Only register event handler on main frame

            V8EventHandler.RegisterInContext(context);
        }

        protected override void OnContextReleased(CefBrowser browser, CefFrame frame, CefV8Context context)
        {
            V8EventHandler.DestroyFromContext(context);
        }

        protected override bool OnProcessMessageReceived(CefBrowser browser, CefFrame frame, CefProcessId sourceProcess, CefProcessMessage message)
        {
            switch (message.Name)
            {
                case nameof(ProcessEventType.FireEvent):
                {
                    string eventName = message.Arguments.GetString(0);
                    CefValue argument = message.Arguments.GetValue(1);

                    V8EventHandler.FireEvent(frame.V8Context, eventName, argument);
                    return true;
                }
            }
            
            return false;
        }
    }
}