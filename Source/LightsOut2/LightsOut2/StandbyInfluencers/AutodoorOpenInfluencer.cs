using HarmonyLib;
using LightsOut2.Core;
using RimWorld;
using Verse;

namespace LightsOut2.StandbyInfluencers
{
    /// <summary>
    /// Keeps an autodoor active while its door panels are moving.
    /// </summary>
    public sealed class AutodoorOpenInfluencer : StandbyInfluencerBase
    {
        /// <summary>
        /// Creates an influencer for the supplied autodoor.
        /// </summary>
        /// <param name="parent">The autodoor whose open state controls standby.</param>
        public AutodoorOpenInfluencer(ThingWithComps parent)
            : base(parent)
        {
            _door = (Building_Door)parent;
        }

        /// <summary>
        /// Returns whether the door has finished opening or closing.
        /// </summary>
        public override bool WantsToBeInStandby
        {
            get
            {
                // the door marks itself as "open" immediately upon the open transition starting
                // similarly that flag is cleared immediately upon the closing transition starting
                // we want to be in standby any time the door is at rest (fully open or fully closed)
                return _door.Open
                    // fully open: open + no ticks until fully openend
                    ? _door.TicksTillFullyOpened <= 0
                    // fully closed: closed + no ticks remaining until fully closed
                    : _getTicksSinceOpen(_door) <= 0;
            }
        }

        /// <summary>
        /// The cached delegate for RimWorld's non-public door closing progress counter.
        /// </summary>
        private static readonly AccessTools.FieldRef<Building_Door, int> _getTicksSinceOpen =
            AccessTools.FieldRefAccess<Building_Door, int>("ticksSinceOpen");

        /// <summary>
        /// The door whose open state controls standby.
        /// </summary>
        private readonly Building_Door _door;
    }
}
