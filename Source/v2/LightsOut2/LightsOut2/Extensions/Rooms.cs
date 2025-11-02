using System.Collections.Generic;
using Verse;

namespace LightsOut2.Extensions
{
    public static class Rooms
    {
        /// <summary>
        /// Retrieves the Pawns in the room that are considered occupants (may activate lights)
        /// </summary>
        /// <param name="room">The room to check</param>
        /// <returns>The enumerable list of Pawns in the given Room</returns>
        public static IEnumerable<Pawn> Occupants(this Room room)
        {
            // loop over all of the Things in the room
            foreach (Thing thing in room.ContainedAndAdjacentThings)
            {
                if (thing is Pawn pawn && pawn.ActivatesLights()) { yield return pawn; }
            }
            yield break;
        }

        /// <summary>
        /// Checks to see if the given room is occupied
        /// </summary>
        /// <param name="room">The room to check</param>
        /// <param name="toIgnore">The pawn to ignore</param>
        /// <returns>Whether or not the room is currently occupied</returns>
        public static bool IsOccupied(this Room room, Pawn toIgnore = null)
        {
            foreach (Pawn occupant in room.Occupants())
            {
                if (occupant != null && occupant != toIgnore)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks to see if the given room is occupied, ignoring the given set of pawns
        /// </summary>
        /// <param name="room">The room to check</param>
        /// <param name="toIgnore">The list of pawns to ignore</param>
        /// <returns>Whether or not the room is currently occupied</returns>
        public static bool IsOccupied(this Room room, HashSet<Pawn> toIgnore)
        {
            foreach(Pawn occupant in room.Occupants())
            {
                if (occupant is null) { continue; }
                if (toIgnore is null || !toIgnore.Contains(occupant))
                {
                    return true;
                }
            }
            return false;
        }
    }
}