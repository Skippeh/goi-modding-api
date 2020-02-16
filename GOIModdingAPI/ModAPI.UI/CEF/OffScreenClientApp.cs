using Xilium.CefGlue;

namespace ModAPI.UI.CEF
{
    public class OffScreenClientApp : CefApp
    {
        protected override void OnRegisterCustomSchemes(CefSchemeRegistrar registrar)
        {
            registrar.AddCustomScheme("modapi",
                CefSchemeOptions.FetchEnabled |
                CefSchemeOptions.Secure |
                CefSchemeOptions.Standard |
                CefSchemeOptions.CorsEnabled |
                CefSchemeOptions.DisplayIsolated // Don't allow other schemes to access any content from this scheme
            );

            registrar.AddCustomScheme("plugin",
                CefSchemeOptions.FetchEnabled |
                CefSchemeOptions.Secure |
                CefSchemeOptions.Standard |
                CefSchemeOptions.CorsEnabled
            );
        }

        protected override CefBrowserProcessHandler GetBrowserProcessHandler()
        {
            return new BrowserProcessHandler();
        }

        protected override CefRenderProcessHandler GetRenderProcessHandler()
        {
            return new RenderProcessHandler();
        }
    }
}