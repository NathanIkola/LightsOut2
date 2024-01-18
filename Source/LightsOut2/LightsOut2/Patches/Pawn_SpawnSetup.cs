using HarmonyLib;
using Verse;
using Verse.AI;
using LightsOut2.Common;
using LightsOut2.ThingComps;

namespace LightsOut2.Patches
{
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.SpawnSetup))]
    public class Pawn_SpawnSetup
    {
        public static void Postfix(Pawn __instance)
        {
            JobDriver driver = __instance.jobs?.curDriver;
            if (driver is null) return;

            // job doesn't have a target, ignore it
            ThingWithComps thing = driver.job?.GetTarget(TargetIndex.A).Thing as ThingWithComps;
            if (thing is null) return;

            // ensure the target has a StandbyComp
            StandbyComp comp = Utils.GetStandbyComp(thing);
            if (comp is null) return;

            // if it does have a standby comp ensure the finish action is accounted for
            comp.UpdateStandbyFromActuator(null);
            if (!comp.IsInStandby)
            {
                driver.AddFinishAction(() =>
                {
                    comp.UpdateStandbyFromActuator(__instance);
                });
            }
        }
    }
}