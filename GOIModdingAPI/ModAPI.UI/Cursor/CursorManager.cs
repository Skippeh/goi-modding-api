using System;
using ModAPI.UI.Win32Input;

namespace ModAPI.UI.Cursor
{
    internal static class CursorManager
    {
        public static IntPtr CurrentCursorHandle;

        public static void Update()
        {
            if (CurrentCursorHandle != IntPtr.Zero)
            {
                Win32API.SetCursor(CurrentCursorHandle);
            }
        }
    }
}