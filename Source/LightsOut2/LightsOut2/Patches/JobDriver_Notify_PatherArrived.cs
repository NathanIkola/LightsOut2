using HarmonyLib;
using LightsOut2.Common;
using LightsOut2.ThingComps;
using Verse;
using Verse.AI;

namespace LightsOut2.Patches
{
    [HarmonyPatch(typeof(JobDriver), nameof(JobDriver.Notify_PatherArrived))]
    public class JobDriver_Notify_PatherArrived
    {
        public static void Prefix(JobDriver __instance)
        {
            ThingWithComps thing = __instance.job?.targetA.Thing as ThingWithComps;
            if (thing is null) return;

            // retrieve the standby comp and update it
            StandbyComp standbyComp = Utils.GetStandbyComp(thing);
            if (standbyComp is null) return;

            Pawn pawn = __instance.GetActor();
            standbyComp.UpdateStandbyFromActuator(null);
            if (!standbyComp.IsInStandby)
            {
                __instance.AddFinishAction(() =>
                {
                    standbyComp.UpdateStandbyFromActuator(pawn);
                });
            }
        }
    }
}