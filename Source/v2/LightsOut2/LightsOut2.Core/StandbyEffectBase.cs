using System;
using Verse;

namespace LightsOut2.Core
{
    /// <summary>
    /// An interface for objects that can apply an effect due to the standby status
    /// </summary>
    public abstract class StandbyEffectBase : IExposable, IDisposable
    {
        /// <summary>
        /// Instantiates a new standby effect with the given parent
        /// </summary>
        /// <param name="parent">The parent being affected</param>
        public StandbyEffectBase(ThingWithComps parent)
        {
            _parent = parent;
        }

        /// <summary>
        /// Updates the standby status for this effect
        /// </summary>
        /// <param name="inStandby">Whether or not the parent is in standby</param>
        public void UpdateStandby(bool inStandby)
        {
            // no update taking place, ignore it
            if (inStandby == _inStandby) { return; }

            _inStandby = inStandby;
            OnStandbyChanged(!_inStandby, _inStandby);
        }

        /// <summary>
        /// Allows the effect to have some setup code
        /// </summary>
        public virtual void Initialize() { }

        /// <summary>
        /// Allows the effect to have setup code after spawning
        /// </summary>
        /// <param name="respawningAfterLoad">Whether or not this is respawning after loading</param>
        public virtual void PostSpawnSetup(bool respawningAfterLoad) { }

        /// <summary>
        /// Allows the effect to perform any saving/loading operations
        /// </summary>
        public virtual void ExposeData() { }

        /// <summary>
        /// Allows the effect to perform any tickwise logic that is necessary
        /// </summary>
        public virtual void Tick() { }

        /// <summary>
        /// Allows the effect to clean up any data when being disposed of
        /// </summary>
        public virtual void Dispose() { }

        /// <summary>
        /// Gets some text to display in the inspect panel for the building
        /// that is being affected when in debug mode
        /// </summary>
        /// <returns>Any helpful debug info</returns>
        public virtual string DebugInspectString()
        {
            return null;
        }

        /// <summary>
        /// Event called when the standby status is changed
        /// </summary>
        /// <param name="wasInStandby">What the standby status used to be</param>
        /// <param name="isInStandby">What the standby status now is</param>
        protected abstract void OnStandbyChanged(bool wasInStandby, bool isInStandby);

        /// <summary>
        /// The parent thing that this effect is attached to
        /// </summary>
        protected ThingWithComps _parent;

        /// <summary>
        /// Whether or not the building is in standby
        /// </summary>
        protected bool _inStandby;
    }
}