using LightsOut2.Core.StandbyComps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace LightsOut2.Core.ExtensionMethods
{
    public static class LightsOutExtensions
    {
        /// <summary>
        /// Retrieve the first StandbyComp encountered on the given <paramref name="thing"/>
        /// </summary>
        /// <param name="thing">The <see cref="ThingWithComps"/> to check</param>
        /// <returns>The <see cref="StandbyComp"/> or derived class from this <paramref name="thing"/></returns>
        public static IStandbyComp GetStandbyComp(this ThingWithComps thing)
        {
            if (thing.AllComps is null) return null;
            foreach (ThingComp comp in thing.AllComps)
                if (comp is IStandbyComp standby)
                    return standby;

            return null;
        }
    }
}