using HarmonyLib;
using LightsOut2.Common;
using LightsOut2.ThingComps;
using RimWorld;

namespace LightsOut2.Patches
{
    /// <summary>
    /// The class that allows autodoors to enter standby
    /// </summary>
    [HarmonyPatch(typeof(Building_Door), "DoorOpen")]
    public class Building_Door_DoorOpen
    {
        /// <summary>
        /// Postfix patch so that we can detect the moment the door is marked as open
        /// </summary>
        /// <param name="__instance"></param>
        public static void Postfix(Building_Door __instance)
        {
            // manual doors are not affected
            if (!__instance.DoorPowerOn) return;

            StandbyComp comp = __instance.GetStandbyComp();
            if (comp is null) return;

            comp.IsInStandby = !__instance.Open;
        }
    }
}