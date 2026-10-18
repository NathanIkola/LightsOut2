using System.Collections.Generic;
using Verse;

using LightsOut2.Logic;

namespace LightsOut2.Extensions
{
    public static class Rooms
    {
        /// <summary>
        /// Determines the room's lighting state while ignoring specified pawns.
        /// </summary>
        /// <param name="room">The room to inspect.</param>
        /// <param name="ignoredPawns">Pawns excluded from this state calculation.</param>
        /// <returns>The room's empty, occupied-awake, or occupied-asleep state.</returns>
        public static RoomOccupancy GetLightingState(this Room room, ISet<Pawn> ignoredPawns)
        {
            bool foundOccupant = false;
            foreach (Pawn pawn in room.Occupants())
            {
                if (ignoredPawns != null && ignoredPawns.Contains(pawn)) { continue; }
                foundOccupant = true;
                if (!pawn.IsAsleep()) { return RoomOccupancy.OccupiedWithAwakePawn; }
            }
            return foundOccupant ? RoomOccupancy.OccupiedWithSleepingPawns : RoomOccupancy.Empty;
        }

        /// <summary>
        /// Retrieves the pawns in the room that count as occupants for light control.
        /// </summary>
        /// <param name="room">The room to check</param>
        /// <returns>The eligible occupants in the given room.</returns>
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
                if (occupant != toIgnore && occupant?.ActivatesLights() == true)
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
        public static bool IsOccupied(this Room room, ISet<Pawn> toIgnore)
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
