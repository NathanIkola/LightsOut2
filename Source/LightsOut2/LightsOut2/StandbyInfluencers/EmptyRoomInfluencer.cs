using LightsOut2.Comps;
using LightsOut2.Core;
using LightsOut2.Extensions;
using LightsOut2.Logic;
using System.Text;
using Verse;

namespace LightsOut2.StandbyInfluencers
{
    /// <summary>
    /// Influencer that reacts to whether the room is empty or all occupants are asleep.
    /// </summary>
    public sealed class EmptyRoomInfluencer : StandbyInfluencerBase
    {
        /// <summary>
        /// Creates an influencer for the supplied light.
        /// </summary>
        /// <param name="parent">The light whose room state controls standby.</param>
        public EmptyRoomInfluencer(ThingWithComps parent)
            : base(parent) { }

        /// <summary>
        /// Subscribes to changes in the room's cached occupancy state.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            SubscribeToRoomChanges();
        }

        /// <summary>
        /// Flags the light's room so the tracker initializes its cached lighting state.
        /// </summary>
        /// <param name="respawningAfterLoad">Whether the light is respawning after a save was loaded.</param>
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            TrySetInitialState();
        }

        /// <summary>
        /// Unsubscribes from changes in the room's cached occupancy state.
        /// </summary>
        public override void Dispose()
        {
            LightsOut2Mod.StaticLogger.Trace($"Disposing EmptyRoomInfluencer on ThingComp with def '{_parent.def.defName}'");
            base.Dispose();
            UnsubscribeFromRoomChanges();
        }

        /// <summary>
        /// Determines whether this light should enter standby based on the room's cached occupant state and settings.
        /// </summary>
        public override bool WantsToBeInStandby
        {
            get
            {
                return _hasRoomState
                    && (_roomState == RoomOccupancy.Empty && LightsOut2Settings.FlickLights
                        || _roomState == RoomOccupancy.OccupiedWithSleepingPawns && LightsOut2Settings.FlickLightsForSleepingPawns);
            }
        }

        /// <summary>
        /// Provides the current cached room lighting state for inspection.
        /// </summary>
        /// <returns>A description of the cached state and applicable settings.</returns>
        public override string DebugInspectString()
        {
            StringBuilder sb = new StringBuilder(base.DebugInspectString());
            Room parentRoom = _parent.GetRoom();
            sb.AppendLine($"Room ID: {parentRoom.ID}");
            sb.AppendLine($"Room empty: {_roomState == RoomOccupancy.Empty}");
            if (_hasRoomState && _roomState != RoomOccupancy.Empty)
            {
                StringBuilder occupants = new StringBuilder();
                bool first = true;
                foreach (Pawn pawn in parentRoom.Occupants())
                {
                    if (!first) { occupants.Append(", "); }
                    occupants.Append($"{pawn}");
                    first = false;
                }
                sb.AppendLine($"Occupants: {occupants}");
            }

            return sb.ToString().Trim();
        }

        /// <summary>
        /// Retries initialization until the room has a cached occupancy state.
        /// </summary>
        public override void Tick()
        {
            base.Tick();
            if (!_hasSetInitialState) { TrySetInitialState(); }
        }

        /// <summary>
        /// Initializes this influencer from the tracker's cached room state.
        /// </summary>
        public void TrySetInitialState()
        {
            Room parentRoom = _parent.GetRoom();
            if (parentRoom is null) { return; }

            RoomOccupancyTrackerGameComp tracker = RoomOccupancyTrackerGameComp.Instance;
            if (!tracker.TryGetRoomLightingState(parentRoom, out _))
            {
                tracker.FlagRoomForEvaluation(parentRoom, null);
                tracker.EvaluateDirtyRooms();
            }

            _hasRoomState = tracker.TryGetRoomLightingState(parentRoom, out _roomState);
            LightsOut2Mod.StaticLogger.Trace($"EmptyRoomInfluencer on ThingComp with def '{_parent.def.defName}' set initial state to {_roomState} for Room ID {parentRoom.ID}");
            _hasSetInitialState = _hasRoomState;
        }

        /// <summary>
        /// Handles a change to the room's cached occupancy state.
        /// </summary>
        /// <param name="room">The room whose state changed.</param>
        /// <param name="state">The room's new state.</param>
        public void OnRoomStateChanged(Room room, RoomOccupancy state)
        {
            if (room is null) { return; }

            _hasSetInitialState = true;
            if (!ShouldBeRunning())
            {
                Dispose();
                return;
            }

            if (room == _parent.GetRoom())
            {
                LightsOut2Mod.StaticLogger.Trace($"EmptyRoomInfluencer on ThingComp with def '{_parent.def.defName}' detected room state change to {state} for Room ID {room.ID}");
                _roomState = state;
                _hasRoomState = true;
            }
        }

        /// <summary>
        /// Subscribes this instance to room state changes.
        /// </summary>
        private void SubscribeToRoomChanges()
        {
            RoomOccupancyTrackerGameComp.Instance.OnRoomStateChanged += OnRoomStateChanged;
        }

        /// <summary>
        /// Unregisters this instance from room state changes.
        /// </summary>
        private void UnsubscribeFromRoomChanges()
        {
            RoomOccupancyTrackerGameComp.Instance.OnRoomStateChanged -= OnRoomStateChanged;
        }

        /// <summary>
        /// Determines whether this influencer should continue running.
        /// </summary>
        /// <returns>True if the parent has not despawned; otherwise, false.</returns>
        private bool ShouldBeRunning()
        {
            // not performing integrity checking, just return the simple answer
            bool canRun = !_hasParentEverSpawned || _parent.Spawned;
            if (!LightsOut2Settings.EnableIntegrityChecks)
            {
                return canRun;
            }

            // if the parent is currently spawned, there's no problem
            if (_parent.Spawned)
            {
                _hasParentEverSpawned = true;
            }
            // if the parent is despawned but has been spawned before, log a warning
            else if (!_parent.Spawned && _hasParentEverSpawned)
            {
                LightsOut2Mod.StaticLogger.Warning($"EmptyRoomInfluencer on ThingComp with def '{_parent.def.defName}' did not unregister from room change events before despawning");
            }

            return canRun;
        }

        /// <summary>
        /// The latest cached state for the light's room.
        /// </summary>
        private RoomOccupancy _roomState;

        /// <summary>
        /// Whether a room state has been received for the light's room.
        /// </summary>
        private bool _hasRoomState = false;

        /// <summary>
        /// Whether initial room state has been set.
        /// </summary>
        private bool _hasSetInitialState = false;

        /// <summary>
        /// Whether the parent has ever been observed as spawned.
        /// </summary>
        private bool _hasParentEverSpawned = false;

    }
}
