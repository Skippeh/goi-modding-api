using System;
using System.IO;
using ModAPI.UI.CEF;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Xilium.CefGlue;
using Object = System.Object;

namespace ModAPI.UI
{
    public class FullscreenBrowserInstance : IBrowserInstance
    {
        internal OffScreenClient Client { get; private set; }
        internal CefBrowser Browser { get; private set; }

        private GameObject uiObject;
        private readonly Texture2D texture;

        private Vector2 windowSize;
        private bool disposing;

        public FullscreenBrowserInstance()
        {
            windowSize = new Vector2(Screen.width, Screen.height);
            texture = new Texture2D(Screen.width, Screen.height, TextureFormat.BGRA32, false);

            Client = new OffScreenClient(texture.width, texture.height);
            var browserSettings = new CefBrowserSettings();
            var windowSettings = CefWindowInfo.Create();
            windowSettings.SetAsWindowless(IntPtr.Zero, transparent: true);
            Browser = CefBrowserHost.CreateBrowserSync(windowSettings, Client, browserSettings, "about:blank");
            
            InitializeUnityObject();
        }

        private void InitializeUnityObject()
        {
            uiObject = new GameObject("FullscreenBrowser");
            var browserComponent = uiObject.AddComponent<BrowserInstanceComponent>();
            browserComponent.OffScreenClient = Client;
            browserComponent.TextureTarget = texture;
            browserComponent.BrowserInstance = this;
        }

        public void Dispose()
        {
            UIHost.DestroyBrowser(this);
        }

        public void DisposeInternal()
        {
            // If UIHost.DestroyBrowser is called first then this object will be disposed twice because the dispose method is also called from BrowserInstanceComponent when destroyed.
            // By checking if we're already disposing we can just exit out early.
            if (disposing)
                return;

            if (uiObject)
            {
                disposing = true;
                UnityEngine.Object.Destroy(uiObject);
            }

            Client.Dispose();
            Browser.Dispose();
            Browser = null;
        }

        public void Resize(int width, int height)
        {
            lock (Client.PixelLock)
            {
                texture.Resize(width, height);
                Client.Resize(width, height);
            }
        }

        public void LoadUrl(string url)
        {
            Browser.GetMainFrame().LoadUrl(url);
        }

        public void Update()
        {
            if (Screen.width != windowSize.x || Screen.height != windowSize.y)
            {
                windowSize = new Vector2(Screen.width, Screen.height);
                Resize(Screen.width, Screen.height);
            }
        }
    }
}