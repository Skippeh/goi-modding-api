using System;
using System.Collections.Generic;
using System.Linq;
using ModAPI.UI.CEF.Extensions;
using ModAPI.UI.Events;
using Xilium.CefGlue;

namespace ModAPI.UI.CEF.V8.Events
{
    /// <summary>
    /// This class is responsible for handling javascript->c# communication
    /// </summary>
    internal static class V8EventHandler
    {
        private static readonly List<ContextData> ContextData = new List<ContextData>();
        
        public static void RegisterInContext(CefV8Context context)
        {
            context.Enter();
            
            try
            {
                var eventHandlerValue = CefV8Value.CreateObject();

                var functionHandler = new FunctionHandler();
                eventHandlerValue.SetValue("on", CefV8Value.CreateFunction("on", functionHandler), CefV8PropertyAttribute.ReadOnly);
                eventHandlerValue.SetValue("off", CefV8Value.CreateFunction("off", functionHandler), CefV8PropertyAttribute.ReadOnly);
                eventHandlerValue.SetValue("call", CefV8Value.CreateFunction("call", functionHandler), CefV8PropertyAttribute.ReadOnly);

                context.GetGlobal().SetValue("apiEventHandler", eventHandlerValue, CefV8PropertyAttribute.ReadOnly | CefV8PropertyAttribute.DontDelete);
            }
            finally
            {
                context.Exit();
            }
        }

        public static void DestroyFromContext(CefV8Context context)
        {
            ContextData.RemoveAll(cd => cd.Context.IsSame(context));
        }

        public static void FireEvent(CefV8Context context, string eventName, CefValue argument)
        {
            context.Enter();

            try
            {
                var contextData = GetContextData(context, createIfNotFound: false);

                List<EventCallback> callbacks = contextData?.GetCallbacks(eventName).ToList();

                if (callbacks == null || callbacks.Count == 0)
                    return;

                var v8Arguments = new[] {argument.ToV8Value()};

                foreach (EventCallback callback in callbacks)
                {
                    callback.Callback.ExecuteFunction(callback.Context, v8Arguments);
                }
            }
            finally
            {
                context.Exit();
            }
        }

        private static ContextData GetContextData(CefV8Context context, bool createIfNotFound)
        {
            ContextData result = ContextData.FirstOrDefault(cd => cd.Context.IsSame(context));

            if (result != null)
                return result;

            if (!createIfNotFound)
                return null;
            
            result = new ContextData() {Context = context};
            ContextData.Add(result);

            return result;
        }

        private class FunctionHandler : CefV8Handler
        {
            protected override bool Execute(string name, CefV8Value obj, CefV8Value[] arguments, out CefV8Value returnValue, out string exception)
            {
                switch (name)
                {
                    case "on":
                        return ExecuteOn(arguments, out returnValue, out exception);
                    case "off":
                        return ExecuteOff(arguments, out returnValue, out exception);
                    case "call":
                        return ExecuteCall(arguments, out returnValue, out exception);
                }
                
                returnValue = null;
                exception = "Unknown function";
                return false;
            }

            private bool ExecuteOn(CefV8Value[] arguments, out CefV8Value returnValue, out string exception)
            {
                string eventName;
                CefV8Value callbackFunc;
                CefV8Value callbackContext;
                
                try
                {
                    GetOnOffArguments(arguments, out eventName, out callbackFunc, out callbackContext);
                }
                catch (ArgumentException ex)
                {
                    returnValue = null;
                    exception = ex.Message;
                    return true;
                }

                ContextData contextData = GetContextData(CefV8Context.GetCurrentContext(), createIfNotFound: true);
                contextData.AddCallback(eventName, callbackFunc, callbackContext);
                
                returnValue = null;
                exception = null;
                return true;
            }

            private bool ExecuteOff(CefV8Value[] arguments, out CefV8Value returnValue, out string exception)
            {
                string eventName;
                CefV8Value callbackFunc;
                CefV8Value callbackContext;
                
                try
                {
                    GetOnOffArguments(arguments, out eventName, out callbackFunc, out callbackContext);
                }
                catch (ArgumentException ex)
                {
                    returnValue = null;
                    exception = ex.Message;
                    return true;
                }

                ContextData contextData = GetContextData(CefV8Context.GetCurrentContext(), createIfNotFound: false);
                bool success = contextData?.RemoveCallback(eventName, callbackFunc, callbackContext) == true;

                returnValue = CefV8Value.CreateBool(success);
                exception = null;
                return true;
            }

            private bool ExecuteCall(CefV8Value[] arguments, out CefV8Value returnValue, out string exception)
            {
                try
                {
                    if (arguments.Length != 1 && arguments.Length != 2)
                        throw new ArgumentException("Invalid amount of arguments specified.");

                    if (!arguments[0].IsString)
                        throw new ArgumentException("Event name is not a string.");
                }
                catch (ArgumentException ex)
                {
                    returnValue = null;
                    exception = ex.Message;
                    return true;
                }

                string eventName = arguments[0].GetStringValue();

                var currentFrame = CefV8Context.GetCurrentContext().GetFrame();
                CefProcessMessage message = CefProcessMessage.Create(nameof(ProcessEventType.FireEvent));
                CefValue cefArgument;

                if (arguments.Length == 2)
                {
                    try
                    {
                        cefArgument = arguments[1].ToCefValue();
                    }
                    catch (ArgumentException ex)
                    {
                        returnValue = null;
                        exception = $"Invalid argument type: {ex.Message}";
                        return true;
                    }
                }
                else
                {
                    cefArgument = CefValue.Create();
                    cefArgument.SetNull();
                }

                message.Arguments.SetString(0, eventName);
                message.Arguments.SetValue(1, cefArgument);
                
                currentFrame.SendProcessMessage(CefProcessId.Browser, message);

                returnValue = null;
                exception = null;
                return true;
            }

            private static void GetOnOffArguments(CefV8Value[] arguments, out string eventName, out CefV8Value callbackFunc, out CefV8Value callbackContext)
            {
                if (arguments.Length != 2 && arguments.Length != 3)
                    throw new ArgumentException("Invalid amount of arguments specified.");

                if (!arguments[0].IsString)
                    throw new ArgumentException("Event name is not a string.");

                if (!arguments[1].IsFunction)
                    throw new ArgumentException("Event callback is not a function.");

                if (arguments.Length == 3 && !arguments[2].IsObject)
                    throw new ArgumentException("Callback context is not an object.");

                eventName = arguments[0].GetStringValue();
                callbackFunc = arguments[1];
                callbackContext = arguments.Length == 3 ? arguments[2] : null;
            }
        }
    }
}