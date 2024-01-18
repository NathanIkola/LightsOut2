using HarmonyLib;
using LightsOut2.Common;
using LightsOut2.ThingComps;
using RimWorld;

namespace LightsOut2.Patches
{
    /// <summary>
    /// The class that handles autodoors closing
    /// </summary>
    [HarmonyPatch(nameof(Building_Door), "DoorTryClose")]
    public class Building_Door_DoorTryClose
    {
        /// <summary>
        /// The patch that actually sets the autodoor back into standby
        /// </summary>
        /// <param name="__instance">The door to affect</param>
        /// <param name="__result">Whether the door closed successfully</param>
        public static void Postfix(Building_Door __instance, bool __result)
        {
            // if the door did not close successfully, ignore this
            if (!__result) return;

            // otherwise put the door into standby
            StandbyComp comp = __instance.GetStandbyComp();
            if (comp is null) return;
            comp.IsInStandby = true;
        }
    }
}