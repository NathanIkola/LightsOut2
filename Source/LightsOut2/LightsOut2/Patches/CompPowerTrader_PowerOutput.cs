﻿using HarmonyLib;
using LightsOut2.Common;
using LightsOut2.ThingComps;
using RimWorld;

namespace LightsOut2.Patches
{
    /// <summary>
    /// Class that changes the power draw in a CompPowerTrader depending on the standby state of the StandbyComp
    /// </summary>
    [HarmonyPatch(typeof(CompPowerTrader))]
    [HarmonyPatch(nameof(CompPowerTrader.PowerOutput), MethodType.Getter)]
    public class CompPowerTrader_PowerOutput
    {
        /// <summary>
        /// Takes the result of <see cref="CompPowerTrader.PowerOutput"/> and multiplies it by the new rate
        /// </summary>
        /// <param name="__instance">The <see cref="CompPowerTrader"/> that is being affected</param>
        /// <param name="__result">The current power consumption rate</param>
        public static void Postfix(CompPowerTrader __instance, ref float __result)
        {
            StandbyComp standbyComp = __instance.parent.GetStandbyComp();
            __result *= standbyComp?.Rate ?? 1f;
        }
    }
}