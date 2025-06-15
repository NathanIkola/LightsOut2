using HarmonyLib;
using Verse;
using Verse.AI;
using LightsOut2.Comps;

namespace LightsOut2.Patches
{
    /// <summary>
    /// A class used to detect when a pawn changes rooms
    /// </summary>
    [HarmonyPatch(typeof(Pawn_PathFollower), "TryEnterNextPathCell")]
    public class Pawn_PathFollower_TryEnterNextPathCell
    {
        /// <summary>
        /// Tracks the room the pawn was in before entering the next path cell
        /// </summary>
        /// <param name="___pawn">The pawn to check</param>
        /// <param name="__state">The room the pawn was in before this tick</param>
        public static void Prefix(Pawn ___pawn, ref Room __state)
        {
            __state = ___pawn.GetRoom();
        }

        /// <summary>
        /// After potentially chaging cells, checks to see if the pawn has changed rooms
        /// </summary>
        /// <param name="___pawn">The pawn to check</param>
        /// <param name="__state">The room the pawn was in before this tick</param>
        public static void Postfix(Pawn ___pawn, ref Room __state)
        {
            Room currRoom = ___pawn.GetRoom();
            if (currRoom != __state)
            {
                RoomTrackerGameComp.Instance.UpdateRoom(___pawn, currRoom, __state);
            }
        }
    }
}