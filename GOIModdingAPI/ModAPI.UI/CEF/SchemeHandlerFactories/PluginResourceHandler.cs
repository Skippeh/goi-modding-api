using System;
using System.IO;

namespace ModAPI.UI.CEF.SchemeHandlerFactories
{
    internal class PluginResourceHandler : BaseResourceHandler
    {
        public PluginResourceHandler(string rootPath) : base(rootPath)
        {
        }

        protected override string GetRootDirectory(Uri uri)
        {
            string pluginName = uri.Segments[0];
            return Path.Combine(Path.Combine(RootPath, pluginName), "html");
        }
    }
}