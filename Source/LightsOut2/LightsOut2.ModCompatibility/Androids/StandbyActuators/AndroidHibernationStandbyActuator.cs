using LightsOut2.Core.StandbyActuators;
using Verse;

namespace LightsOut2.ModCompatibility.Androids.StandbyActuators
{
    public class AndroidHibernationStandbyActuator : IStandbyActuator
    {
        public bool IsInStandby(ThingWithComps thing, Pawn pawn)
        {
            // check if there is a pawn standing on the hibernation pad
            return thing.InteractionCell.GetFirstPawn(thing.Map) != null;
        }

        public bool ReadyToRun(ThingWithComps thing)
        {
            return thing.Map?.regionAndRoomUpdater?.Enabled ?? false;
        }
    }
}
