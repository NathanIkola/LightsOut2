using System;

namespace LightsOut2.CompProperties
{
    /// <summary>
    /// The comp properties passed from XML mods to the StandbyComp classes
    /// </summary>
    public class CompProperties_Standby : Verse.CompProperties
    {
        /// <summary>
        /// Whether or not this StandbyComp should start enabled
        /// </summary>
        public bool startEnabled;

        /// <summary>
        /// An option to override the GlowerActuator class being used for this StandbyLightComp
        /// </summary>
        public Type glowerActuatorClass;

        /// <summary>
        /// An option to override the StandbyActuator class being used for this StandbyComp
        /// </summary>
        public Type standbyActuatorClass;
    }
}