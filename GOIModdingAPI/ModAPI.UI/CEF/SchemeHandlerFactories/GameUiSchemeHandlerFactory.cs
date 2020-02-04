using System;
using Xilium.CefGlue;

namespace ModAPI.UI.CEF.SchemeHandlerFactories
{
    internal class GameUiSchemeHandlerFactory : CefSchemeHandlerFactory
    {
        protected override CefResourceHandler Create(CefBrowser browser, CefFrame frame, string schemeName, CefRequest request)
        {
            return new BaseResourceHandler("gameui/");
        }
    }
}