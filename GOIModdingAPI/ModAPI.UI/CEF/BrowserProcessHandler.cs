using ModAPI.UI.CEF.SchemeHandlerFactories;
using Xilium.CefGlue;

namespace ModAPI.UI.CEF
{
    internal class BrowserProcessHandler : CefBrowserProcessHandler
    {
        private readonly GameUiSchemeHandlerFactory gameUiSchemeHandlerFactory = new GameUiSchemeHandlerFactory();
        private readonly PluginSchemeHandlerFactory pluginSchemeHandlerFactory = new PluginSchemeHandlerFactory();
        
        protected override void OnContextInitialized()
        {
            // bug: For some reason the process freezes on exit when a custom resource handler is used (doesn't seem to matter which is used). Haven't figured out why yet.
            CefRuntime.RegisterSchemeHandlerFactory("modapi", "gameui", gameUiSchemeHandlerFactory);
            CefRuntime.RegisterSchemeHandlerFactory("plugin", null, pluginSchemeHandlerFactory);
        }
    }
}