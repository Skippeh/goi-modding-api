using System;
using System.Reflection;
using Harmony;
using ModAPI.API;
using ModAPI.API.Events;
using ModAPI.Plugins;
using UnityEngine;

namespace SamplePlugin
{
    [Plugin("Sample Plugin", "Skippy", ShortDescription = "A sample plugin to showcase functionality and usage.")]
    public class SamplePlugin : Plugin
    {
        protected override void Initialize()
        {
            APIHost.Events.SceneChanged += OnSceneChanged;
            
            // Apply all harmony hooks in this assembly
            var harmonyInstance = HarmonyInstance.Create("com.sampleplugin");
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());

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

    // Patch GravityControl.Start method and set gravity to 10% of original value
    // More info on usage: https://github.com/pardeike/Harmony/wiki
    [HarmonyPatch(typeof(GravityControl))]
    [HarmonyPatch("Start")]
    internal static class GravityPatch
    {
        private static void Postfix()
        {
            Physics2D.gravity *= 0.1f; // 10% gravity
            Physics.gravity = Physics2D.gravity; // Will make particle effects have the same gravity
        }
    }
}