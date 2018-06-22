using ModAPI.API;
using ModAPI.Plugins;

namespace SamplePlugin
{
    public class SamplePlugin : Plugin
    {
        protected override void Initialize()
        {
            APIHost.Logger.LogDebug("SamplePlugin initializing");
        }
    }
}