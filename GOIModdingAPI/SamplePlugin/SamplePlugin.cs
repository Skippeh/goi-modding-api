using System;
using ModAPI.API;
using ModAPI.API.Events;
using ModAPI.Plugins;

namespace SamplePlugin
{
    [Plugin("Sample Plugin", "Skippy", ShortDescription = "A sample plugin to showcase functionality and usage.")]
    public class SamplePlugin : Plugin
    {
        protected override void Initialize()
        {
            APIHost.Events.SceneChanged += OnSceneChanged;
            APIHost.Logger.LogDebug("SamplePlugin initialized");
        }

        protected override void Destroy()
        {
            APIHost.Events.SceneChanged -= OnSceneChanged;
        }

        private void OnSceneChanged(SceneChangedEventArgs args)
        {
            APIHost.Logger.LogDebug($"OnSceneChanged, new scene: {args.SceneType}");
        }
    }
}