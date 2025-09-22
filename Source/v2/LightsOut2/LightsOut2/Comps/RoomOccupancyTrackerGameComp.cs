using jl08lib.Logging;
using LightsOut2.Extensions;
using System;
using System.Collections.Generic;
using Verse;

namespace LightsOut2.Comps
{
    /// <summary>
    /// A class that tracks the rooms that pawns are in
    /// </summary>
    public sealed class RoomOccupancyTrackerGameComp : GameComponent, IDisposable
    {
        /// <summary>
        /// An instance of the RoomTrackerGameComp for the current game
        /// </summary>
        public static RoomOccupancyTrackerGameComp Instance => Current.Game.GetComponent<RoomOccupancyTrackerGameComp>();

        /// <summary>
        /// Constructor required by the game to initialize the component
        /// </summary>
        /// <param name="_">The game this is being initialized for</param>
        public RoomOccupancyTrackerGameComp(Game _) { }

        /// <summary>
        /// Resets the room tracker when starting a new game
        /// </summary>
        public override void StartedNewGame()
        {
            base.StartedNewGame();
            using (LightsOut2Mod.StaticLogger.OpenSection($"RoomTracker starting new game", LogLevel.Trace))
            {
                Dispose();
            }
        }

        /// <summary>
        /// Resets the room tracker when loading a saved game
        /// </summary>
        public override void LoadedGame()
        {
            base.LoadedGame();
            using (LightsOut2Mod.StaticLogger.OpenSection($"RoomTracker loading game", LogLevel.Trace))
            {
                Dispose();
            }
        }

        /// <summary>
        /// Cleans up the cache
        /// </summary>
        public void Dispose()
        {
            int numRooms = _roomOccupancy.Count;
            _roomOccupancy.Clear();
            _dirtyRooms.Clear();
            Logger.Trace($"Cleared {numRooms} rooms from {nameof(RoomOccupancyTrackerGameComp)} cache");
        }

        /// <summary>
        /// Updates the room associated with the specified pawn
        /// </summary>
        /// <param name="pawn">The pawn to update the room for</param>
        /// <param name="newRoom">The room the pawn is now in</param>
        /// <param name="oldRoom">The room the pawn was in before</param>
        public void UpdateRoom(Pawn pawn, Room newRoom, Room oldRoom)
        {
            // sanity checks
            if (!Logger.AssertNonNull(pawn, nameof(pawn))) { return; }
            if (!Logger.AssertNonNull(newRoom, nameof(newRoom))) { return; }
            if (!Logger.Assert(newRoom != oldRoom, "New room is the same as old room")) { return; }

            Logger.Trace($"Updating room for pawn {pawn} ({oldRoom?.ID} -> {newRoom.ID})");


            // mark the old room as needing re-evaluation since it might now be empty
            FlagRoomForEvaluation(oldRoom);
            SetOccupancy(newRoom, true);
        }

        /// <summary>
        /// Run roughly every frame (not tick)
        /// </summary>
        public override void GameComponentUpdate()
        {
            base.GameComponentUpdate();
            EvaluateDirtyRooms();
        }

        /// <summary>
        /// Reevaluates the occupancy of the rooms marked as dirty
        /// </summary>
        public void EvaluateDirtyRooms()
        {
            // grab and reset the currently dirty rooms
            HashSet<Room> roomsToEvaluate = _dirtyRooms;
            _dirtyRooms = new HashSet<Room>();

            foreach (Room room in roomsToEvaluate)
            {
                bool isOccupied = IsRoomOccupied(room);
                SetOccupancy(room, isOccupied);
            }
        }

        /// <summary>
        /// Retrieves the last known occupancy status for the room
        /// </summary>
        /// <param name="room">The room to get the status for</param>
        /// <param name="lastStatus">The last known status</param>
        /// <returns>True if the status was found, false if no status existed</returns>
        public bool TryGetLastOccupancyStatus(Room room, out bool lastStatus)
        {
            if (!_roomOccupancy.TryGetValue(room, out lastStatus)) { return false; }
            return true;
        }

        /// <summary>
        /// Marks the room as dirty so that it gets evaluated next tick
        /// </summary>
        /// <param name="room">The room to mark as dirty</param>
        private void FlagRoomForEvaluation(Room room)
        {
            if (room is null) { return; }

            _dirtyRooms.Add(room);
        }

        /// <summary>
        /// Update the cached occupancy value. If the occupancy changes, fires off the event
        /// </summary>
        /// <param name="room">The room to update</param>
        /// <param name="isOccupied">Whether or not it is currently occupied</param>
        private void SetOccupancy(Room room, bool isOccupied)
        {
            if (room is null) { return; }

            // if this room wasn't previously in the cache or the occupancy changed
            if (!TryGetLastOccupancyStatus(room, out bool wasOccupied) || isOccupied != wasOccupied)
            {
                OnOccupancyChanged?.Invoke(room, isOccupied);
            }

            // if the room is occupied, then we don't need to evaluate it
            if (isOccupied) { _dirtyRooms.Remove(room); }
            _roomOccupancy[room] = isOccupied;
        }

        /// <summary>
        /// Checks to see if the given room is occupied
        /// </summary>
        /// <param name="room">The room to check</param>
        /// <param name="toIgnore">The pawn to ignore</param>
        /// <returns>Whether or not the room is currently occupied</returns>
        private bool IsRoomOccupied(Room room, Pawn toIgnore = null)
        {
            if (room is null) { return false; }

            foreach (Pawn occupant in RoomPawns(room))
            {
                // skip null pawns or the pawn that just left the room
                if (occupant is null || occupant == toIgnore) { continue; }
                // otherwise check if the pawn should count
                if (occupant.ActivatesLights())
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Retrieves the Pawns in the room
        /// </summary>
        /// <param name="room">The room to check</param>
        /// <returns>The enumerable list of Pawns in the given Room</returns>
        private IEnumerable<Pawn> RoomPawns(Room room)
        {
            if (room is null) { yield break; }

            // loop over all of the Things in the room
            foreach(Thing thing in room.ContainedAndAdjacentThings)
            {
                if (thing is Pawn pawn) { yield return pawn; }
            }
            yield break;
        }

        /// <summary>
        /// The logger to use
        /// </summary>
        private static LoggerBase Logger => LightsOut2Mod.StaticLogger;

        /// <summary>
        /// The set of rooms that were marked dirty last tick
        /// </summary>
        private HashSet<Room> _dirtyRooms = new HashSet<Room>();

        /// <summary>
        /// Cached evaluation results
        /// </summary>
        private readonly Dictionary<Room, bool> _roomOccupancy = new Dictionary<Room, bool>();

        /// <summary>
        /// A handler for a Room's occupancy being updated
        /// </summary>
        /// <param name="room">The room that had the occupancy updated</param>
        /// <param name="isOccupied">Whether or not the Room is currently occupied</param>
        public delegate void OccupancyUpdateHandler(Room room, bool isOccupied);

        /// <summary>
        /// The event raised when there's an update to a Room's occupancy
        /// </summary>
        /// <remarks>
        /// This may be invoked even if the occupancy hasn't changed (it will just be invoked with the existing value)
        /// </remarks>
        public event OccupancyUpdateHandler OnOccupancyChanged;
    }
}