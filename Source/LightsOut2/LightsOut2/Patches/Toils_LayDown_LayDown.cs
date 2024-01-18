using HarmonyLib;
using LightsOut2.Common;
using RimWorld;
using System;
using Verse;
using Verse.AI;

namespace LightsOut2.Patches
{
    [HarmonyPatch(typeof(Toils_LayDown), nameof(Toils_LayDown.LayDown))]
    public class Toils_LayDown_LayDown
    {
        public static void Postfix(Toil __result, bool canSleep)
        {
            // if we don't turn lights off in bed or the pawn can't sleep, ignore this patch
            if (!LightsOut2Settings.TurnOffLightsInBed || !canSleep)
                return;

            __result.AddPreInitAction(() =>
            {
                Pawn pawn = __result.actor;
                if (pawn is null) return;
                Room room = pawn.GetRoom();
                if (room is null) return;

                // if the room is empty besides this pawn, turn the lights off
                if (Utils.IsRoomEmpty(room, pawn))
                    SetRoomOccupiedStatus(room, false);

                // either way, ensure the pawn turns the lights back on when they wake up
                __result.AddFinishAction(() =>
                {
                    SetRoomOccupiedStatus(room, true);
                });
            });

            // we need to determine if the pawn's sleep status changes in the middle of the tick
            // there's no way to patch that, so hijack the tick action for this Toil
            Action tickAction = __result.tickAction;
            __result.tickAction = () =>
            {
                // get the pawn's sleep status before the tick
                Pawn pawn = __result.actor;
                bool wasAsleep = Utils.PawnIsAsleep(pawn);

                // now run the normal tick action
                tickAction();

                // if the sleeping status did not change, exit early
                if (wasAsleep == Utils.PawnIsAsleep(pawn)) return;

                // otherwise determine if the room is now occupied
                Room room = pawn.GetRoom();
                if (room is null) return;
                // if the pawn just woke up, the room is occupied
                bool occupied = wasAsleep || !Utils.IsRoomEmpty(room, pawn);
                SetRoomOccupiedStatus(room, occupied);
            };
        }

        private static void SetRoomOccupiedStatus(Room room, bool occupied)
        {
            Pawn_PathFollower_TryEnterNextPathCell.RaiseOnRoomOccupancyChangedEvent(room, occupied);
        }
    }
}