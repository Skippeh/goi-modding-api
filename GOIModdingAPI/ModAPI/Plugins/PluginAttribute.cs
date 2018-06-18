using System;

namespace ModAPI.Plugins
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class PluginAttribute : Attribute
    {
        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string Author { get; set; }
    }
}