using HarmonyLib;
using LightsOut2.Core.ExtensionMethods;
using LightsOut2.Core.StandbyComps;
using Verse;

namespace LightsOut2.Patches
{
    /// <summary>
    /// A class used to actuate all standby comps upon spawning
    /// </summary>
    [HarmonyPatch(typeof(ThingWithComps), nameof(ThingWithComps.SpawnSetup))]
    public class ThingWithComps_SpawnSetup
    {
        /// <summary>
        /// A function which detects a StandbyComp and updates its status as necessary
        /// </summary>
        /// <param name="__instance">The ThingWithComps to check</param>
        public static void Postfix(ThingWithComps __instance)
        {
            IStandbyComp standbyComp = __instance.GetStandbyComp();
            standbyComp?.UpdateStandbyFromActuator(null);
        }
    }
}