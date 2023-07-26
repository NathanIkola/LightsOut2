using HarmonyLib;
using LightsOut2.PowerUpgrades;
using LightsOut2.ThingComps;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace LightsOut2.Patches
{
    /// <summary>
    /// A simple patch that simply adds my power upgrade to CompProperties_Power objects
    /// </summary>
    [HarmonyPatch(typeof(ThingComp), nameof(ThingComp.Initialize))]
    public class ThingComp_Initialize
    {
        /// <summary>
        /// After initializing the power comp, ensure that we have the StandbyComp
        /// and the associated power upgrades
        /// </summary>
        /// <param name="__instance">The comp being initialized</param>
        /// <param name="__0">The properties for this comp</param>
        public static void Postfix(ThingComp __instance, CompProperties __0)
        {
            if (__0 is CompProperties_Power props && props.PowerConsumption > 0)
                EnsureStandbyComp(__instance.parent, props);
        }

        /// <summary>
        /// Ensure that the StandbyComp is present on <paramref name="thing"/> and return it
        /// </summary>
        /// <param name="thing">The ThingWithComps to check</param>
        /// <param name="props">The power props to add to if necessary</param>
        private static void EnsureStandbyComp(ThingWithComps thing, CompProperties_Power props)
        {
            if (thing.TryGetComp<StandbyComp>() is null)
            {
                StandbyComp comp = StandbyComp.CreateOnThing(thing);

                if (props.powerUpgrades is null)
                    props.powerUpgrades = new List<CompProperties_Power.PowerUpgrade>();

                // for some reason the Initialize seems to get hit twice so this keeps us from adding the power upgrade twice
                if (!props.powerUpgrades.Any(x => x is StandbyPowerUpgrade))
                {
                    StandbyPowerUpgrade upgrade = new StandbyPowerUpgrade();
                    comp.OnStandbyChanged += upgrade.OnStandbyChanged;
                    upgrade.OnStandbyChanged(comp.IsInStandby);
                    props.powerUpgrades.Add(upgrade);
                }
            }
        }
    }
}