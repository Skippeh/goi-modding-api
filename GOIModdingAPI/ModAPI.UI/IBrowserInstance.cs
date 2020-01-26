using System;

namespace ModAPI.UI
{
    internal interface IBrowserInstance : IDisposable
    {
        void Resize(int width, int height);
        void LoadUrl(string url);
        void Update();
        
        /// <summary>
        /// Used internally in the ModAPI.UI assembly. Do not call this! Use Dispose() instead to destroy the browser.
        /// </summary>
        void DisposeInternal();
    }
}