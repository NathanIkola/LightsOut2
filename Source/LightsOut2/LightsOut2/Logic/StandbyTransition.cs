using System;

namespace LightsOut2.Logic
{
    /// <summary>
    /// Tracks a requested standby state and its optional entry delay.
    /// </summary>
    public sealed class StandbyTransition
    {
        /// <summary>
        /// Gets whether standby is currently active.
        /// </summary>
        public bool InStandby { get; private set; }

        /// <summary>
        /// Gets whether standby is currently requested.
        /// </summary>
        public bool DesiresStandby { get; private set; }

        /// <summary>
        /// Advances the transition by one tick.
        /// </summary>
        /// <param name="desiresStandby">Whether all influencers request standby.</param>
        /// <param name="delayTicks">The number of ticks before entering standby.</param>
        /// <returns>Whether the active standby state changed.</returns>
        public bool Tick(bool desiresStandby, int delayTicks)
        {
            bool previousState = InStandby;
            bool previouslyDesiredStandby = DesiresStandby;

            // if we are not going into standby
            if (!desiresStandby)
            {
                DesiresStandby = false;
                InStandby = false;
                _remainingTicks = 0;
                return previousState != InStandby;
            }

            // if we are entering standby
            if (!previouslyDesiredStandby)
            {
                _remainingTicks = Math.Max(delayTicks, 0);
                DesiresStandby = true;
            }

            // if we are delaying standby, check if it's ready
            if (!InStandby && --_remainingTicks <= 0)
            {
                InStandby = true;
            }

            return previousState != InStandby;
        }

        /// <summary>
        /// Ticks remaining before standby begins.
        /// </summary>
        private int _remainingTicks;
    }
}
