using jl08lib.Logging;
using LightsOut2.Extensions;
using System.Collections.Generic;
using Verse;
using LightsOut2.Logic;

namespace LightsOut2.Comps
{
    /// <summary>
    /// Connects the game independent occupancy algorithm to RimWorld rooms and pawns.
    /// </summary>
    internal sealed class RimWorldOccupancyState : OccupancyStateBase<Room, Pawn>
    {
        /// <inheritdoc />
        protected override bool IsDoorway(Room room) => room.IsDoorway;

        /// <inheritdoc />
        protected override RoomOccupancy GetState(Room room, ISet<Pawn> ignoredPawns) => room.GetLightingState(ignoredPawns);
    }

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
        public RoomOccupancyTrackerGameComp(Game _)
        {
            _occupancyState.Changed += InvokeOccupancyChangedHandler;
            _occupancyState.StateChanged += InvokeRoomStateChangedHandler;
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
            Logger.Trace($"Updating room for pawn {pawn} ({oldRoom?.ID} -> {newRoom?.ID})");
            _occupancyState.PawnMoved(pawn, newRoom, oldRoom);
        }

        /// <summary>
        /// Marks every room known to the tracker for reevaluation.
        /// </summary>
        public void MarkAllRoomsDirty()
        {
            Logger.Trace("Marking all rooms dirty");
            _occupancyState.MarkAllRoomsDirty();
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
            _occupancyState.EvaluateDirtyRooms();
        }

        /// <summary>
        /// Marks the room as dirty so that it gets evaluated next tick
        /// </summary>
        /// <param name="room">The room to mark as dirty</param>
        /// <param name="triggeringPawn">The pawn that triggered the evaluation (to ignore when checking)</param>
        public void FlagRoomForEvaluation(Room room, Pawn triggeringPawn)
        {
            _occupancyState.MarkRoomDirty(room, triggeringPawn);
        }

        /// <summary>
        /// Retrieves the last known occupancy status for the room
        /// </summary>
        /// <param name="room">The room to get the status for</param>
        /// <param name="lastStatus">The last known status</param>
        /// <returns>True if the status was found, false if no status existed</returns>
        public bool TryGetLastOccupancyStatus(Room room, out bool lastStatus)
        {
            return _occupancyState.TryGetOccupancyStatus(room, out lastStatus);
        }

        /// <summary>
        /// Retrieves the cached lighting state for a room.
        /// </summary>
        /// <param name="room">The room to check.</param>
        /// <param name="state">Receives the cached room state when one exists.</param>
        /// <returns>True when a cached status exists; otherwise, false.</returns>
        public bool TryGetRoomLightingState(Room room, out RoomOccupancy state)
        {
            return _occupancyState.TryGetRoomState(room, out state);
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
                foreach (Room room in _occupancyState.KnownRooms)
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
        /// Goes through the list of pawns and removes any that have been despawned
        /// </summary>
        private void RemoveDespawnedPawns()
        {
            foreach(Pawn pawn in _occupancyState.TrackedPawns)
            {
                if (!pawn.Spawned)
                {
                    _occupancyState.Forget(pawn);
                }
            }
        }

        /// <summary>
        /// Invokes the handler for a room's occupancy changing
        /// </summary>
        /// <param name="room">The room that had the occupancy updated</param>
        /// <param name="isOccupied">Whether or not the Room is currently occupied</param>
        private void InvokeOccupancyChangedHandler(Room room, bool isOccupied)
        {
            OnOccupancyChanged?.Invoke(room, isOccupied);
        }

        /// <summary>
        /// Publishes a room's new cached state to interested lights.
        /// </summary>
        /// <param name="room">The room whose state changed.</param>
        /// <param name="state">The room's new state.</param>
        private void InvokeRoomStateChangedHandler(Room room, RoomOccupancy state)
        {
            OnRoomStateChanged?.Invoke(room, state);
        }

        /// <summary>
        /// The logger to use
        /// </summary>
        private static LoggerBase Logger => LightsOut2Mod.StaticLogger;

        /// <summary>
        /// The occupancy state for rooms and tracked pawns.
        /// </summary>
        private readonly RimWorldOccupancyState _occupancyState = new RimWorldOccupancyState();

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

        /// <summary>
        /// Raised when a room's full occupancy state is first cached or changes.
        /// </summary>
        public event RoomStateUpdateHandler OnRoomStateChanged;

        /// <summary>
        /// Handles a change to a room's full occupancy state.
        /// </summary>
        /// <param name="room">The room whose state changed.</param>
        /// <param name="state">The room's new state.</param>
        public delegate void RoomStateUpdateHandler(Room room, RoomOccupancy state);
    }
}
