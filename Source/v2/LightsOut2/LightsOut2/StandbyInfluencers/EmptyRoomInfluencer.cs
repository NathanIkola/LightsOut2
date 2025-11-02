using LightsOut2.Comps;
using LightsOut2.Core;
using LightsOut2.Extensions;
using System.Text;
using Verse;

namespace LightsOut2.StandbyInfluencers
{
    /// <summary>
    /// Influencer that reacts to whether the room is empty or not
    /// </summary>
    public sealed class EmptyRoomInfluencer : StandbyInfluencerBase
    {
        public EmptyRoomInfluencer(ThingWithComps parent)
            : base(parent) { }

        public override void Initialize()
        {
            base.Initialize();
            SubscribeToRoomChanges();
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            TrySetInitialState();
        }

        public override void Dispose()
        {
            LightsOut2Mod.StaticLogger.Trace($"Disposing EmptyRoomInfluencer on ThingComp with def '{_parent.def.defName}'");
            base.Dispose();
            UnsubscribeFromRoomChanges();
        }

        /// <summary>
        /// This only wnats to be in standby mode if the room is empty
        /// </summary>
        public override bool WantsToBeInStandby => _roomEmpty;

        public override string DebugInspectString()
        {
            StringBuilder sb = new StringBuilder(base.DebugInspectString());

            Room parentRoom = _parent.GetRoom();
            sb.AppendLine($"Room ID: {parentRoom.ID}");
            sb.AppendLine($"Room empty: {_roomEmpty}");
            if(!_roomEmpty)
            {
                StringBuilder occupants = new StringBuilder();
                bool first = true;
                foreach(Pawn pawn in parentRoom.Occupants())
                {
                    if (!first) { occupants.Append(", "); }
                    occupants.Append($"{pawn}");
                    first = false;
                }
                sb.AppendLine($"Occupants: {occupants}");
            }

            return sb.ToString().Trim();
        }

        public override void Tick()
        {
            base.Tick();
            if (!_hasSetInitialState) { TrySetInitialState(); }
        }

        /// <summary>
        /// Attempts to set the initial state for the influencer
        /// </summary>
        public void TrySetInitialState()
        {
            Room parentRoom = _parent.GetRoom();
            if (parentRoom is null) { return; }

            RoomOccupancyTrackerGameComp.Instance.TryGetLastOccupancyStatus(parentRoom, out bool isOccupied);
            _roomEmpty = !isOccupied;
            LightsOut2Mod.StaticLogger.Trace($"EmptyRoomInfluencer on ThingComp with def '{_parent.def.defName}' set initial state to {(isOccupied ? "occupied" : "empty")} for Room ID {parentRoom.ID}");
            _hasSetInitialState = true;
        }

        /// <summary>
        /// A handler for when the room's occupancy changes
        /// </summary>
        /// <param name="room">The room that experienced an occupancy change</param>
        /// <param name="isOccupied">Whether or not it's currently occupied</param>
        public void OnOccupancyChangedHandler(Room room, bool isOccupied)
        {
            if (room is null) { return; }

            _hasSetInitialState = true; // if we haven't set it yet, there's no reason to try setting it now
            if (!ShouldBeRunning())
            {
                Dispose();
                return;
            }

            Room parentRoom = _parent.GetRoom();
            if (room == parentRoom)
            {
                LightsOut2Mod.StaticLogger.Trace($"EmptyRoomInfluencer on ThingComp with def '{_parent.def.defName}' detected occupancy change to {(isOccupied ? "occupied" : "empty")} for Room ID {room.ID}");
                _roomEmpty = !isOccupied;
            }
        }

        /// <summary>
        /// Subscribes this instance to room change events
        /// </summary>
        private void SubscribeToRoomChanges()
        {
            RoomOccupancyTrackerGameComp.Instance.OnOccupancyChanged += OnOccupancyChangedHandler;
        }

        /// <summary>
        /// Unregisters this instance from the room change events
        /// </summary>
        private void UnsubscribeFromRoomChanges()
        {
            RoomOccupancyTrackerGameComp.Instance.OnOccupancyChanged -= OnOccupancyChangedHandler;
        }

        /// <summary>
        /// Determines if this influencer should still be influencing
        /// </summary>
        /// <returns>True if the state of the system looks good, false if not (e.x., the parent Thing is despawned)</returns>
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
        /// Whether the room that this light is in is currently empty
        /// </summary>
        private bool _roomEmpty;

        /// <summary>
        /// Whether or not the comp has successfully set up the initial state
        /// </summary>
        private bool _hasSetInitialState = false;

        /// <summary>
        /// Whether or not this influencer's parent has been seen as spawned
        /// </summary>
        private bool _hasParentEverSpawned = false;
    }
}