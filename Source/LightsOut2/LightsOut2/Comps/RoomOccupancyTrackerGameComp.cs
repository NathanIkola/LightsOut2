using jl08lib.Logging;
using LightsOut2.Extensions;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace LightsOut2.Comps
{
    /// <summary>
    /// A class that tracks the rooms that pawns are in
    /// </summary>
    public sealed class RoomOccupancyTrackerGameComp : GameComponent
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

            // attempt to get the last known room for this pawn
            // sometimes pawns just... teleport for no good reason
            Room lastRoom = _pawnRooms.GetValueOrDefault(pawn);
            _pawnRooms[pawn] = newRoom;

            // mark the old room as needing re-evaluation since it might now be empty
            if (oldRoom != null && !oldRoom.IsDoorway)
            {
                FlagRoomForEvaluation(oldRoom, pawn);
            }
            // if we have a different last known room, then mark that as dirty too
            if (lastRoom != null && lastRoom != oldRoom)
            {
                FlagRoomForEvaluation(lastRoom, pawn);
            }
            // set the new room to be occupied
            if (newRoom != null && !newRoom.IsDoorway)
            {
                SetOccupancy(newRoom, true);
            }
        }

        /// <summary>
        /// Run roughly every frame (not tick)
        /// </summary>
        public override void GameComponentUpdate()
        {
            base.GameComponentUpdate();
            RemoveDespawnedPawns();
            EvaluateDirtyRooms();

            if (ShouldRunIntegrityChecks())
            {
                PerformIntegrityChecks();
            }
        }

        /// <summary>
        /// Reevaluates the occupancy of the rooms marked as dirty
        /// </summary>
        public void EvaluateDirtyRooms()
        {
            // grab and reset the currently dirty rooms
            Dictionary<Room, HashSet<Pawn>> roomsToEvaluate = _dirtyRooms;
            _dirtyRooms = new Dictionary<Room, HashSet<Pawn>>();

            foreach(KeyValuePair<Room, HashSet<Pawn>> pair in roomsToEvaluate)
            {
                Room room = pair.Key;
                HashSet<Pawn> pawnsToIgnore = pair.Value;
                bool isOccupied = room.IsOccupied(pawnsToIgnore);
                SetOccupancy(room, isOccupied);
            }
        }

        /// <summary>
        /// Marks the room as dirty so that it gets evaluated next tick
        /// </summary>
        /// <param name="room">The room to mark as dirty</param>
        /// <param name="triggeringPawn">The pawn that triggered the evaluation (to ignore when checking)</param>
        public void FlagRoomForEvaluation(Room room, Pawn triggeringPawn)
        {
            if (room is null) { return; }

            if (!_dirtyRooms.ContainsKey(room))
            {
                _dirtyRooms[room] = new HashSet<Pawn>();
            }

            // don't add null pawns to the list, just leave it empty
            if (triggeringPawn != null)
            {
                _dirtyRooms[room].Add(triggeringPawn);
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
            return _roomOccupancy.TryGetValue(room, out lastStatus);
        }

        /// <summary>
        /// Determines whether or not integrity checks should run this frame
        /// </summary>
        /// <returns>True if integrity checks should run, false otherwise</returns>
        private bool ShouldRunIntegrityChecks()
        {
            return LightsOut2Settings.EnableIntegrityChecks 
                && ++_framesSinceLastIntegrityCheck >= LightsOut2Settings.FramesBetweenIntegrityChecks;
        }

        /// <summary>
        /// Loops over the cached results and verifies that all rooms are correct
        /// </summary>
        private void PerformIntegrityChecks()
        {
            _framesSinceLastIntegrityCheck = 0;
            using(LightsOut2Mod.StaticLogger.OpenSection("Beginning RoomOccupancyTrackerGameComp integrity check", LogLevel.Trace))
            {
                foreach (Room room in _roomOccupancy.Keys)
                {
                    TryGetLastOccupancyStatus(room, out bool isOccupied);

                    // if flicking lights is enabled then we have to check for occupants
                    bool foundAnyOccupants = false;
                    foreach (Pawn occupant in room.Occupants())
                    {
                        foundAnyOccupants = true;
                        // if the room is occupied and we found an occupant, then it checks out
                        if (isOccupied) { break; }
                        // otherwise log a warning for the pawn that's not being counted
                        LightsOut2Mod.StaticLogger.Warning($"Integrity violation: Room {room} was expected to be empty, but found Pawn '{occupant}'");
                    }
                    if (isOccupied && !foundAnyOccupants)
                    {
                        LightsOut2Mod.StaticLogger.Warning($"Integrity violation: Room {room} was expected to be occupied, but found no occupants");
                    }
                }
            }
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
        /// Goes through the list of pawns and removes any that have been despawned
        /// </summary>
        private void RemoveDespawnedPawns()
        {
            List<Pawn> pawns = _pawnRooms.Keys.ToList();
            foreach(Pawn pawn in pawns)
            {
                if (!pawn.Spawned)
                {
                    _pawnRooms.Remove(pawn);
                }
            }
        }

        /// <summary>
        /// The logger to use
        /// </summary>
        private static LoggerBase Logger => LightsOut2Mod.StaticLogger;

        /// <summary>
        /// The set of rooms that were marked dirty last tick
        /// </summary>
        private Dictionary<Room, HashSet<Pawn>> _dirtyRooms = new Dictionary<Room, HashSet<Pawn>>();

        /// <summary>
        /// Cached evaluation results
        /// </summary>
        private readonly Dictionary<Room, bool> _roomOccupancy = new Dictionary<Room, bool>();

        /// <summary>
        /// A list of the last room we observed a Pawn to be in
        /// </summary>
        private readonly Dictionary<Pawn, Room> _pawnRooms = new Dictionary<Pawn, Room>();

        /// <summary>
        /// The frame counter used in integrity checking
        /// </summary>
        private int _framesSinceLastIntegrityCheck = 0;

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