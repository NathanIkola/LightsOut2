using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightsOut2.CompProperties
{
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
    }
}