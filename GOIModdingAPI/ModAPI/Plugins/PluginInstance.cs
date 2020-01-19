using System;
using System.Reflection;

namespace ModAPI.Plugins
{
    public class PluginInstance
    {
        internal Assembly Assembly { get; set; }
        public string DirectoryPath { get; internal set; }
        public Plugin Plugin { get; internal set; }
        public string Name { get; internal set; }
        public string ShortDescription { get; internal set; }
        public string Author { get; internal set; }
        public Version Version { get; internal set; }
    }
}