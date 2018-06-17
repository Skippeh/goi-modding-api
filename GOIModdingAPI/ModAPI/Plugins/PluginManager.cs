using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using I2.Loc;
using ModAPI.API;
using ModAPI.Logging;

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
                string key = name.ToLowerInvariant();

                if (plugins.ContainsKey(key))
                    return;

                APIHost.Logger.LogDebug($"Loading plugin: {Path.GetFileNameWithoutExtension(name)}");

                var plugin = new PluginInstance();

                try
                {
                    plugin.Assembly = Assembly.LoadFrom(Path.Combine(Options.PluginsDirectory, name));

                    foreach (Type type in plugin.Assembly.GetExportedTypes())
                    {
                        // todo: check for valid plugin type
                    }

                    plugins.Add(key, plugin);
                }
                catch (Exception ex)
                {
                    APIHost.Logger.LogException(ex, "Failed to load plugin.");
                }
            }
        }

        private void UnloadAssembly(string name)
        {
            lock (this)
            {
                name = name.ToLowerInvariant();

                if (!plugins.ContainsKey(name))
                    return;

                APIHost.Logger.LogDebug($"Unloading plugin: {Path.GetFileNameWithoutExtension(name)}");
            }
        }
    }
}