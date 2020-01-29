using UnityEngine;
using Xilium.CefGlue;

namespace ModAPI.UI.Win32Input
{
    internal class MouseEventData
    {
        /// <summary>
        /// The button that was clicked. Only relevant on MouseDown/MouseUp events.
        /// </summary>
        public CefMouseButtonType Button;

        public Vector2 ScreenPosition;
        public Vector2 WindowPosition;

        /// <summary>
        /// Whether the mouse left the client area. Only relevant on MouseMove event.
        /// </summary>
        public bool LeftClientArea;

        /// <summary>
        /// How many times the mouse has been clicked in a row (double click etc).
        /// </summary>
        public uint ClickCount;

        /// <summary>
        /// How long the scroll wheel has scrolled. Only relevant on MouseScroll event.
        /// </summary>
        public int ScrollDelta;

        public MSG OriginalMessage;

        private bool intercept;

        public bool Intercept
        {
            get => intercept;
            set => intercept = intercept || value; // Only allow setting value to true.
        }
    }
}