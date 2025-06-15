using LightsOut2.Extensions;
using System;
using UnityEngine.Analytics;
using Verse;

namespace LightsOut2.Comps
{
    public sealed class RoomOccupancyGameComp : GameComponent, IDisposable
    {
        /// <summary>
        /// An instance of the RoomOccupancyGameComp for the current game
        /// </summary>
        public static RoomOccupancyGameComp Instance => Current.Game.GetComponent<RoomOccupancyGameComp>();

        /// <summary>
        /// Constructor required by the game to initialize the component
        /// </summary>
        /// <param name="_">The game this is being initialized</param>
        public RoomOccupancyGameComp(Game _) 
        {
            RoomTrackerGameComp.Instance.OnRoomChanged += UpdateOccupancy;
        }

        /// <summary>
        /// Updates the occupancy of the rooms passed in
        /// </summary>
        /// <param name="pawn">The pawn that changed rooms</param>
        /// <param name="newRoom">The room the pawn is now in</param>
        /// <param name="oldRoom">The room the pawn was in before</param>
        public void UpdateOccupancy(Pawn pawn, Room newRoom, Room oldRoom)
        {
            // we know the current room is occupied, no need to check that
            if (newRoom != null) 
            { 
                OnRoomOccupancyUpdated?.Invoke(newRoom, true); 
            }
            if (oldRoom != null)
            {
                bool isOccupied = IsRoomOccupied(pawn, oldRoom);
                OnRoomOccupancyUpdated?.Invoke(oldRoom, isOccupied);
            }
        }

        /// <summary>
        /// Disposes of any resources
        /// </summary>
        public void Dispose()
        {
            RoomTrackerGameComp.Instance.OnRoomChanged -= UpdateOccupancy;
        }

        /// <summary>
        /// Checks to see if the given room is occupied
        /// </summary>
        /// <param name="toIgnore">The pawn to ignore</param>
        /// <param name="room">The room to check</param>
        /// <returns>Whether or not the room is currently occupied</returns>
        private bool IsRoomOccupied(Pawn toIgnore, Room room)
        {
            foreach(Pawn roomPawn in RoomTrackerGameComp.Instance.GetCachedPawns(room))
            {
                // skip null pawns or the pawn that just left the room
                if (roomPawn is null || roomPawn == toIgnore) { continue; }
                // otherwise check if the pawn should count
                if (roomPawn.ActivatesLights()) 
                { 
                    return true; 
                }
            }

            return false;
        }

        /// <summary>
        /// A delegate for getting updates about room occupancy
        /// </summary>
        /// <param name="room">The room that's getting the update</param>
        /// <param name="isOccupied">Whether the room is occupied</param>
        public delegate void RoomOccupancyUpdatedHandler(Room room, bool isOccupied);

        /// <summary>
        /// The event called when a room's occupancy has been updated
        /// </summary>
        public event RoomOccupancyUpdatedHandler OnRoomOccupancyUpdated;
    }
}