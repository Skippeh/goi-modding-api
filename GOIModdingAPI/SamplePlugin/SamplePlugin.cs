using ModAPI.API;
using ModAPI.Plugins;

namespace SamplePlugin
{
    [Plugin("Sample Plugin", "Skippy", ShortDescription = "A sample plugin to showcase functionality and usage.")]
    public class SamplePlugin : Plugin
    {
        protected override void Initialize()
        {
            APIHost.Logger.LogDebug("SamplePlugin initializing");
        }
    }
}