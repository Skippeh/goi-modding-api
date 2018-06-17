using System;
using System.IO;

namespace Patcher
{
    internal class Program
    {
        public static string PatcherLocation { get; private set; }
        
        private static int Main(string[] args)
        {
            PatcherLocation = Path.GetDirectoryName(typeof(Program).Assembly.Location);
            
            if (args.Length < 1)
            {
                Console.Error.WriteLine("First argument missing (game directory)");
                return 1;
            }

            string gameDirectory = Path.GetFullPath(args[0]);

            if (Path.GetExtension(gameDirectory) != String.Empty)
                gameDirectory = Path.GetDirectoryName(gameDirectory);
            
            if (!Directory.Exists(gameDirectory))
            {
                Console.Error.WriteLine("Game directory does not exist.");
                return 1;
            }

            string gameAssemblyPath = Path.Combine(gameDirectory, "GettingOverIt_Data", "Managed", "Assembly-CSharp.dll");
            
            if (!File.Exists(gameAssemblyPath))
            {
                Console.Error.WriteLine($"Could not find assembly at \"{gameAssemblyPath}\".");
                return 1;
            }

            string modAssemblyPath = Path.Combine(PatcherLocation, "ModAPI.dll");

            if (!File.Exists(modAssemblyPath))
            {
                Console.Error.WriteLine($"Could not find mod api assembly at \"{modAssemblyPath}\".");
                return 1;
            }

            var patcher = new Patching.Patcher(gameAssemblyPath, modAssemblyPath);

            if (patcher.PatchAssembly())
            {
                Console.WriteLine("Assembly patched successfully.");
                return 0;
            }
            else
            {
                Console.Error.WriteLine("Failed to patch assembly.");
                return 2;
            }
        }
    }
}