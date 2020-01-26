using System;
using System.Collections.Generic;
using System.Linq;
using ModAPI.UI.CEF;
using Xilium.CefGlue;

namespace ModAPI.UI
{
    internal static class UIHost
    {
        internal static readonly List<IBrowserInstance> browsers = new List<IBrowserInstance>();
        
        public static void Initialize()
        {
            CefRuntime.Load(@"GettingOverIt_Data\Managed");

            var cefArgs = new CefMainArgs(Environment.GetCommandLineArgs());
            var cefApp = new OffScreenClientApp();
            var settings = new CefSettings
            {
                MultiThreadedMessageLoop = false,
                WindowlessRenderingEnabled = true,
                BrowserSubprocessPath = "ModAPI.UI.SubProcess.exe",
                BackgroundColor = new CefColor(255, 255, 0, 0),
                PersistUserPreferences = true,
                UserDataPath = "CEF/UserData",
                CachePath = "CEF/Cache"
            };
            
            CefRuntime.Initialize(cefArgs, settings, cefApp, IntPtr.Zero);
        }

        public static void Update()
        {
            foreach (var browser in browsers)
            {
                browser.Update();
            }
            
            CefRuntime.DoMessageLoopWork();
        }

        public static void Destroy()
        {
            foreach (var browser in browsers.ToList())
            {
                DestroyBrowser(browser);
            }

            CefRuntime.Shutdown();
        }

        public static FullscreenBrowserInstance CreateFullscreenBrowser(string url)
        {
            var instance = new FullscreenBrowserInstance();
            browsers.Add(instance);
            instance.LoadUrl(url);
            return instance;
        }

        public static void DestroyBrowser(IBrowserInstance browser)
        {
            browser.DisposeInternal();
            browsers.Remove(browser);
        }
    }
}