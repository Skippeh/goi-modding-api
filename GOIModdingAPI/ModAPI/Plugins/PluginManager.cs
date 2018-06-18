using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using FluffyUnderware.DevTools.Extensions;
using I2.Loc;
using ModAPI.API;
using ModAPI.Logging;
using ModAPI.Types;

namespace ModAPI.Plugins
{
    public sealed class PluginManager
    {
        public PluginOptions Options { get; private set; }

        private readonly FileSystemWatcher fileWatcher;
        private readonly Dictionary<string, PluginInstance> plugins = new Dictionary<string, PluginInstance>();
        
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

            LoadPlugins();
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
            LoadAssembly(Path.GetFileName(e.Name));
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed)
                return;

            var fileName = Path.GetFileName(e.Name);

            UnloadAssembly(fileName);
            LoadAssembly(fileName);
        }

        private void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            UnloadAssembly(Path.GetFileName(e.Name));
        }

        private void LoadPlugins()
        {
            foreach (string filePath in Directory.GetFiles(Options.PluginsDirectory))
            {
                LoadAssembly(Path.GetFileName(filePath));
            }
        }

        private void LoadAssembly(string name)
        {
            lock (this)
            {
                string pluginName = Path.GetFileNameWithoutExtension(name);
                string key = name.ToLowerInvariant();

                if (plugins.ContainsKey(key))
                    return;

                if (key.Any(ch => ch.ToString().Trim() == string.Empty))
                {
                    APIHost.Logger.LogError($"Failed to load plugin {pluginName}. The file name contains whitespace.");
                    return;
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
                        return;
                    }

                    if (pluginType == null)
                    {
                        APIHost.Logger.LogError(
                            $"Failed to load plugin {pluginName}. Could not find a valid plugin type. Make sure your plugin class is public and that you've added the PluginAttribute.");
                        return;
                    }
                    else if (pluginType.Name != pluginName)
                    {
                        APIHost.Logger.LogError($"Failed to load plugin {pluginName}. The plugin class' name needs to be {pluginName}.");
                        return;
                    }

                    var pluginAttribute = pluginType.GetCustomAttribute<PluginAttribute>();

                    if (!PluginTypeHasValidAttribute(pluginAttribute))
                    {
                        APIHost.Logger.LogError($"Failed to load plugin {pluginName}. The reason is above this message.");
                        return;
                    }

                    plugin.Author = pluginAttribute.Author;
                    plugin.Name = pluginAttribute.Name;
                    plugin.ShortDescription = pluginAttribute.ShortDescription ?? "No description available.";
                    plugin.Version = plugin.Assembly.GetName().Version;
                    plugin.Plugin = (Plugin) Activator.CreateInstance(pluginType);

                    plugins.Add(key, plugin);
                    plugin.Plugin.Initialize();
                }
                catch (Exception ex)
                {
                    APIHost.Logger.LogException(ex, "Failed to load plugin.");
                }
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

        private void UnloadAssembly(string name)
        {
            lock (this)
            {
                name = name.ToLowerInvariant();

                if (!plugins.ContainsKey(name))
                    return;

                var plugin = plugins[name];

                APIHost.Logger.LogDebug($"Unloading plugin: {plugin.Name}");
            }
        }

        public void OnNewScene(SceneType oldSceneType, SceneType sceneType)
        {
            lock (this)
            {
                foreach (var kv in plugins)
                {
                    kv.Value.Plugin.OnNewScene(oldSceneType, sceneType);
                }
            }
        }
    }
}