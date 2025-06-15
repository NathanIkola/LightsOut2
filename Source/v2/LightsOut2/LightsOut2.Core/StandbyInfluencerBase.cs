using System.Collections.Generic;
using Verse;

namespace LightsOut2.Core
{
    /// <summary>
    /// An interface for objects that influence the standby state
    /// </summary>
    public abstract class StandbyInfluencerBase : IExposable
    {
        /// <summary>
        /// Instantiates a new standby influencer with the given parent
        /// </summary>
        /// <param name="parent">The parent being influenced</param>
        public StandbyInfluencerBase(ThingWithComps parent)
        {
            _parent = parent;
        }

        /// <summary>
        /// Whether this influencer wishes to be in standby mode
        /// </summary>
        public abstract bool WantsToBeInStandby { get; }

        /// <summary>
        /// Whether this influencer is currently active
        /// </summary>
        public bool IsActive => !WantsToBeInStandby;

        /// <summary>
        /// Allows the influencer to perform any saving/loading operations
        /// </summary>
        public virtual void ExposeData() { }

        /// <summary>
        /// Retrieves the list of gizmos to display for this influencer
        /// </summary>
        /// <returns>A list of gizmos to display</returns>
        public virtual IEnumerable<Gizmo> GetGizmos()
        {
            yield break;
        }

        /// <summary>
        /// Gets some text to display in the inspect panel for the building
        /// that this influencer is attached to when in debug mode
        /// </summary>
        /// <returns>Any helpful debug info</returns>
        public virtual string DebugInspectString()
        {
            return null;
        }

        /// <summary>
        /// The parent thing that this influencer is attached to
        /// </summary>
        protected ThingWithComps _parent;
    }
}