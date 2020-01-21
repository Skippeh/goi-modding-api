using System;
using System.Reflection;
using ModAPI.SemanticVersioning;

namespace ModAPI.Plugins
{
    public class PluginInstance
    {
        internal Assembly Assembly { get; set; }
        internal string DirectoryPath { get; set; }
        public Plugin Plugin { get; internal set; }
        public string Name { get; internal set; }
        public string ShortDescription { get; internal set; }
        public string Author { get; internal set; }
        public SemVersion Version { get; internal set; }
    }
}