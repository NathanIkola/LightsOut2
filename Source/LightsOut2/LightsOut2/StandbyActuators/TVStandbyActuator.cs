using RimWorld;
using System.Collections.Generic;
using Verse;
using LightsOut2.Core.StandbyActuators;
using Verse.AI;

namespace LightsOut2.StandbyActuators
{
    /// <summary>
    /// The actuator that allows TVs to turn on and off
    /// </summary>
    public class TVStandbyActuator : IStandbyActuator
    {
        public bool IsInStandby(ThingWithComps thing, Pawn pawn)
        {
            // if the actuating pawn is still watching TV, it's not in standby
            JobDriver driver = pawn?.jobs?.curDriver;
            if (driver != null && !driver.ended) 
                return false;
            // otherwise check the pawns
            return !AnyPawnWatching(thing, pawn);
        }

        public bool ReadyToRun(ThingWithComps thing)
        {
            return thing.Map?.regionAndRoomUpdater?.Enabled ?? false;
        }

        /// <summary>
        /// Determines if any pawn is watching the <paramref name="tv"/>
        /// </summary>
        /// <param name="tv">The television to check for</param>
        /// <param name="pawnToIgnore">A <see cref="Pawn"/> to ignore when searching</param>
        /// <returns>Whether any pawn is watching the <paramref name="tv"/></returns>
        private bool AnyPawnWatching(ThingWithComps tv, Pawn pawnToIgnore)
        {
            if (tv is null || tv.Map is null || !(tv.Map.regionAndRoomUpdater?.Enabled ?? false))
                return false;

            IEnumerable<IntVec3> watchArea = WatchBuildingUtility.CalculateWatchCells(tv.def, tv.Position, tv.Rotation, tv.Map);
            if (watchArea is null) return false;

            foreach (IntVec3 cell in watchArea)
            {
                Pawn pawn = cell.GetFirstPawn(tv.Map);
                if (pawn == pawnToIgnore) continue;
                JobDriver driver = pawn?.jobs?.curDriver;
                // check that the pawn is currently engaged in a job, and is targeting the TV
                if (driver != null && !driver.ended && pawn?.CurJob?.targetA.Thing == tv)
                    return true;
            }

            return false;
        }
    }
}