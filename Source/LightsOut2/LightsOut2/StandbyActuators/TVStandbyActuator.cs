using RimWorld;
using System.Collections.Generic;
using Verse;

namespace LightsOut2.StandbyActuators
{
    public class TVStandbyActuator : IStandbyActuator
    {
        public bool IsInStandby(ThingWithComps thing, Pawn pawn)
        {
            return !AnyPawnWatching(thing, pawn);
        }

        /// <summary>
        /// Determines if any pawn is watching the <paramref name="tv"/>
        /// </summary>
        /// <param name="tv">The television to check for</param>
        /// <param name="pawnToIgnore">A <see cref="Pawn"/> to ignore when searching</param>
        /// <returns>Whether any pawn is watching the <paramref name="tv"/></returns>
        private bool AnyPawnWatching(ThingWithComps tv, Pawn pawnToIgnore)
        {
            if (tv is null || tv.Map is null)
                return false;

            IEnumerable<IntVec3> watchArea = WatchBuildingUtility.CalculateWatchCells(tv.def, tv.Position, tv.Rotation, tv.Map);
            if (watchArea is null) return false;

            foreach (IntVec3 cell in watchArea)
            {
                Pawn pawn = cell.GetFirstPawn(tv.Map);
                if (pawn == pawnToIgnore) continue;
                if (pawn?.CurJob?.targetA.Thing == tv)
                    return true;
            }

            return false;
        }
    }
}