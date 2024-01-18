using Verse;

namespace LightsOut2.StandbyActuators
{
    public interface IStandbyActuator
    {
        /// <summary>
        /// Determines whether the given <paramref name="thing"/> is in standby
        /// </summary>
        /// <param name="thing">The <see cref="ThingWithComps"/> to check</param>
        /// <returns>Whether or not the <paramref name="thing"/> is in standby</returns>
        bool IsInStandby(ThingWithComps thing);
    }
}