using LightsOut2.Core;
using LightsOut2.Gizmos;
using System.Collections.Generic;
using Verse;

namespace LightsOut2.StandbyInfluencers
{
    /// <summary>
    /// An influencer that allows the user to keep the light on
    /// </summary>
    public class KeepOnInfluencer : StandbyInfluencerBase
    {
        public KeepOnInfluencer(ThingWithComps parent)
            : base(parent) { }

        /// <summary>
        /// This influencer only wants to be in standby mode if the keep on comp is activated
        /// </summary>
        public override bool WantsToBeInStandby => !_keepOnGizmo.KeepOn;

        public override void ExposeData()
        {
            base.ExposeData();
            
            bool keepOn = _keepOnGizmo.KeepOn;
            Scribe_Values.Look(ref keepOn, "KeepOn");
            _keepOnGizmo.KeepOn = keepOn;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            yield return _keepOnGizmo;
            yield break;
        }

        public override string DebugInspectString()
        {
            return $"Keep On: {_keepOnGizmo.KeepOn}";
        }

        /// <summary>
        /// The gizmo that allows the user to toggle the "keep on" mode for this light
        /// </summary>
        private readonly KeepOnGizmo _keepOnGizmo = new KeepOnGizmo();
    }
}