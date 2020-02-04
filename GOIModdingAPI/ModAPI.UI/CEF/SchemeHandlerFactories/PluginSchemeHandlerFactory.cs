using Xilium.CefGlue;

namespace ModAPI.UI.CEF.SchemeHandlerFactories
{
    internal class PluginSchemeHandlerFactory : CefSchemeHandlerFactory
    {
        protected override CefResourceHandler Create(CefBrowser browser, CefFrame frame, string schemeName, CefRequest request)
        {
            return new PluginResourceHandler("Plugins/");
        }
    }
}