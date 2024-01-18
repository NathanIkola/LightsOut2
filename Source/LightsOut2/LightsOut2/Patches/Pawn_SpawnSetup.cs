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
            NotifyPawnSpawnedInRoom(__instance);

            JobDriver driver = __instance.jobs?.curDriver;
            if (driver is null) return;

            // job doesn't have a target, ignore it
            ThingWithComps thing = driver.job?.GetTarget(TargetIndex.A).Thing as ThingWithComps;
            if (thing is null) return;

            // ensure the target has a StandbyComp
            StandbyComp comp = Utils.GetStandbyComp(thing);
            if (comp is null) return;

            // if it does have a standby comp ensure the finish action is accounted for
            comp.UpdateStandbyFromActuator();
            if (!comp.IsInStandby)
            {
                driver.AddFinishAction(() =>
                {
                    comp.UpdateStandbyFromActuator();
                });
            }
        }

        private static void NotifyPawnSpawnedInRoom(Pawn pawn)
        {
            Room room = pawn.GetRoom();
            if (room is null) return;

            // if turning off lights for sleeping pawns and this pawn is sleeping, don't update lights
            // since they're either correctly off or on for a reason
            if (LightsOut2Settings.TurnOffLightsInBed && Utils.PawnIsAsleep(pawn))
                return;

            // otherwise, invoke the room occupancy change handler to notify any lights
            Pawn_PathFollower_TryEnterNextPathCell.RaiseOnRoomOccupancyChangedEvent(room, true);
        }
    }
}