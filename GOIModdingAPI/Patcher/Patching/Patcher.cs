using System;
using System.Collections.Generic;
using System.IO;
using Mono.Cecil;
using Patcher.Patching.Patches;

namespace Patcher.Patching
{
    public class Patcher
    {
        private readonly string gameAssemblyPath;
        private readonly ModuleDefinition gameModuleDefinition;
        private readonly ModuleDefinition apiModuleDefinition;
        private readonly List<IAssemblyPatch> assemblyPatches = new List<IAssemblyPatch>();
        
        public Patcher(string gameAssemblyPath, string apiAssemblyPath)
        {
            var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(Path.GetFullPath(Path.GetDirectoryName(gameAssemblyPath)));
            
            this.gameAssemblyPath = gameAssemblyPath;
            gameModuleDefinition = ModuleDefinition.ReadModule(gameAssemblyPath, new ReaderParameters {InMemory = true, AssemblyResolver = resolver});

            apiModuleDefinition = ModuleDefinition.ReadModule(apiAssemblyPath, new ReaderParameters {AssemblyResolver = resolver});
            
            assemblyPatches.AddRange(new IAssemblyPatch[]
            {
                new InitializePatch(),
            });
        }

        public bool PatchAssembly()
        {
            bool failed = false;
            
            foreach (var patch in assemblyPatches)
            {
                try
                {
                    if (patch.IsPatched(gameModuleDefinition, apiModuleDefinition))
                    {
                        Console.WriteLine($"Patch already installed, skipping {patch.GetType().Name}.");
                        continue;
                    }

                    if (patch.Patch(gameModuleDefinition, apiModuleDefinition))
                        Console.WriteLine($"Patch installed: {patch.GetType().Name}.");
                    else
                    {
                        Console.Error.WriteLine($"Failed to install patch {patch.GetType().Name}!");
                        failed = true;
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"An exception was raised while applying patch {patch.GetType().Name}:");
                    Console.Error.WriteLine(ex);
                    failed = true;
                }
            }

            if (failed)
            {
                return false;
            }
            
            #if DEBUG
            string newFileName = Path.GetFileNameWithoutExtension(gameAssemblyPath) + "_Original.dll";
            string newFilePath = Path.Combine(Path.GetDirectoryName(gameAssemblyPath), newFileName);

            if (!File.Exists(newFilePath))
            {
                File.Move(gameAssemblyPath, newFilePath);
            }
            #endif

            gameModuleDefinition.Write(gameAssemblyPath);

            return true;
        }
    }
}