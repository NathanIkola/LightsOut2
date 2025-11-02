using System;
using System.Collections.Generic;
using Verse;

namespace LightsOut2.Comps.Properties
{
    /// <summary>
    /// The properties for standby comps
    /// </summary>
    public class CompProperties_Standby : CompProperties
    {
        /// <summary>
        /// If true, this will prevent the comp from delaying standby mode
        /// </summary>
        public bool noDelay = false;

        /// <summary>
        /// The list of standby influencers to use
        /// </summary>
        public List<Type> standbyInfluencers = new List<Type>();

        /// <summary>
        /// The list of standby effects to use
        /// </summary>
        public List<Type> standbyEffects = new List<Type>();
    }
}