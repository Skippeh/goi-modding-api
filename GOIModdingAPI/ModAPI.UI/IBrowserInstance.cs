using System;
using ModAPI.UI.Events;

namespace ModAPI.UI
{
    public interface IBrowserInstance : IDisposable
    {
        void Resize(int width, int height);
        void LoadUrl(string url);
        void Update();
        UIEventHandler EventHandler { get; }

        /// <summary>
        /// Used internally in the ModAPI.UI assembly. Do not call this! Use Dispose() instead to destroy the browser.
        /// </summary>
        void DisposeInternal();
    }
}