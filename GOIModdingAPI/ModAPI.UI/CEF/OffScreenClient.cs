using System;
using ModAPI.UI.CEF.Conversion;
using ModAPI.UI.Events;
using UnityEngine;
using Xilium.CefGlue;

namespace ModAPI.UI.CEF
{
    internal class OffScreenClient : CefClient, IDisposable
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        
        internal object PixelLock { get; private set; }

        /// <summary>Array of raw 32-bit pixel data (BGRA * width * height)</summary>
        internal byte[] PixelBuffer { get; private set; }

        private readonly OffScreenClientRenderHandler renderHandler;
        private readonly OffScreenClientLoadHandler loadHandler;

        internal CefBrowserHost BrowserHost;

        private readonly IBrowserInstance owner;
        
        public OffScreenClient(int width, int height, IBrowserInstance owner)
        {
            this.owner = owner;
            PixelLock = new object();
            Resize(width, height);

            renderHandler = new OffScreenClientRenderHandler(this);
            loadHandler = new OffScreenClientLoadHandler(this);
        }

        public void LoadToTexture(Texture2D texture)
        {
            if (BrowserHost == null)
                return;

            lock (PixelLock)
            {
                texture.LoadRawTextureData(PixelBuffer);
                texture.Apply();
            }
        }

        public void Resize(int width, int height)
        {
            Width = width;
            Height = height;
            PixelBuffer = new byte[width * height * 4];
        }

        protected override CefRenderHandler GetRenderHandler()
        {
            return renderHandler;
        }

        protected override CefLoadHandler GetLoadHandler()
        {
            return loadHandler;
        }

        public void Dispose()
        {
            if (BrowserHost != null)
            {
                BrowserHost.CloseBrowser(true);
                BrowserHost.Dispose();
                BrowserHost = null;
            }
        }

        protected override bool OnProcessMessageReceived(CefBrowser browser, CefFrame frame, CefProcessId sourceProcess, CefProcessMessage message)
        {
            switch (message.Name)
            {
                case nameof(ProcessEventType.FireEvent):
                {
                    string eventName = message.Arguments.GetString(0);
                    CefValue argument = message.Arguments.GetValue(1);

                    owner.EventHandler.FireEvent(eventName, argument);
                    
                    return true;
                }
            }
            
            return false;
        }
    }
}