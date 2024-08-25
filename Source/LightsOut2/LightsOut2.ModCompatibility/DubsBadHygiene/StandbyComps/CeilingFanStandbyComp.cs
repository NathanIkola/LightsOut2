using LightsOut2.ThingComps;

namespace LightsOut2.ModCompatibility.DubsBadHygiene.StandbyComps
{
    /// <summary>
    /// A class for the CeilingFan in DBH to allow flicking off the light
    /// without actually affecting power
    /// </summary>
    public class CeilingFanStandbyComp : StandbyLightComp
    {
        public override float GetRateAsStandbyStatus(bool isInStandby)
        {
            return 1f;
        }
    }
}