using System;
using System.IO;
using ModAPI.UI.CEF;
using ModAPI.UI.Events;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Xilium.CefGlue;
using Object = System.Object;

namespace ModAPI.UI
{
    public class FullscreenBrowserInstance : IBrowserInstance
    {
        /// <summary>
        /// Gets or sets whether the ui should be visible and handle input.
        /// </summary>
        public bool Enabled
        {
            get => uiObject && uiObject.activeSelf;
            set
            {
                if (uiObject)
                    uiObject.SetActive(value);
            }
        }

        public UIEventHandler EventHandler { get; }
        
        internal OffScreenClient Client { get; private set; }
        internal CefBrowser Browser { get; private set; }

        private GameObject uiObject;
        private readonly Texture2D texture;

        private Vector2 windowSize;
        private bool disposing;

        public FullscreenBrowserInstance(Color? backgroundColor = null)
        {
            windowSize = new Vector2(Screen.width, Screen.height);
            texture = new Texture2D(Screen.width, Screen.height, TextureFormat.BGRA32, false);

            Client = new OffScreenClient(texture.width, texture.height, this);
            var browserSettings = new CefBrowserSettings();

            if (backgroundColor != null)
            {
                var bgValue = backgroundColor.Value;
                browserSettings.BackgroundColor = new CefColor(
                    (byte) (bgValue.a * 255),
                    (byte) (bgValue.r * 255),
                    (byte) (bgValue.g * 255),
                    (byte) (bgValue.b * 255)
                );
            }

            // This is not a constant framerate, it's more of a max framerate. The UI still only renders when it's needed.
            // This can be overriden by using --off-screen-frame-rate=60.
            // Default value is 30.
            browserSettings.WindowlessFrameRate = 60;
            
            var windowSettings = CefWindowInfo.Create();
            windowSettings.SetAsWindowless(IntPtr.Zero, transparent: true);
            Browser = CefBrowserHost.CreateBrowserSync(windowSettings, Client, browserSettings, "about:blank");
            EventHandler = new UIEventHandler(Browser, this);

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

            disposing = true;
            
            if (uiObject)
            {
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