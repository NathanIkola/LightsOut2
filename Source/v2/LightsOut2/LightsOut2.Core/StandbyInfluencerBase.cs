using System;
using System.Collections.Generic;
using Verse;

namespace LightsOut2.Core
{
    /// <summary>
    /// An interface for objects that influence the standby state
    /// </summary>
    public abstract class StandbyInfluencerBase : IExposable, IDisposable
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
        /// Allows the influencer to have some setup code
        /// </summary>
        public virtual void Initialize() { }

        /// <summary>
        /// Allows the influencer to have setup code after spawning
        /// </summary>
        /// <param name="respawningAfterLoad">Whether or not this is respawning after loading</param>
        public virtual void PostSpawnSetup(bool respawningAfterLoad) { }

        /// <summary>
        /// Whether this influencer wishes to be in standby mode
        /// </summary>
        public abstract bool WantsToBeInStandby { get; }

        /// <summary>
        /// Whether this influencer is currently active
        /// </summary>
        public bool BuildingIsActive => !WantsToBeInStandby;

        /// <summary>
        /// Allows the influencer to perform any saving/loading operations
        /// </summary>
        public virtual void ExposeData() { }

        /// <summary>
        /// Allows the influencer to perform any tickwise logic that is necessary
        /// </summary>
        public virtual void Tick() { }

        /// <summary>
        /// Allows the influencer to clean up any data when being disposed of
        /// </summary>
        public virtual void Dispose() { }

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