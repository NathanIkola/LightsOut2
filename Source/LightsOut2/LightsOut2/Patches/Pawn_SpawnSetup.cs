using HarmonyLib;
using Verse;
using Verse.AI;
using LightsOut2.Common;
using LightsOut2.ThingComps;

namespace LightsOut2.Patches
{
    /// <summary>
    /// The patch responsible for ensuring benches and lights get disabled in response to a pawn spawning in
    /// </summary>
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.SpawnSetup))]
    public class Pawn_SpawnSetup
    {
        /// <summary>
        /// Determines if the given pawn is actively in the middle of a job -- if it spawns already at the bench, then the PatherArrived
        /// event is never sent to the bench, meaning it won't activate itself. This essentially mirrors that logic.
        /// </summary>
        /// <param name="__instance"></param>
        public static void Postfix(Pawn __instance)
        {
            // the regionAndRoomUpdater error will be triggered while loading if ran here. Add it as a deferred task with the condition
            // that the regionAndRoomUpdater is enabled, allowing us to ensure the room is updated on the first tick it's viable to do so
            TickManager_DoSingleTick.AddDeferredTask(
                () => __instance.Map.regionAndRoomUpdater.Enabled,
                () => Pawn_PathFollower_TryEnterNextPathCell.RaiseOnRoomOccupancyChangedEvent(__instance.GetRoom(), true));

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
                driver.AddFinishAction((JobCondition) =>
                {
                    comp.UpdateStandbyFromActuator(__instance);
                });
            }
        }
    }
}