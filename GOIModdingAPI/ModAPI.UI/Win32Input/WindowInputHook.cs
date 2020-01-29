using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ModAPI.UI.Win32Input
{
    internal static class WindowInputHook
    {
        public static void HookRawInput()
        {
            if (!RawInput.Start())
            {
                Console.WriteLine("Failed to hook input");
            }
        }

        public static void UnHook()
        {
            RawInput.Stop();
        }
    }
}