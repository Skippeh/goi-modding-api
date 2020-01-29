using System;
using System.Collections.Generic;
using System.Linq;
using ModAPI.UI.CEF;
using ModAPI.UI.Cursor;
using ModAPI.UI.Win32Input;
using UnityEngine;
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
            WindowInputHook.HookRawInput();
        }

        public static void Update()
        {
            foreach (var browser in browsers)
            {
                browser.Update();
            }
            
            CefRuntime.DoMessageLoopWork();
            
            // It's necessary to call this every frame because unity sets the cursor to the one specified in UnityEngine.Cursor every time a mouse event is sent if the mouse is visible.
            // The UnityEngine.Cursor is not used by the game so it's ok to override it.
            if (UnityEngine.Cursor.visible)
            {
                CursorManager.Update();
            }
        }

        public static void Destroy()
        {
            WindowInputHook.UnHook();
            
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

        public static FullscreenBrowserInstance CreateFullscreenBrowser(string url, Color defaultBackgroundColor)
        {
            var instance = new FullscreenBrowserInstance(defaultBackgroundColor);
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