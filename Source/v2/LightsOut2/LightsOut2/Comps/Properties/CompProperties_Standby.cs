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
        /// Whether or not this thing is tagged as a light source
        /// </summary>
        public bool isLight = false;

        /// <summary>
        /// If true, this will prevent the comp from delaying standby mode
        /// </summary>
        public bool noDelay = false;

        /// <summary>
        /// The list of standby influencers to use
        /// </summary>
        public List<Type> standbyInfluencers = new List<Type>();
    }
}