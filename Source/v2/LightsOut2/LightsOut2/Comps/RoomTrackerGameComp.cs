using jl08lib.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace LightsOut2.Comps
{
    /// <summary>
    /// A class that tracks the rooms that pawns are in
    /// </summary>
    public sealed class RoomTrackerGameComp : GameComponent, IDisposable
    {
        /// <summary>
        /// An instance of the RoomTrackerGameComp for the current game
        /// </summary>
        public static RoomTrackerGameComp Instance => Current.Game.GetComponent<RoomTrackerGameComp>();

        /// <summary>
        /// Constructor required by the game to initialize the component
        /// </summary>
        /// <param name="_">The game this is being initialized for</param>
        public RoomTrackerGameComp(Game _) { }

        /// <summary>
        /// Updates the room associated with the specified pawn
        /// </summary>
        /// <param name="pawn">The pawn to update the room for</param>
        /// <param name="newRoom">The room the pawn is now in</param>
        public void UpdateRoom(Pawn pawn, Room newRoom)
        {
            Room oldRoom = TryGetRoom(pawn);
            UpdateRoom(pawn, newRoom, oldRoom);
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
            if (!Logger.AssertNonNull(pawn)) { return; }
            if (!Logger.AssertNonNull(newRoom)) { return; }

            // optional integrity check to make sure the old room was correct
            if (LightsOut2Settings.EnableIntegrityChecks)
            {
                IntegrityCheckOneRoom(pawn, oldRoom);
            }

            Logger.Trace($"Updating room for pawn {pawn} ({oldRoom?.ID} -> {newRoom.ID})");
            _pawnRooms[pawn] = newRoom;

            OnRoomChanged?.Invoke(pawn, newRoom, oldRoom);
        }

        /// <summary>
        /// Retrieves the room associated with the specified pawn
        /// </summary>
        /// <param name="pawn">The pawn to get the room for</param>
        /// <returns>The room for the pawn</returns>
        public Room GetRoom(Pawn pawn)
        {
            // sanity check
            if (!Logger.AssertNonNull(pawn)) { return null; }

            Room room = TryGetRoom(pawn);
            if (room is null)
            {
                room = pawn.GetRoom();
                _pawnRooms[pawn] = room;
            }
            return room;
        }

        /// <summary>
        /// Retrieves the room associated with the specified pawn from the cache, if it exists
        /// </summary>
        /// <param name="pawn">The pawn to get the room for</param>
        /// <returns>The room for the pawn or null if it isn't found</returns>
        public Room TryGetRoom(Pawn pawn)
        {
            // sanity check
            if (!Logger.AssertNonNull(pawn)) { return null; }
            if (!_pawnRooms.ContainsKey(pawn)) { return null; }
            return _pawnRooms[pawn];
        }

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
        /// Run roughly every frame (not tick)
        /// </summary>
        public override void GameComponentUpdate()
        {
            base.GameComponentUpdate();
            
            IntegrityCheckUpdate();
            CollectionUpdate();
        }

        /// <summary>
        /// Disposes of the subscribed event handlers so they don't stick around
        /// </summary>
        public void Dispose()
        {
            // clean up cached pawns
            int numPawns = _pawnRooms.Count;
            _pawnRooms.Clear();
            LightsOut2Mod.StaticLogger.Trace($"Cleared {numPawns} pawn rooms from RoomTrackerGameComp");
        }

        /// <summary>
        /// Get all of the pawns in the cache
        /// </summary>
        /// <returns>All of the pawns in the cache</returns>
        public IEnumerable<Pawn> GetCachedPawns()
        {
            foreach(Pawn pawn in _pawnRooms.Keys)
            {
                yield return pawn;
            }
            yield break;
        }

        /// <summary>
        /// Gets all of the pawns in the cache in the specified room
        /// </summary>
        /// <param name="room">The room to get pawns in</param>
        /// <returns>The list of pawns in the room</returns>
        public IEnumerable<Pawn> GetCachedPawns(Room room)
        {
            foreach (KeyValuePair<Pawn, Room> pawnRoomPair in _pawnRooms)
            {
                if (pawnRoomPair.Value != room) { continue; }
                yield return pawnRoomPair.Key;
            }
            yield break;
        }

        /// <summary>
        /// Performs periodic integrity checks to ensure that the cached rooms are still correct
        /// </summary>
        private void IntegrityCheckUpdate()
        {
            // if integrity checks are disabled or it's not time to check, exit
            if (!LightsOut2Settings.EnableIntegrityChecks) { return; }
            if (++_integrityCheckUpdateCounter < LightsOut2Settings.FramesBetweenIntegrityChecks) { return; }

            _integrityCheckUpdateCounter = 0; // reset the counter
            Stopwatch sw = Stopwatch.StartNew();
            using (Logger.OpenSection("Beginning integrity check for RoomTrackerGameComp", LogLevel.Trace))
            {
                PerformIntegrityCheck();
            }
            sw.Stop();
            Logger.Trace($"Finished ({sw.ElapsedMilliseconds}ms)");
        }

        /// <summary>
        /// Performs periodic cleanup of the pawn list to remove any pawns that are no longer valid
        /// (e.g., dead pawns, pawns that have left the map, etc...)
        /// </summary>
        private void CollectionUpdate()
        {
            // not time to collectyet, exit
            if (++_collectionCounter < _collectionInterval) { return; }

            _collectionCounter = 0; // reset the counter
            int pawnsRemoved = 0;
            using (Logger.OpenSection($"Cleaning the pawn room cache", LogLevel.Trace))
            {
                pawnsRemoved = PerformCollection();
            }
            Logger.Trace($"Pawns removed: {pawnsRemoved}");
        }

        /// <summary>
        /// Perform the integrity checks and verify that all cached data is still valid
        /// </summary>
        private void PerformIntegrityCheck()
        {
            IntegrityCheckRooms();
        }

        /// <summary>
        /// Search through pawns and remove any ones that are no longer valid
        /// </summary>
        /// <returns>The number of pawns that were removed</returns>
        private int PerformCollection()
        {
            // make a list of pawns to remove (prevents modifying the dictionary while iterating)
            List<Pawn> pawnsToRemove = new List<Pawn>();
            foreach (Pawn pawn in _pawnRooms.Keys)
            {
                // ignore the pawn if it's supposed to be here
                if (PawnIsValid(pawn)) { continue; }
                // otherwise get ready to remove them
                pawnsToRemove.Add(pawn);
            }
            // remove any pawns that were marked for removal
            foreach (Pawn pawn in pawnsToRemove)
            {
                // remove the pawn from the cache
                _pawnRooms.Remove(pawn);
            }
            return pawnsToRemove.Count;
        }

        /// <summary>
        /// Verifies that the cached rooms for each pawn are still valid
        /// </summary>
        private void IntegrityCheckRooms()
        {
            foreach(Pawn pawn in _pawnRooms.Keys)
            {
                Room currentRoom = pawn.GetRoom();
                if (!IntegrityCheckOneRoom(pawn, currentRoom))
                {
                    // if the integrity check failed, update the cached room to the current one to recover
                    _pawnRooms[pawn] = currentRoom;
                }
            }
        }

        /// <summary>
        /// Performs the integrity check logic for a single pawn and room
        /// </summary>
        /// <param name="pawn">The pawn being checked</param>
        /// <param name="actualRoom"></param>
        /// <returns>True if the integrity check passes (no issue), false otherwise</returns>
        private bool IntegrityCheckOneRoom(Pawn pawn, Room actualRoom)
        {
            // if we didn't get an actual room, then ignore the integrity check
            if (actualRoom is null) { return true; }
            // ensure the cached room matches the actual room
            Room cachedRoom = TryGetRoom(pawn);
            if (cachedRoom is null || cachedRoom == actualRoom) { return true; }
            // if not, log a warning about the integrity violation and return the failure
            Logger.Warning($"Integrity violation for pawn {pawn}: cached room was {cachedRoom.ID} but should have been {actualRoom.ID}");
            return false;
        }

        /// <summary>
        /// Determines if the given pawn is valid for tracking
        /// </summary>
        /// <param name="pawn">The pawn to check</param>
        /// <returns>Whether or not the pawn is valid</returns>
        private static bool PawnIsValid(Pawn pawn)
        {
            // sanity check
            if (pawn is null) { return false; }
            // ensure the pawn is still alive and on the map
            if (pawn.Dead || !pawn.Spawned) { return false; }
            // ensure the pawn isn't destroyed
            if (pawn.Destroyed) { return false; }
            // otherwise they look good
            return true;
        }

        /// <summary>
        /// The logger to use
        /// </summary>
        private static LoggerBase Logger => LightsOut2Mod.StaticLogger;

        /// <summary>
        /// A counter to track the number of updates that have passed, used
        /// for integrity checking every so often
        /// </summary>
        private int _integrityCheckUpdateCounter = 0;

        /// <summary>
        /// A counter to track the number of updates that have passed, used
        /// for collection to clean up the list of cached rooms
        /// </summary>
        private int _collectionCounter;

        /// <summary>
        /// The number of updates to wait before doing culling pass on the pawn list
        /// </summary>
        private readonly int _collectionInterval = 3600;

        /// <summary>
        /// The list of rooms that each pawn is currently in
        /// </summary>
        private readonly Dictionary<Pawn, Room> _pawnRooms = new Dictionary<Pawn, Room>();

        /// <summary>
        /// A handler for the event raised when a pawn changes rooms
        /// </summary>
        /// <param name="pawn">The pawn that changed rooms</param>
        /// <param name="newRoom">The room the pawn is now in</param>
        /// <param name="oldRoom">The room the pawn left</param>
        public delegate void RoomChangedHandler(Pawn pawn, Room newRoom, Room oldRoom);

        /// <summary>
        /// The event raised when a pawn changes rooms
        /// </summary>
        public event RoomChangedHandler OnRoomChanged;
    }
}