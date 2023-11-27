using HarmonyLib;
using LightsOut2.Debug;
using LightsOut2.ThingComps;
using RimWorld;

namespace LightsOut2.Patches
{
    [HarmonyPatch(typeof(CompPowerTrader))]
    [HarmonyPatch(nameof(CompPowerTrader.PowerOutput), MethodType.Getter)]
    public class CompPowerTrader_PowerOutput
    {
        public static void Postfix(CompPowerTrader __instance, ref float __result)
        {
            StandbyComp standbyComp = __instance.parent.GetComp<StandbyComp>();
            __result *= standbyComp?.Rate ?? 1f;
        }
    }
}