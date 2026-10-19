using HarmonyLib;
using LightsOut2.Comps;
using LightsOut2.StandbyInfluencers;
using Verse;
using Verse.AI;

namespace LightsOut2.Patches
{
    /// <summary>
    /// Notifies an attached in-use influencer when a job driver reaches its destination.
    /// </summary>
    [HarmonyPatch(typeof(JobDriver), nameof(JobDriver.Notify_PatherArrived))]
    public static class JobDriver_Notify_PatherArrived
    {
        /// <summary>
        /// Starts tracking the job when its primary target has an in-use influencer.
        /// </summary>
        /// <param name="__instance">The job driver whose pawn arrived.</param>
        public static void Postfix(JobDriver __instance)
        {
            NotifyIfTargetIsInUseBuilding(__instance);
        }

        /// <summary>
        /// Finds the primary target's in-use influencer and notifies it about the arriving job.
        /// </summary>
        /// <param name="driver">The job driver to check.</param>
        internal static void NotifyIfTargetIsInUseBuilding(JobDriver driver)
        {
            Thing target = driver?.job?.targetA.Thing;
            Pawn pawn = driver?.GetActor();
            if (target is not ThingWithComps targetWithComps || pawn == null || !IsAtUsePosition(driver, target, pawn))
            {
                return;
            }

            StandbyComp standbyComp = targetWithComps.TryGetComp<StandbyComp>();
            if (standbyComp != null && standbyComp.TryGetInfluencer(out InUseInfluencer influencer))
            {
                influencer.NotifyJobArrived(driver);
            }
        }

        /// <summary>
        /// Determines whether the pawn occupies the building's interaction cell or job destination.
        /// </summary>
        /// <param name="driver">The job driver being evaluated.</param>
        /// <param name="target">The job's primary target.</param>
        /// <param name="pawn">The pawn performing the job.</param>
        /// <returns>True if the pawn is at the building's use position.</returns>
        internal static bool IsAtUsePosition(JobDriver driver, Thing target, Pawn pawn)
        {
            if (driver is RimWorld.JobDriver_WatchBuilding)
            {
                return pawn.Position == driver.job.GetTarget(TargetIndex.B).Cell;
            }

            if (target.def.hasInteractionCell && target is Building building)
            {
                return pawn.Position == building.InteractionCell;
            }

            return pawn.Position == driver.job.GetTarget(TargetIndex.B).Cell;
        }
    }
}
