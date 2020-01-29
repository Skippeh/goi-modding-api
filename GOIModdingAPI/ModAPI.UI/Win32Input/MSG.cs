using System;
using System.Runtime.InteropServices;

namespace ModAPI.UI.Win32Input
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct MSG
    {
        // ReSharper disable InconsistentNaming
        public IntPtr hwnd;
        public WM message;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public POINT pt;
        public uint lPrivate;
        // ReSharper restore InconsistentNaming
    }
}