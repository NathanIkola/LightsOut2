using LightsOut2.Comps;
using LightsOut2.Core;
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
            TrySetInitialState();
        }

        public override void Dispose()
        {
            base.Dispose();
            UnsubscribeFromRoomChanges();
        }

        /// <summary>
        /// This only wnats to be in standby mode if the room is empty
        /// </summary>
        public override bool WantsToBeInStandby => _roomEmpty;

        public override string DebugInspectString()
        {
            return $"Room empty: {_roomEmpty}";
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

            RoomOccupancyTrackerGameComp.Instance.TryGetLastOccupancyStatus(_parent.GetRoom(), out bool isOccupied);
            _roomEmpty = !isOccupied;
            _hasSetInitialState = true;
        }

        /// <summary>
        /// A handler for when the room's occupancy changes
        /// </summary>
        /// <param name="room">The room that experienced an occupancy change</param>
        /// <param name="isOccupied">Whether or not it's currently occupied</param>
        public void OnOccupancyChangedHandler(Room room, bool isOccupied)
        {
            _hasSetInitialState = true; // if we haven't set it yet, there's no reason to try setting it now
            if (!ShouldBeRunning())
            {
                Dispose();
                return;
            }

            Room parentRoom = _parent.GetRoom();
            if (room == parentRoom)
            {
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
            if (_parent.Spawned) { return true; }

            if (LightsOut2Settings.EnableIntegrityChecks)
            {
                LightsOut2Mod.StaticLogger.Warning($"EmptyRoomInfluencer on ThingComp with def '{_parent.def.defName}' did not unregister from room change events before despawning");
            }
            return false;
        }

        /// <summary>
        /// Whether the room that this light is in is currently empty
        /// </summary>
        private bool _roomEmpty;

        /// <summary>
        /// Whether or not the comp has successfully set up the initial state
        /// </summary>
        private bool _hasSetInitialState = false;
    }
}