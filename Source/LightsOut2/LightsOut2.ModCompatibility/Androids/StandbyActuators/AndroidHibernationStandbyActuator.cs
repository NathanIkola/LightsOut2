using LightsOut2.Core.StandbyActuators;
using Verse;
using Verse.AI;

namespace LightsOut2.ModCompatibility.Androids.StandbyActuators
{
    public class AndroidHibernationStandbyActuator : IStandbyActuator
    {
        public bool IsInStandby(ThingWithComps thing, Pawn actuatingPawn)
        {
            // use the actuating pawn, otherwise check if there is a pawn standing on the hibernation pad
            Pawn pawn = actuatingPawn ?? thing.Position.GetFirstPawn(thing.Map);
            JobDriver driver = pawn?.jobs?.curDriver;
            // if there is no driver for some reason, just return in standby
            if (driver is null) return true;
            // otherwise, if the driver is active then it can't be in standby
            return driver.ended;
        }

        public bool ReadyToRun(ThingWithComps thing)
        {
            return thing.Map?.regionAndRoomUpdater?.Enabled ?? false;
        }
    }
}
