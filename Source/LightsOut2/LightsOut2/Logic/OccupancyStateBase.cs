using System.Collections.Generic;
using System.Linq;

namespace LightsOut2.Logic
{
    /// <summary>
    /// Describes the occupant state of a room for light control
    /// </summary>
    public enum RoomOccupancy
    {
        /// <summary>
        /// The room has no eligible occupants
        /// </summary>
        Empty,

        /// <summary>
        /// The room has at least one eligible occupant who is awake
        /// </summary>
        OccupiedWithAwakePawn,

        /// <summary>
        /// The room has eligible occupants and all of them are asleep
        /// </summary>
        OccupiedWithSleepingPawns
    }
    
    /// <summary>
    /// Stores room occupancy and reconciles rooms affected by pawn movement.
    /// </summary>
    /// <typeparam name="TRoom">The room identity type.</typeparam>
    /// <typeparam name="TPawn">The pawn identity type.</typeparam>
    public abstract class OccupancyStateBase<TRoom, TPawn> 
        where TRoom : class where TPawn : class
    {
        /// <summary>
        /// Determines whether the supplied room is a doorway.
        /// </summary>
        /// <param name="room">The room to inspect.</param>
        /// <returns>Whether the room should be excluded from occupancy tracking.</returns>
        protected abstract bool IsDoorway(TRoom room);

        /// <summary>
        /// Gets the current state for a room, excluding pawns being reconciled.
        /// </summary>
        /// <param name="room">The room to inspect.</param>
        /// <param name="ignoredPawns">Pawns whose movement caused reevaluation.</param>
        /// <returns>The current state of the room.</returns>
        protected abstract RoomOccupancy GetState(TRoom room, ISet<TPawn> ignoredPawns);

        /// <summary>
        /// Raised when a room first becomes known or changes occupancy.
        /// </summary>
        public event OccupancyChangedHandler Changed;

        /// <summary>
        /// Raised when a room's full occupancy state is first cached or changes.
        /// </summary>
        public event RoomStateChangedHandler StateChanged;

        /// <summary>
        /// The delegate called when the room occupancy is changed
        /// </summary>
        /// <param name="room">The room whose occupancy was changed</param>
        /// <param name="isOccupied">Whether or not the room is now occupied</param>
        public delegate void OccupancyChangedHandler(TRoom room, bool isOccupied);

        /// <summary>
        /// The delegate called when a room's full occupancy state changes.
        /// </summary>
        /// <param name="room">The room whose state changed.</param>
        /// <param name="state">The room's new state.</param>
        public delegate void RoomStateChangedHandler(TRoom room, RoomOccupancy state);

        /// <summary>
        /// Gets the last evaluated status for a room.
        /// </summary>
        /// <param name="room">The room whose cached status is requested.</param>
        /// <param name="occupied">Receives the cached occupancy status when one exists.</param>
        /// <returns><see langword="true"/> when a cached status exists; otherwise, <see langword="false"/>.</returns>
        public bool TryGetOccupancyStatus(TRoom room, out bool occupied)
        {
            if (TryGetRoomState(room, out RoomOccupancy state))
            {
                occupied = IsOccupied(state);
                return true;
            }
            occupied = false;
            return false;
        }

        /// <summary>
        /// Gets the last evaluated state for a room.
        /// </summary>
        /// <param name="room">The room whose cached state is requested.</param>
        /// <param name="state">Receives the cached room state when one exists.</param>
        /// <returns>True when a cached state exists; otherwise, false.</returns>
        public bool TryGetRoomState(TRoom room, out RoomOccupancy state)
        {
            return _roomStates.TryGetValue(room, out state);
        }

        /// <summary>
        /// Records a pawn move and marks each former room for reconciliation.
        /// </summary>
        /// <param name="pawn">The pawn whose location changed.</param>
        /// <param name="newRoom">The pawn's new room, or <see langword="null"/> if it left all rooms.</param>
        /// <param name="reportedOldRoom">The room reported as the pawn's previous room.</param>
        public void PawnMoved(TPawn pawn, TRoom newRoom, TRoom reportedOldRoom)
        {
            if (pawn is null) { return; }

            // it's possible a room transition was missed, so get the last known room as well
            TryGetCurrentRoom(pawn, out TRoom knownOldRoom);
            // either set the new room for the pawn or remove them entirely
            if (newRoom is null) { _pawnRooms.Remove(pawn); }
            else { _pawnRooms[pawn] = newRoom; }
            
            MarkRoomDirty(reportedOldRoom, pawn);
            // if we did miss a transition, then mark the last known room dirty as well
            if (knownOldRoom != reportedOldRoom)
            {
                MarkRoomDirty(knownOldRoom, pawn);
            }

            // if we have a new room, set it to be occupied
            if (newRoom != null && !IsDoorway(newRoom))
            {
                Set(newRoom, GetState(newRoom, new HashSet<TPawn>()));
            }
        }

        /// <summary>
        /// Removes a despawned pawn and schedules its former room.
        /// </summary>
        /// <param name="pawn">The pawn to remove from tracking.</param>
        public void Forget(TPawn pawn)
        {
            if (TryGetCurrentRoom(pawn, out TRoom room))
            {
                _pawnRooms.Remove(pawn);
                MarkRoomDirty(room, pawn);
            }
        }

        /// <summary>
        /// Marks a room for reevaluation, optionally excluding the triggering pawn.
        /// </summary>
        /// <param name="room">The room to reevaluate.</param>
        /// <param name="pawn">The pawn to exclude from the occupancy check, if any.</param>
        public void MarkRoomDirty(TRoom room, TPawn pawn)
        {
            if (room == null) { return; }
            if (!_dirty.TryGetValue(room, out HashSet<TPawn> ignored))
            {
                ignored = new HashSet<TPawn>();
                _dirty.Add(room, ignored);
            }
            if (pawn != null) { ignored.Add(pawn); }
        }

        /// <summary>
        /// Marks each known room for reevaluation.
        /// </summary>
        public void MarkAllRoomsDirty()
        {
            foreach (TRoom room in _roomStates.Keys.ToList())
            {
                MarkRoomDirty(room, null);
            }

            foreach (TRoom room in _pawnRooms.Values.ToList()) 
            {
                MarkRoomDirty(room, null);
            }
        }

        /// <summary>
        /// Reevaluates dirty rooms against current physical occupants.
        /// </summary>
        public void EvaluateDirtyRooms()
        {
            Dictionary<TRoom, HashSet<TPawn>> pending = _dirty;
            _dirty = new Dictionary<TRoom, HashSet<TPawn>>();
            foreach (KeyValuePair<TRoom, HashSet<TPawn>> pair in pending)
            {
                // skip doorways
                TRoom dirtyRoom = pair.Key;
                if (IsDoorway(dirtyRoom)) { continue; }
                // update occupancy status
                HashSet<TPawn> ignoredPawns = pair.Value;
                RoomOccupancy state = GetState(dirtyRoom, ignoredPawns);
                Set(dirtyRoom, state);
            }
        }

        /// <summary>
        /// Gets all tracked pawns for despawn checking.
        /// </summary>
        public IEnumerable<TPawn> TrackedPawns => _pawnRooms.Keys.ToList();

        /// <summary>
        /// Gets rooms with cached occupancy for integrity checks.
        /// </summary>
        public IEnumerable<TRoom> KnownRooms => _roomStates.Keys.ToList();

        /// <summary>
        /// Sets a room status and emits a change only when needed.
        /// </summary>
        private void Set(TRoom room, RoomOccupancy state)
        {
            bool wasKnown = _roomStates.TryGetValue(room, out RoomOccupancy previousState);
            bool wasOccupied = wasKnown && IsOccupied(previousState);
            bool isOccupied = IsOccupied(state);
            _roomStates[room] = state;
            if (!wasKnown || previousState != state)
            {
                StateChanged?.Invoke(room, state);
            }
            if (!wasKnown || wasOccupied != isOccupied)
            {
                Changed?.Invoke(room, isOccupied);
            }

            if (isOccupied) { _dirty.Remove(room); }
        }

        /// <summary>
        /// Attempts to get the current room for a pawn
        /// </summary>
        /// <param name="pawn">The pawn to look up the current room for</param>
        /// <param name="currentRoom">The pawn's current room, or null if not found</param>
        /// <returns><see cref="true"/> if a room was found, <see cref="false"/> otherwise</returns>
        private bool TryGetCurrentRoom(TPawn pawn, out TRoom currentRoom)
        {
            return _pawnRooms.TryGetValue(pawn, out currentRoom);
        }

        /// <summary>
        /// Cached state for known rooms.
        /// </summary>
        private readonly Dictionary<TRoom, RoomOccupancy> _roomStates = new Dictionary<TRoom, RoomOccupancy>();

        /// <summary>
        /// Determines whether a lighting state represents an occupied room.
        /// </summary>
        /// <param name="state">The room lighting state.</param>
        /// <returns>True when the room has eligible occupants.</returns>
        private static bool IsOccupied(RoomOccupancy state)
        {
            return state != RoomOccupancy.Empty;
        }

        /// <summary>
        /// Last known room for each pawn.
        /// </summary>
        private readonly Dictionary<TPawn, TRoom> _pawnRooms = new Dictionary<TPawn, TRoom>();

        /// <summary>
        /// Rooms awaiting evaluation and the pawns which may have left them.
        /// </summary>
        private Dictionary<TRoom, HashSet<TPawn>> _dirty = new Dictionary<TRoom, HashSet<TPawn>>();
    }
}
