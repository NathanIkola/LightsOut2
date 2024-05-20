using Verse;
using LightsOut2.Core.StandbyActuators;

namespace LightsOut2.StandbyActuators
{
    /// <summary>
    /// The actuator in charge of turning a bench on and off
    /// </summary>
    public class BenchStandbyActuator : IStandbyActuator
    {
        public bool IsInStandby(ThingWithComps thing, Pawn actuatingPawn)
        {
            // no map, just quit out
            Map map = thing?.Map;
            if (map is null) return true;

            // if there's no pawn in the interaciton cell, quit
            Pawn pawn = thing.InteractionCell.GetFirstPawn(map);
            if (pawn is null) return true;

            // verify that the pawn is there for a job, not just in the cell
            Thing target = pawn.CurJob?.targetA.Thing;
            if (target != thing)
                return true;

            // if all of the above pass, then this shouldn't be in standby
            return false;
        }

        public bool ReadyToRun(ThingWithComps thing)
        {
            return thing.Map?.regionAndRoomUpdater?.Enabled ?? false;
        }
    }
}