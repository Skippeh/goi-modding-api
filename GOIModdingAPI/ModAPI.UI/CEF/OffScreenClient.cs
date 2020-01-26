using System;
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
        
        public OffScreenClient(int width, int height)
        {
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
    }
}