using System;
using System.Runtime.InteropServices;

namespace ModAPI.UI.Win32Input
{
    internal static class Win32API
    {
        public delegate int HookProc(int code, IntPtr wParam, ref MSG lParam);

        [DllImport("User32")]
        public static extern IntPtr SetWindowsHookEx(HookType code, HookProc func, IntPtr hInstance, int threadID);

        [DllImport("User32")]
        public static extern int UnhookWindowsHookEx(IntPtr hhook);

        [DllImport("User32")]
        public static extern int CallNextHookEx(IntPtr hhook, int code, IntPtr wParam, ref MSG lParam);

        [DllImport("Kernel32")]
        public static extern uint GetCurrentThreadId();

        [DllImport("Kernel32")]
        public static extern IntPtr GetModuleHandle(string lpModuleName);
        
        [DllImport("USER32.dll")]
        public static extern short GetKeyState(VK nVirtKey);

        /// <summary>Returned value is in milliseconds.</summary>
        [DllImport("user32.dll")]
        public static extern uint GetDoubleClickTime();

        /// <summary>
        /// Gets the message time for the last message retrieved in milliseconds. The time is the elapsed time since system startup.
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern long GetMessageTime();
        
        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(SM smIndex);
        
        [DllImport("user32.dll")]
        public static extern bool ScreenToClient(IntPtr hWnd, ref Int32POINT lpPoint);

        [DllImport("user32.dll")]
        public static extern bool ClientToScreen(IntPtr hWnd, ref Int32POINT lpPoint);
    }
}
