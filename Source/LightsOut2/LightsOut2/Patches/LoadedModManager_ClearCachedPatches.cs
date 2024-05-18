using HarmonyLib;
using LightsOut2.Core.ModCompatibility;
using Verse;

namespace LightsOut2.Core.Patches
{
    [HarmonyPatch(typeof(LoadedModManager), nameof(LoadedModManager.ClearCachedPatches))]
    public class LoadedModManager_ClearCachedPatches
    {
        /// <summary>
        /// Searches all loaded mods for compatibility patches, triggered
        /// after all mods have finished loading so load order doesn't matter
        /// </summary>
        public static void Postfix()
        {
            Harmony instance = LightsOut2Mod.HarmonyInstance;
            ModCompatibilityManager.LoadCompatibilityPatches(instance);
        }
    }
}