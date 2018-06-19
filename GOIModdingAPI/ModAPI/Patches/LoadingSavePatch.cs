using System;
using Harmony;
using ModAPI.API;

namespace ModAPI.Patches
{
    [HarmonyPatch(typeof(Saviour))]
    [HarmonyPatch("Load")]
    [HarmonyPatch(new[] {typeof(SaveState)})]
    internal static class LoadingSavePatch
    {
        private static void Prefix(SaveState saveState)
        {
            APIHost.Events.OnLoadingSave(saveState);
        }
    }
}