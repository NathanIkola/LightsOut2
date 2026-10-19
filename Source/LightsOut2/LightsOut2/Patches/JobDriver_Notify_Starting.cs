using HarmonyLib;
using Verse;
using Verse.AI;

namespace LightsOut2.Patches
{
    /// <summary>
    /// Handles jobs that begin with the pawn already at the building's use position.
    /// </summary>
    [HarmonyPatch(typeof(JobDriver), nameof(JobDriver.Notify_Starting))]
    public static class JobDriver_Notify_Starting
    {
        /// <summary>
        /// Notifies the in-use influencer when the pawn already occupies the job's destination.
        /// </summary>
        /// <param name="__instance">The job driver that is starting.</param>
        public static void Postfix(JobDriver __instance)
        {
            Pawn pawn = __instance?.GetActor();
            Thing target = __instance?.job?.targetA.Thing;
            if (pawn == null || target == null)
            {
                return;
            }

            if (JobDriver_Notify_PatherArrived.IsAtUsePosition(__instance, target, pawn))
            {
                JobDriver_Notify_PatherArrived.NotifyIfTargetIsInUseBuilding(__instance);
            }
        }
    }
}
