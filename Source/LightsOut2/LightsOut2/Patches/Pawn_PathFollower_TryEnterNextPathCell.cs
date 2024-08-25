using HarmonyLib;
using LightsOut2.Common;
using Verse;
using Verse.AI;

namespace LightsOut2.Patches
{
    /// <summary>
    /// A class used to detect when pawns change rooms
    /// </summary>
    [HarmonyPatch(typeof(Pawn_PathFollower), "TryEnterNextPathCell")]
    public class Pawn_PathFollower_TryEnterNextPathCell
    {
        /// <summary>
        /// A function which captures the current room a pawn is in
        /// </summary>
        /// <param name="___pawn">The <see cref="Pawn"/> that's movung between rooms</param>
        /// <param name="__state">The room the pawn is in before the move</param>
        public static void Prefix(Pawn ___pawn, ref Room __state)
        {
            if (!ShouldPawnFlickLights(___pawn)) return;
            __state = ___pawn.GetRoom();
        }

        /// <summary>
        /// A function which determines if a pawn has swapped rooms
        /// </summary>
        /// <param name="___pawn">The <see cref="Pawn"/> that's movung between rooms</param>
        /// <param name="__state">The room the pawn is in before the move</param>
        public static void Postfix(Pawn ___pawn, ref Room __state)
        {
            if (!ShouldPawnFlickLights(___pawn)) return;
            Room newRoom = ___pawn.GetRoom();
            if (newRoom == __state) return;

            if (Utils.IsRoomEmpty(__state, ___pawn)) 
                RaiseOnRoomOccupancyChangedEvent(__state, false);
            if (Utils.IsRoomEmpty(newRoom, ___pawn))
                RaiseOnRoomOccupancyChangedEvent(newRoom, true);
        }

        /// <summary>
        /// A function which determines if it's appropriate for the given pawn to flick a light at all
        /// </summary>
        /// <param name="pawn">The <see cref="Pawn"/> in question</param>
        /// <returns>Whether or not the <paramref name="pawn"/> can flick lights</returns>
        private static bool ShouldPawnFlickLights(Pawn pawn)
        {
            // animals shouldn't flick lights unless this is true
            if (!LightsOut2Mod.AnimalsActivateLights && pawn.RaceProps.Animal) return false;
            // allow flicking lights for sleeping pawns
            if (LightsOut2Mod.TurnOffLightsInBed) return true;
            // otherwise fall back to the global override
            if (!LightsOut2Mod.FlickLights) return false;
            return true;
        }

        /// <summary>
        /// A function that allows external callers to raise the OnOccupancyChanged event
        /// </summary>
        /// <param name="room">The <see cref="Room"/> whose occupancy changed</param>
        /// <param name="hasOccupants">Whether or not the room has occupants</param>
        public static void RaiseOnRoomOccupancyChangedEvent(Room room, bool hasOccupants)
        {
            OnRoomOccupancyChanged?.Invoke(room, hasOccupants);
        }

        /// <summary>
        /// Callback template for when the occupancy status (has/does not have Pawn) changes
        /// </summary>
        /// <param name="room">The <see cref="Room"/> whose occupancy status changed</param>
        /// <param name="hasOccupants">Whether or not this room is occupied</param>
        public delegate void RoomOccupancyChangedHandler(Room room, bool hasOccupants);

        /// <summary>
        /// The event raised whenever the occupancy status of a room changes
        /// </summary>
        public static event RoomOccupancyChangedHandler OnRoomOccupancyChanged;
    }
}
