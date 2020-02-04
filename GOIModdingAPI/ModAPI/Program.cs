using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony;
using ModAPI.API;
using ModAPI.UI;
using ModAPI.Windows;

namespace ModAPI
{
    internal class Program
    {
        private static StreamWriter consoleWriter;

        private static bool initialized;
        
        public static void Main()
        {
            if (initialized)
                return;

            initialized = true;

            if (Environment.GetCommandLineArgs().Any(line => line.ToLowerInvariant() == "--console"))
            {
                InitializeConsole();
            }
            
            APIHost.Initialize();
            
            try
            {
                var harmony = HarmonyInstance.Create("com.goimodapi");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception ex)
            {
                APIHost.Logger.LogException(ex, "Failed to apply all runtime patches!");
            }
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