using HarmonyLib;
using LightsOut2.ThingComps;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace LightsOut2.Patches
{
    /// <summary>
    /// A patch that adds my power upgrade to objects with CompProperties_Power comps
    /// </summary>
    [HarmonyPatch(typeof(ThingDef), nameof(ThingDef.PostLoad))]
    public class ThingWithComps_InitializeComps
    {
        /// <summary>
        /// The patch that adds the PowerComp to a ThingDef as they are loaded into the game
        /// </summary>
        /// <param name="__instance"></param>
        public static void Prefix(ThingDef __instance)
        {
            if (HasPowerProps(__instance))
                __instance.comps.Add(new CompProperties() { compClass = typeof(StandbyComp) });
        }

        /// <summary>
        /// Determines if the def has a <see cref="CompProperties_Power"/> present that could be affected by this mod.
        /// Specifically, it will trigger for any power-consuming ThingDef.
        /// </summary>
        /// <param name="def">The <see cref="ThingDef"/> to check</param>
        /// <returns><see langword="true"/> for this def if it has one, or <see langword="false"/> otherwise</returns>
        private static bool HasPowerProps(ThingDef def)
        {
            if (def.comps is null) return false;

            foreach (CompProperties props in def.comps)
                if (props is CompProperties_Power powerProps && powerProps.PowerConsumption > 0) 
                    return true;
            return false;
        }
    }
}