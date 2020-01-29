using System;
using System.Runtime.InteropServices;
using ModAPI.UI.Cursor;
using ModAPI.UI.Win32Input;
using UnityEngine;
using Xilium.CefGlue;

namespace ModAPI.UI.CEF
{
    internal class OffScreenClientRenderHandler : CefRenderHandler
    {
        private OffScreenClient client;
        
        public OffScreenClientRenderHandler(OffScreenClient client)
        {
            this.client = client;
        }

        protected override CefAccessibilityHandler GetAccessibilityHandler()
        {
            return null;
        }

        protected override void GetViewRect(CefBrowser browser, out CefRectangle rect)
        {
            rect = new CefRectangle(0, 0, client.Width, client.Height);
        }

        protected override bool GetScreenInfo(CefBrowser browser, CefScreenInfo screenInfo)
        {
            return false;
        }

        protected override void OnPopupSize(CefBrowser browser, CefRectangle rect)
        {
            
        }

        protected override void OnPaint(CefBrowser browser, CefPaintElementType type, CefRectangle[] dirtyRects, IntPtr buffer, int width, int height)
        {
            lock (client.PixelLock)
            {
                Marshal.Copy(buffer, client.PixelBuffer, 0, width * height * 4);
            }
        }

        protected override void OnAcceleratedPaint(CefBrowser browser, CefPaintElementType type, CefRectangle[] dirtyRects, IntPtr sharedHandle)
        {
            throw new NotImplementedException();
        }

        protected override void OnCursorChange(CefBrowser browser, IntPtr cursorHandle, CefCursorType type, CefCursorInfo customCursorInfo)
        {
            CursorManager.CurrentCursorHandle = cursorHandle;

            if (UnityEngine.Cursor.visible)
            {
                // This is also called every frame. It's necessary because unity sets the cursor to the one specified in UnityEngine.Cursor every time a mouse event is sent if the mouse is visible.
                CursorManager.Update();
            }
        }

        protected override void OnScrollOffsetChanged(CefBrowser browser, double x, double y)
        {
            
        }

        protected override void OnImeCompositionRangeChanged(CefBrowser browser, CefRange selectedRange, CefRectangle[] characterBounds)
        {
            
        }
    }
}