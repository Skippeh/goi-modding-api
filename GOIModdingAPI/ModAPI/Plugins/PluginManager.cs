using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using FluffyUnderware.DevTools.Extensions;
using Harmony;
using I2.Loc;
using ModAPI.API;
using ModAPI.Logging;
using ModAPI.Plugins.Events;
using ModAPI.SemanticVersioning;
using ModAPI.Types;

namespace ModAPI.Plugins
{
    public sealed class PluginManager
    {
        private static readonly SemVersion APIVersion = new SemVersion(Assembly.GetExecutingAssembly().GetName().Version);
        
        public List<PluginInstance> Plugins => plugins.Values.ToList();
        
        internal PluginOptions Options { get; private set; }

        private readonly FileSystemWatcher fileWatcher;
        private readonly Dictionary<string, PluginInstance> plugins = new Dictionary<string, PluginInstance>();
        private readonly List<Plugin> tickingPlugins = new List<Plugin>();

        private readonly Queue<string> loadQueue = new Queue<string>();
        private readonly Queue<string> unloadQueue = new Queue<string>();

        internal PluginManager(PluginOptions options)
        {
            Options = options;

            if (!Directory.Exists(Options.PluginsDirectory))
                Directory.CreateDirectory(Options.PluginsDirectory);

            fileWatcher = new FileSystemWatcher(Options.PluginsDirectory)
            {
                Filter = "*.dll",
                IncludeSubdirectories = true
            };

            fileWatcher.Created += OnFileCreated;
            fileWatcher.Deleted += OnFileDeleted;
            fileWatcher.Changed += OnFileChanged;
        }

        internal void StartListening()
        {
            fileWatcher.EnableRaisingEvents = true;
            APIHost.Logger.LogDebug($"Started listening for plugin changes in \"{Path.GetFullPath(Options.PluginsDirectory)}\".");
        }
        
        internal void StopListening()
        {
            fileWatcher.EnableRaisingEvents = false;
            APIHost.Logger.LogDebug($"Stopped listening for plugin changes in \"{Path.GetFullPath(Options.PluginsDirectory)}\".");
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            lock (this)
            {
                loadQueue.Enqueue(Path.GetFileName(e.Name));
            }
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed)
                return;

            var fileName = Path.GetFileName(e.Name);

            lock (this)
            {
                unloadQueue.Enqueue(fileName);
                loadQueue.Enqueue(fileName);
            }
        }

        private void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            lock (this)
            {
                unloadQueue.Enqueue(Path.GetFileName(e.Name));
            }
        }

        internal void LoadPlugins()
        {
            foreach (string directoryPath in Directory.GetDirectories(Options.PluginsDirectory))
            {
                LoadPlugin(directoryPath);
            }
        }

        private bool LoadPlugin(string directoryPath, string requiredVersion = null)
        {
            string filePath = Path.Combine(directoryPath, Path.GetFileName(directoryPath) + ".dll");

            APIHost.Logger.LogInfo($"Loading plugin: {filePath}");

            if (!File.Exists(filePath))
            {
                APIHost.Logger.LogError($"Could not find plugin assembly file: {filePath}");
                return false;
            }

            var pluginInstance = LoadAssembly(Path.GetFileName(filePath), requiredVersion);

            if (pluginInstance == null)
            {
                AssemblyDependencyManager.RemoveSearchDirectory(directoryPath);
            }
            
            return pluginInstance != null;
        }

        private PluginInstance LoadAssembly(string name, string requiredVersion = null)
        {
            string pluginName = Path.GetFileNameWithoutExtension(name);
            string filePath = Path.Combine(Path.Combine(Options.PluginsDirectory, pluginName), pluginName) + ".dll";
            string key = name.ToLowerInvariant();

            if (plugins.ContainsKey(key))
                return plugins[key];

            if (key.Any(ch => ch.ToString().Trim() == string.Empty))
            {
                APIHost.Logger.LogError($"Failed to load plugin {pluginName}. The file name contains whitespace.");
                return null;
            }

            var plugin = new PluginInstance
            {
                DirectoryPath = Path.GetDirectoryName(filePath)
            };

            try
            {
                var bytes = File.ReadAllBytes(filePath);

                string symbolFilePath = Path.ChangeExtension(filePath, ".pdb");
                byte[] symbolBytes = null;

                if (File.Exists(symbolFilePath))
                {
                    APIHost.Logger.LogDebug("Found pdb, loading symbols");
                    symbolBytes = File.ReadAllBytes(symbolFilePath);
                }

                plugin.Assembly = Assembly.Load(bytes, symbolBytes);
                AssemblyDependencyManager.AddSearchDirectory(plugin.DirectoryPath);

                var requiredModApiVersionAttribute = (ModAPIVersionAttribute) plugin.Assembly.GetCustomAttributes(typeof(ModAPIVersionAttribute), false).FirstOrDefault();

                if (requiredModApiVersionAttribute == null)
                {
                    APIHost.Logger.LogError($"Failed to load plugin {pluginName}. Could not find assembly attribute ModAPIVersion.");
                    return null;
                }
                
                if (!APIVersion.DependencyMatches(requiredModApiVersionAttribute.Version))
                {
                    APIHost.Logger.LogError($"Failed to load plugin {pluginName}. It depends on an incompatible version of the mod api ({requiredModApiVersionAttribute.Version}).");
                    return null;
                }
                
                Type pluginType;

                try
                {
                    pluginType = plugin.Assembly.GetExportedTypes().SingleOrDefault(TypeIsValidPlugin);
                }
                catch (InvalidOperationException)
                {
                    APIHost.Logger.LogError($"Failed to load plugin {pluginName}. It contains more than a single plugin.");
                    return null;
                }

                if (pluginType == null)
                {
                    APIHost.Logger.LogError(
                        $"Failed to load plugin {pluginName}. Could not find a valid plugin type. Make sure your plugin class is public and that you've added the PluginAttribute.");
                    return null;
                }

                if (pluginType.Name != pluginName)
                {
                    APIHost.Logger.LogError($"Failed to load plugin {pluginName}. The plugin's class name needs to be {pluginName}.");
                    return null;
                }

                var pluginAttribute = pluginType.GetCustomAttribute<PluginAttribute>();

                if (!PluginTypeHasValidAttribute(pluginAttribute))
                {
                    APIHost.Logger.LogError($"Failed to load plugin {pluginName}. The reason is above this message.");
                    return null;
                }

                if (!PluginHasValidConstructor(pluginType))
                {
                    APIHost.Logger.LogError($"Failed to load plugin {pluginName}. There is no constructor with 0 parameters.");
                }

                if (pluginAttribute.Dependencies?.Length > 0)
                {
                    int numDeps = pluginAttribute.Dependencies.Length;
                    APIHost.Logger.LogDebug($"Loading {numDeps} dependant plugin{(numDeps == 1 ? "" : "s")}...");

                    foreach (string dependency in pluginAttribute.Dependencies)
                    {
                        if (!dependency.Contains("@"))
                        {
                            APIHost.Logger.LogError($"A dependency has an invalid format, could not find '@' after plugin name. ({dependency})");
                            continue;
                        }
                        
                        string[] dependencyAndVersion = dependency.Split('@');
                        string dependencyName = dependencyAndVersion[0];
                        string dependencyVersion = dependencyAndVersion[1];
                        
                        if (!LoadPlugin(dependencyName, dependencyVersion))
                        {
                            APIHost.Logger.LogError($"Failed to load plugin {pluginName}. A dependant plugin failed to load.");
                            return null;
                        }
                    }
                }

                plugin.Author = pluginAttribute.Author;
                plugin.Name = pluginAttribute.Name;
                plugin.ShortDescription = pluginAttribute.ShortDescription ?? "No description available.";
                plugin.Version = new SemVersion(plugin.Assembly.GetName().Version);

                if (requiredVersion != null && !plugin.Version.DependencyMatches(requiredVersion))
                {
                    APIHost.Logger.LogError($"Could not load plugin '{pluginName}@{requiredVersion}'. The installed plugin version is {plugin.Version}.");
                    return null;
                }

                AssemblyDependencyManager.LoadDependencies(plugin.Assembly, plugin.DirectoryPath);
                plugin.Plugin = (Plugin) Activator.CreateInstance(pluginType);

                plugins.Add(key, plugin);
                plugin.Plugin.OnInitialize();

                APIHost.Logger.LogInfo($"Loaded plugin {plugin.Name} v{plugin.Version} by {plugin.Author} ({plugin.ShortDescription}).");
                return plugin;
            }
            catch (FileNotFoundException ex)
            {
                APIHost.Logger.LogException(ex, $"Failed to load plugin {pluginName}. The file could not be found.");
                return null;
            }
            catch (Exception ex)
            {
                APIHost.Logger.LogException(ex, $"Failed to load plugin {pluginName}.");
                return null;
            }
        }

        private bool TypeIsValidPlugin(Type type)
        {
            if (!typeof(Plugin).IsAssignableFrom(type))
                return false;

            if (type.GetCustomAttribute<PluginAttribute>() == null)
                return false;

            return true;
        }

        private bool PluginTypeHasValidAttribute(PluginAttribute attribute)
        {
            if (string.IsNullOrEmpty(attribute.Name?.Trim()))
            {
                APIHost.Logger.LogError("The plugin name is empty.");
                return false;
            }

            if (string.IsNullOrEmpty(attribute.Author?.Trim()))
            {
                APIHost.Logger.LogError("The plugin author is empty.");
                return false;
            }

            if (attribute.ShortDescription != null && attribute.ShortDescription == string.Empty)
            {
                APIHost.Logger.LogError("The plugin short description is empty (use null instead).");
                return false;
            }

            return true;
        }

        private bool PluginHasValidConstructor(Type type)
        {
            return type.GetConstructor(new Type[0]) != null;
        }

        private void UnloadPlugin(string name, PluginDestroyReason reason = PluginDestroyReason.Unloaded)
        {
            name = name.ToLowerInvariant();

            if (!plugins.ContainsKey(name))
                return;

            var plugin = plugins[name];

            try
            {
                plugin.Plugin.OnPluginDestroying(reason);
            }
            catch (Exception ex)
            {
                APIHost.Logger.LogException(ex, $"An exception was raised while destroying plugin {plugin.Name}.");
            }

            DisableTicking(plugin.Plugin);
            plugins.Remove(name);
            AssemblyDependencyManager.RemoveSearchDirectory(plugin.DirectoryPath);

            APIHost.Logger.LogInfo($"Unloaded plugin: {plugin.Name} v{plugin.Version}");
        }

        internal void OnNewScene(SceneType? oldSceneType, SceneType newSceneType)
        {
            APIHost.Events.OnNewScene(oldSceneType, newSceneType);
        }

        internal void Tick()
        {
            ProcessPluginQueues();
            
            foreach (var plugin in tickingPlugins)
            {
                plugin.OnTick();
            }
        }
        internal void EnableTicking(Plugin plugin)
        {
            if (tickingPlugins.Contains(plugin))
                return;
            
            tickingPlugins.Add(plugin);
        }

        internal void DisableTicking(Plugin plugin)
        {
            tickingPlugins.Remove(plugin);
        }

        internal void UnloadAllPlugins()
        {
            foreach (var pluginName in plugins.Keys.ToList())
            {
                UnloadPlugin(pluginName);
            }
        }

        private void ProcessPluginQueues()
        {
            while (unloadQueue.Count > 0)
            {
                UnloadPlugin(unloadQueue.Dequeue());
            }

            while (loadQueue.Count > 0)
            {
                LoadAssembly(loadQueue.Dequeue());
            }
        }
    }
}