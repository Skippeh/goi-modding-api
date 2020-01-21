using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ModAPI.API;
using UnityEngine;

namespace ModAPI
{
    internal static class AssemblyDependencyManager
    {
        private static readonly List<string> searchDirectories = new List<string>();

        // List all assemblies that are packed in ModAPI.Dependencies.dll
        private static readonly string[] PackedDependencies =
        {
            "System.Data",
            "System.Drawing",
            "System.Runtime.Serialization",
            "System.Transactions",
            "System.Xml",
            "System.Xml.Linq",
            "Newtonsoft.Json"
        };

        private static readonly Assembly PackedDependenciesAssembly;
        private static readonly Dictionary<string, Assembly> loadedAssemblies = new Dictionary<string, Assembly>();
        
        static AssemblyDependencyManager()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
            PackedDependenciesAssembly = Assembly.LoadFile(@"GettingOverIt_Data\Managed\ModAPI.Dependencies.dll");
        }

        public static void AddSearchDirectory(string directory)
        {
            searchDirectories.Add(directory);
        }

        public static void RemoveSearchDirectory(string directory)
        {
            searchDirectories.Remove(directory);
        }

        public static void LoadDependencies(Assembly pluginAssembly, string pluginDirectoryPath)
        {
            foreach (var dependencyName in pluginAssembly.GetReferencedAssemblies())
            {
                if (loadedAssemblies.ContainsKey(dependencyName.FullName)) // Skip libraries we've already loaded (incase multiple plugins depend on the same 3rd party libs (with matching version))
                    continue;

                string filePath = Path.Combine(pluginDirectoryPath, dependencyName.Name + ".dll");

                if (File.Exists(filePath))
                {
                    APIHost.Logger.LogDebug($"Loading dependency: {filePath}");
                    
                    var assembly = Assembly.LoadFrom(filePath);
                    loadedAssemblies[pluginAssembly.FullName] = assembly;
                }
            }
        }

        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (loadedAssemblies.ContainsKey(args.Name))
                return loadedAssemblies[args.Name];
            
            string dllName = args.Name.Contains(",") ? args.Name.Split(',')[0] : args.Name;

            if (PackedDependencies.Contains(dllName))
            {
                return PackedDependenciesAssembly;
            }
            
            dllName += ".dll";

            foreach (string directory in searchDirectories)
            {
                string dllPath = Path.Combine(directory, dllName);
                
                if (File.Exists(dllPath))
                {
                    Assembly result = Assembly.LoadFile(dllPath);
                    loadedAssemblies[args.Name] = result;
                    return result;
                }
                else
                {
                    APIHost.Logger.LogDebug($"File not found: {dllPath}");
                }
            }

            try
            {
                APIHost.Logger.LogDebug($"Attempting to load: {args.Name}");
                var result = Assembly.Load(args.Name);
                loadedAssemblies[args.Name] = result;
                return result;
            }
            catch (Exception)
            {
                // ignored
            }

            APIHost.Logger.LogError($"Could not find assembly: {args.Name}");
            return null;
        }
    }
}