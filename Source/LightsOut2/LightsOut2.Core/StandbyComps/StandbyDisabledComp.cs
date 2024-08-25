using Verse;

namespace LightsOut2.Core.StandbyComps
{
    /// <summary>
    /// A StandbyComp that does absolutely nothing, allowing XML patches 
    /// to selectively disable standby on some buildings
    /// </summary>
    internal class StandbyDisabledComp : IStandbyComp
    {
        public override bool IsInStandby 
        {
            get => false;
            set => _ = value; 
        }

        public override bool IsEnabled 
        { 
            get => false;
            set => _ = value; 
        }

        public override float GetRateAsStandbyStatus(bool isInStandby)
        {
            return 1f;
        }

        public override void UpdateStandbyFromActuator(Pawn pawn)
        {
            return;
        }
    }
}