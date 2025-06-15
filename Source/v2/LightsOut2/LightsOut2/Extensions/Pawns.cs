using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace LightsOut2.Extensions
{
    public static class Pawns
    {
        /// <summary>
        /// Whether or not the given pawn should activate lights based on config
        /// </summary>
        /// <param name="pawn">The pawn to check</param>
        /// <returns>Whether or not the pawn should activate lights</returns>
        public static bool ActivatesLights(this Pawn pawn)
        {
            // pawn is an animal and animals are not configured to flick lights
            if (pawn.RaceProps.Animal && !LightsOut2Settings.AnimalsFlickLights) { return false; }
            // allowed to flick lights if they are in bed
            // this is to allow users to turn off general flicking of lights
            // but still allow pawns to turn off lights when pawns are sleeping
            if (LightsOut2Settings.NightLights) { return true; }
            // otherwise fall back to the the global setting
            return LightsOut2Settings.FlickLights;
        }
    }
}