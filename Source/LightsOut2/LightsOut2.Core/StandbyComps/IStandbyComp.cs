using LightsOut2.Core.StandbyActuators;
using Verse;

namespace LightsOut2.Core.StandbyComps
{
    public abstract class IStandbyComp : ThingComp
    {
        /// <summary>
        /// A function for determining the rate given a specific standby state
        /// </summary>
        /// <param name="isInStandby">Whether or not the thing is in standby</param>
        /// <returns>The rate to modify the power draw by</returns>
        public abstract float GetRateAsStandbyStatus(bool isInStandby);

        /// <summary>
        /// Updates this comp's standby state
        /// </summary>
        /// <param name="pawn">The <see cref="Pawn"/> doing the actuation</param>
        /// <remarks>
        /// Not used by lights because it would be really inefficient to check the room
        /// once for each light to have them determine if they're in standby or not. It's
        /// much easier to check it for a single pawn and apply that result to any lights.
        /// </remarks>
        public virtual void UpdateStandbyFromActuator(Pawn pawn)
        {
            if (StandbyActuator is null) return;
            if (StandbyActuator.ReadyToRun(parent))
                IsInStandby = StandbyActuator.IsInStandby(parent, pawn);
            // if the actuator is not quite ready to run, defer it until it is ready
            else
                TickManager_DoSingleTick.AddDeferredTask(
                    () => StandbyActuator.ReadyToRun(parent),
                    () => IsInStandby = StandbyActuator.IsInStandby(parent, pawn));
        }

        /// <summary>
        /// The actuator used when turning this thing on or off
        /// </summary>
        public IStandbyActuator StandbyActuator { get; set; }

        /// <summary>
        /// Whether or not the owner of this comp is currently in standby
        /// </summary>
        public abstract bool IsInStandby { get; set; }

        /// <summary>
        /// Whether or not this comp is even enabled
        /// </summary>
        public abstract bool IsEnabled { get; set; }

        /// <summary>
        /// Calculates the rate to modify the power draw by
        /// </summary>
        public float Rate
        {
            get
            {
                // if it's not enabled, don't modify anything
                if (!IsEnabled) return 1f;
                // otherwise benches are subject to the standby/active rates from the settings
                return GetRateAsStandbyStatus(IsInStandby);
            }
        }

        /// <summary>
        /// A delegate for the boolean change events
        /// </summary>
        /// <param name="newValue">The new value of the boolean</param>
        /// <param name="fromSettings">Whether or not this change is coming from settings</param>
        public delegate void OnBoolChangedHandler(bool newValue, bool fromSettings);

        /// <summary>
        /// The event invoked when the standby status of a thing changes
        /// </summary>
        public event OnBoolChangedHandler OnStandbyChanged;

        /// <summary>
        /// A method that allows derived classes to raise this event
        /// </summary>
        /// <param name="newValue">The new standby value</param>
        protected void RaiseOnStandbyChanged(bool newValue, bool fromSettings)
        {
            OnStandbyChanged?.Invoke(newValue, fromSettings);
        }
    }
}