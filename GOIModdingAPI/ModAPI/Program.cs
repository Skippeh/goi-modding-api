using System;
using System.IO;
using System.Reflection;
using Harmony;
using ModAPI.API;
using ModAPI.Windows;

namespace ModAPI
{
    internal class Program
    {
        private static StreamWriter consoleWriter;
        
        public static void Main()
        {
            InitializeConsole();
            
            var harmony = HarmonyInstance.Create("com.goimodapi");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            APIHost.Initialize();
        }

        private static void InitializeConsole()
        {
            if (Environment.OSVersion.Platform != PlatformID.MacOSX && Environment.OSVersion.Platform != PlatformID.Unix)
            {
                WinNative.AllocConsole();
            }
            
            consoleWriter = new StreamWriter(Console.OpenStandardOutput()) {AutoFlush = true};
            Console.SetOut(consoleWriter);
        }
    }
}