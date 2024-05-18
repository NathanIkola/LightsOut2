using HarmonyLib;
using LightsOut2.Common;
using LightsOut2.Core.ExtensionMethods;
using LightsOut2.Core.StandbyComps;
using LightsOut2.ThingComps;
using RimWorld;
using UnityEngine;
using Verse;

namespace LightsOut2.Patches
{
    /// <summary>
    /// A class that patches the inspect string to include the On Standby message as well as correct the wattage amount as necessary
    /// </summary>
    [HarmonyPatch(typeof(CompPowerTrader), nameof(CompPowerTrader.CompInspectStringExtra))]
    public class CompPowerTrader_CompInspectStringExtra
    {
        /// <summary>
        /// Used to hijack the inspect string and properlly display the power usage when LightsOut is modifying things
        /// </summary>
        /// <param name="__instance">The <see cref="CompPowerTrader"/> that is outputting a string</param>
        /// <param name="__result">The output message to print</param>
        /// <returns><see langword="false"/> to prevent the actual method from running</returns>
        public static bool Prefix(CompPowerTrader __instance, ref string __result)
        {
            float powerOutput = __instance.PowerOutput;
            if (powerOutput > 0 || __instance.Props.alwaysDisplayAsUsingPower) return true;
            float idleDraw = __instance.Props.idlePowerDraw;
            if (idleDraw >= 0 && Mathf.Approximately(powerOutput, -idleDraw)) return true;

            IStandbyComp standbyComp = __instance.parent.GetStandbyComp();
            if (standbyComp is null) return true;
            if (standbyComp.IsInStandby)
                __result = "InspectString_OnStandby".Translate();
            else
                __result = $"{"PowerNeeded".Translate()}: {(-__instance.PowerOutput).ToString("#####0")} W";
            __result += "\n" + CompPower_CompInspectStringExtra(__instance);
            return false;
        }

        /// <summary>
        /// A recreation of the CompPower.CompInspectStringExtra so that we still get that output
        /// </summary>
        /// <param name="compPower">The comp to pull the info from</param>
        /// <returns>The same output string as CompPower.CompInspectStringExtra</returns>
        private static string CompPower_CompInspectStringExtra(CompPower compPower)
        {
            if (compPower.PowerNet is null)
                return "PowerNotConnected".Translate();

            string value1 = (compPower.PowerNet.CurrentEnergyGainRate() /
                CompPower.WattsToWattDaysPerTick).ToString("F0");
            string value2 = compPower.PowerNet.CurrentStoredEnergy().ToString("F0");
            return "PowerConnectedRateStored".Translate(value1, value2);
        }
    }
}