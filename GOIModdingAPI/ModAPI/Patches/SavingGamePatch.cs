using System.Collections.Generic;
using System.Reflection;
using Harmony;
using ModAPI.API;

namespace ModAPI.Patches
{
    [HarmonyPatch(typeof(Saviour))]
    [HarmonyPatch("Save")]
    internal static class SavingGamePatch
    {
        private static SaveState Postfix(SaveState __result)
        {
            APIHost.Events.OnSaving(__result);
            return __result;
        }
    }
}