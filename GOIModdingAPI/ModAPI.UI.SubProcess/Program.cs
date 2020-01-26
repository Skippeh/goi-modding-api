using System;
using ModAPI.UI.CEF;
using Xilium.CefGlue;

namespace ModAPI.UI.SubProcess
{
    internal class Program
    {
        public static int Main(string[] args)
        {
            CefRuntime.Load(@"GettingOverIt_Data\Managed");
            var cefArgs = new CefMainArgs(args);
            var cefApp = new OffScreenClientApp();

            return CefRuntime.ExecuteProcess(cefArgs, cefApp, IntPtr.Zero);
        }
    }
}