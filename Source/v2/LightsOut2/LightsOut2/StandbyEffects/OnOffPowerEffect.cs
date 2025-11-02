using Verse;

namespace LightsOut2.StandbyEffects
{
    /// <summary>
    /// An effect that sets power to either 0% or 100% depending on standby
    /// </summary>
    public sealed class OnOffPowerEffect : ReducePowerEffect
    {
        public OnOffPowerEffect(ThingWithComps parent) 
            : base(parent) { }

        /// <summary>
        /// Returns a coefficient of 100% power draw when active
        /// </summary>
        /// <returns>1f</returns>
        protected override float ActiveDrawCoefficientDecimal()
        {
            return 1f;
        }

        /// <summary>
        /// Returns a coefficient of 0% power draw when in standby
        /// </summary>
        /// <returns>0f</returns>
        protected override float StandbyDrawCoefficientDecimal()
        {
            return 0f;
        }
    }
}