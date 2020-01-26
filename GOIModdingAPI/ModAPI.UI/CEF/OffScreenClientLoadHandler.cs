using Xilium.CefGlue;

namespace ModAPI.UI.CEF
{
    internal class OffScreenClientLoadHandler : CefLoadHandler
    {
        private readonly OffScreenClient client;

        public OffScreenClientLoadHandler(OffScreenClient client)
        {
            this.client = client;
        }

        protected override void OnLoadStart(CefBrowser browser, CefFrame frame, CefTransitionType transitionType)
        {
            client.BrowserHost = browser.GetHost();
        }
    }
}