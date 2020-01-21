using System;

namespace ModAPI.Plugins
{
    public class ModAPIVersionAttribute : Attribute
    {
        public string Version { get; set; }

        /// <summary>
        /// Specifies which version of the mod api this plugin requires to be able to run. It's required for all plugins to use this attribute.
        /// </summary>
        /// <param name="version">The required mod api version. You can use nodejs styled dependency syntax, such as ^1.0.0 to run on all 1.x.x versions.</param>
        public ModAPIVersionAttribute(string version)
        {
            Version = version ?? throw new ArgumentNullException(nameof(version));
        }
    }
}