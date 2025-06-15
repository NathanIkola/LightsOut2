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

        /// <summary>
        /// This only wnats to be in standby mode if the room is empty
        /// </summary>
        public override bool WantsToBeInStandby => _roomEmpty;

        public override string DebugInspectString()
        {
            return $"Room empty: {_roomEmpty}";
        }

        /// <summary>
        /// Whether the room that this light is in is currently empty
        /// </summary>
        private bool _roomEmpty;
    }
}