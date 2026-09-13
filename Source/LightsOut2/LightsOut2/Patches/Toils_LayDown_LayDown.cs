using HarmonyLib;
using LightsOut2.Comps;
using LightsOut2.Extensions;
using RimWorld;
using System;
using Verse;
using Verse.AI;

namespace LightsOut2.Patches
{
    /// <summary>
    /// A patch used to detect when a pawn goes to bed and react accordingly
    /// </summary>
    [HarmonyPatch(typeof(Toils_LayDown), nameof(Toils_LayDown.LayDown))]
    public class Toils_LayDown_LayDown
    {
        /// <summary>
        /// Detects if a pawn is able to sleep while laying down, and if so initializes the job
        /// to detect sleeping/waking up (even if the pawn doesn't actually leave the bed)
        /// </summary>
        /// <param name="__result">The toil object to modify</param>
        /// <param name="canSleep">Whether or not the job allows the pawn to sleep</param>
        public static void Postfix(Toil __result, bool canSleep)
        {
            if (__result is null) { return; }
            if (canSleep && LightsOut2Settings.FlickLightsForSleepingPawns)
            {
                // add an action to be performed before the pawn actually goes to sleep
                // this is used to mark the room as dirty so that lights can be updated appropriately
                // and is also responsible for adding the finish action to re-evaluate the room when the pawn wakes up
                __result.AddPreInitAction(() =>
                {
                    // verify that the pawn is even possibly an occupant
                    Pawn pawn = __result.actor;
                    if (pawn is null || !pawn.CanBeOccupant()) { return; }
                    Room room = pawn.GetRoom();

                    // mark the room dirty and set it to be marked dirty after the pawn wakes up
                    // ignore this pawn when checking occupancy since they're going to be asleep in bed
                    LightsOut2Mod.StaticLogger.Trace($"Pawn {pawn} is going to sleep in room '{room?.ID.ToString() ?? ""}', flagging room for occupancy evaluation");
                    RoomOccupancyTrackerGameComp.Instance.FlagRoomForEvaluation(room, null);

                    __result.AddFinishAction(() =>
                    {
                        // it's important to get the room again in the callback because it could have changed
                        // (e.x., the pawn was asleep and a wall was broken)
                        Room updatedRoom = pawn.GetRoom();
                        LightsOut2Mod.StaticLogger.Trace($"Pawn {pawn} has woken up, flagging room '{updatedRoom?.ID.ToString() ?? ""}' for occupancy evaluation");
                        RoomOccupancyTrackerGameComp.Instance.FlagRoomForEvaluation(updatedRoom, null);
                    });
                });

                // additionally, hook into the tick action to detect mid-job sleep/wake transitions
                // this can happen in cases where a pawn is bedridden (e.x., in the hospital) and falls asleep/wakes up without ever leaving bed
                // since we can't add an additional tick action, we have to wrap the existing one, so grab it first
                Action tickAction = __result.tickAction;
                // then create our own tick action which wraps the existing one
                __result.tickAction = () =>
                {
                    Pawn pawn = __result.actor;
                    bool? isSleepingBefore = pawn?.jobs.curDriver?.asleep;
                    
                    tickAction();
                    
                    bool? isSleepingAfter = pawn?.jobs.curDriver?.asleep;
                    // if the pawn isn't valid or their sleep state didn't change, ignore it
                    if (isSleepingBefore == isSleepingAfter || !pawn.CanBeOccupant()) { return; }

                    Room room = pawn.GetRoom();
                    if (room is null) { return; }

                    LightsOut2Mod.StaticLogger.Trace($"Pawn {pawn} has {(isSleepingAfter == true ? "fallen asleep" : "woken up")}, flagging room '{room?.ID.ToString() ?? ""}' for occupancy evaluation");
                    RoomOccupancyTrackerGameComp.Instance.FlagRoomForEvaluation(room, null);
                };
            }
        }
    }
}