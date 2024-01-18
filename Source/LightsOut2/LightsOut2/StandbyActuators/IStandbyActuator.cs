using Verse;

namespace LightsOut2.StandbyActuators
{
    public interface IStandbyActuator
    {
        /// <summary>
        /// Determines whether the given <paramref name="thing"/> is in standby
        /// </summary>
        /// <param name="thing">The <see cref="ThingWithComps"/> to check</param>
        /// <param name="pawn">The <see cref="Pawn"/> doing the actuation</param>
        /// <returns>Whether or not the <paramref name="thing"/> is in standby</returns>
        bool IsInStandby(ThingWithComps thing, Pawn pawn);

        /// <summary>
        /// Determines if this IStandbyActuator is ready to run
        /// </summary>
        /// <param name="thing">The <see cref="ThingWithComps"/> to check</param>
        /// <returns>Whether or not this actuator is ready to run</returns>
        bool ReadyToRun(ThingWithComps thing);
    }
}