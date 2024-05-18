using HarmonyLib;
using LightsOut2.Common;
using LightsOut2.Core.ExtensionMethods;
using LightsOut2.Core.StandbyComps;
using LightsOut2.ThingComps;
using Verse;
using Verse.AI;

namespace LightsOut2.Patches
{
    /// <summary>
    /// The patch responsible for notifying benches (et al) that a pawn has arrived at their location and they should turn on.
    /// </summary>
    [HarmonyPatch(typeof(JobDriver), nameof(JobDriver.Notify_PatherArrived))]
    public class JobDriver_Notify_PatherArrived
    {
        /// <summary>
        /// Determines if the given job driver has a target with a StandbyComp variant; if so,
        /// that comp is updated to ensure it accurately reflects the pawn's arrival, and another
        /// update is queued to the job's finish action so that it reflects the pawn leaving.
        /// </summary>
        /// <param name="__instance"></param>
        public static void Prefix(JobDriver __instance)
        {
            ThingWithComps thing = __instance.job?.targetA.Thing as ThingWithComps;
            if (thing is null) return;

            // retrieve the standby comp and update it
            IStandbyComp standbyComp = thing.GetStandbyComp();
            if (standbyComp is null) return;

            Pawn pawn = __instance.GetActor();
            standbyComp.UpdateStandbyFromActuator(null);
            if (!standbyComp.IsInStandby)
            {
                __instance.AddFinishAction((JobCondition) =>
                {
                    standbyComp.UpdateStandbyFromActuator(pawn);
                });
            }
        }
    }
}