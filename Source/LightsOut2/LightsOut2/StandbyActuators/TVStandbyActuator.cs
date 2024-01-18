using RimWorld;
using System.Collections.Generic;
using Verse;

namespace LightsOut2.StandbyActuators
{
    public class TVStandbyActuator : IStandbyActuator
    {
        public bool IsInStandby(ThingWithComps thing)
        {
            return !AnyPawnWatching(thing);
        }

        /// <summary>
        /// Determines if any pawn is watching the <paramref name="tv"/>
        /// </summary>
        /// <param name="tv">The television to check for</param>
        /// <returns>Whether any pawn is watching the <paramref name="tv"/></returns>
        private bool AnyPawnWatching(ThingWithComps tv)
        {
            if (tv is null)
                return false;

            IEnumerable<IntVec3> watchArea = WatchBuildingUtility.CalculateWatchCells(tv.def, tv.Position, tv.Rotation, tv.Map);
            foreach (IntVec3 cell in watchArea)
            {
                Pawn pawn = cell.GetFirstPawn(tv.Map);
                if (pawn.CurJob?.targetA.Thing == tv)
                    return true;
            }

            return false;
        }
    }
}