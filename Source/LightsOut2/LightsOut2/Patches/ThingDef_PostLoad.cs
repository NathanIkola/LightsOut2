using HarmonyLib;
using LightsOut2.Common;
using LightsOut2.CompProperties;
using LightsOut2.ThingComps;
using RimWorld;
using Verse;

namespace LightsOut2.Patches
{
    /// <summary>
    /// A patch that adds my power upgrade to objects with CompProperties_Power comps
    /// </summary>
    [HarmonyPatch(typeof(ThingDef), nameof(ThingDef.PostLoad))]
    public class ThingDef_PostLoad
    {
        /// <summary>
        /// The patch that adds the PowerComp to a ThingDef as they are loaded into the game
        /// </summary>
        /// <param name="__instance"></param>
        public static void Prefix(ThingDef __instance)
        {
            CompProperties_Power powerProps = GetPowerProps(__instance);
            if (powerProps is null) return;

            bool startEnabled = true;
            bool isLight = !__instance.IsTable() && __instance.IsLight();
            if (!isLight && !StandbyComp.ShouldStartEnabled(__instance))
                startEnabled = false;

            // insert the additional StandbyComp
            __instance.comps.Add(new CompProperties_Standby
            {
                startEnabled = startEnabled,
                compClass = isLight 
                    ? typeof(StandbyLightComp) 
                    : typeof(StandbyComp)
            });
        }

        /// <summary>
        /// Retrieves the power props for the given def
        /// </summary>
        /// <param name="def">The <see cref="ThingDef"/> to look up</param>
        /// <returns>The assocaited <see cref="CompProperties_Power"/> if present, <see langword="null"/> otherwise</returns>
        private static CompProperties_Power GetPowerProps(ThingDef def)
        {
            if (def.comps is null) return null;
            foreach (Verse.CompProperties props in def.comps)
                if (props is CompProperties_Power powerProps && powerProps.compClass == typeof(CompPowerTrader)) return powerProps;
            return null;
        }
    }
}