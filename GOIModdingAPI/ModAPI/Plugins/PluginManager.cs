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
using ModAPI.Types;

namespace ModAPI.Plugins
{
    public sealed class PluginManager
    {
        public List<PluginInstance> Plugins => plugins.Values.ToList();
        
        internal PluginOptions Options { get; private set; }

        private readonly FileSystemWatcher fileWatcher;
        private readonly Dictionary<string, PluginInstance> plugins = new Dictionary<string, PluginInstance>();
        private readonly List<Plugin> tickingPlugins = new List<Plugin>();

        private Queue<string> loadQueue = new Queue<string>();
        private Queue<string> unloadQueue = new Queue<string>();
        
        public PluginManager() : this(new PluginOptions())
        {
        }

        public PluginManager(PluginOptions options)
        {
            Options = options;

            if (!Directory.Exists(Options.PluginsDirectory))
                Directory.CreateDirectory(Options.PluginsDirectory);

            fileWatcher = new FileSystemWatcher(Options.PluginsDirectory)
            {
                Filter = "*.dll"
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
            foreach (string filePath in Directory.GetFiles(Options.PluginsDirectory))
            {
                LoadAssembly(Path.GetFileName(filePath));
            }
        }

        private PluginInstance LoadAssembly(string name)
        {
            string pluginName = Path.GetFileNameWithoutExtension(name);
            string key = name.ToLowerInvariant();

            if (plugins.ContainsKey(key))
                return plugins[key];

            if (key.Any(ch => ch.ToString().Trim() == string.Empty))
            {
                APIHost.Logger.LogError($"Failed to load plugin {pluginName}. The file name contains whitespace.");
                return null;
            }

            var plugin = new PluginInstance();

            try
            {
                plugin.Assembly = Assembly.LoadFrom(Path.Combine(Options.PluginsDirectory, name));

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
                else if (pluginType.Name != pluginName)
                {
                    APIHost.Logger.LogError($"Failed to load plugin {pluginName}. The plugin class' name needs to be {pluginName}.");
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
                    APIHost.Logger.LogDebug($"Loading {pluginAttribute.Dependencies.Length} dependant plugin{(pluginAttribute.Dependencies.Length == 1 ? "" : "s")}...");

                    foreach (string dependency in pluginAttribute.Dependencies)
                    {
                        if (LoadAssembly(dependency) == null)
                        {
                            APIHost.Logger.LogError($"Failed to load plugin {pluginName}. A dependant plugin failed to load.");
                            return null;
                        }
                    }
                }

                plugin.Author = pluginAttribute.Author;
                plugin.Name = pluginAttribute.Name;
                plugin.ShortDescription = pluginAttribute.ShortDescription ?? "No description available.";
                plugin.Version = plugin.Assembly.GetName().Version;
                plugin.Plugin = (Plugin) Activator.CreateInstance(pluginType);

                plugins.Add(key, plugin);
                plugin.Plugin.OnInitialize();

                APIHost.Logger.LogDebug($"Loaded plugin {plugin.Name} by {plugin.Author} ({plugin.ShortDescription}).");
                return plugin;
            }
            catch (FileNotFoundException)
            {
                APIHost.Logger.LogError($"Failed to load plugin {pluginName}. The file could not be found.");
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

        private void UnloadAssembly(string name, PluginDestroyReason reason = PluginDestroyReason.Unloaded)
        {
            name = name.ToLowerInvariant();

            if (!plugins.ContainsKey(name))
                return;

            var plugin = plugins[name];
            plugin.Plugin.OnPluginDestroying(reason);

            APIHost.Logger.LogDebug($"Unloading plugin: {plugin.Name}");
        }

        public void OnNewScene(SceneType oldSceneType, SceneType sceneType)
        {
            foreach (var kv in plugins)
            {
                kv.Value.Plugin.OnNewScene(oldSceneType, sceneType);
            }
        }

        internal void TickPlugins()
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

        private void ProcessPluginQueues()
        {
            while (unloadQueue.Count > 0)
            {
                UnloadAssembly(unloadQueue.Dequeue());
            }

            while (loadQueue.Count > 0)
            {
                LoadAssembly(loadQueue.Dequeue());
            }
        }
    }
}