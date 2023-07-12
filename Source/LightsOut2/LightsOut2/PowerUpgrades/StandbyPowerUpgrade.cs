using RimWorld;
using Verse;

namespace LightsOut2.PowerUpgrades
{
    /// <summary>
    /// PowerUpgrade that sets the power draw to the active rate
    /// </summary>
    public class StandbyPowerUpgrade : CompProperties_Power.PowerUpgrade
    {
        public StandbyPowerUpgrade()
        {
            researchProject = DefDatabase<ResearchProjectDef>.GetNamed("Electricity");
        }

        /// <summary>
        /// Handler that sets the active research power rate
        /// </summary>
        /// <param name="isInStandby"></param>
        public void OnStandbyChanged(bool isInStandby)
        {
            factor = isInStandby ? LightsOut2Settings.StandbyPowerDraw : LightsOut2Settings.ActivePowerDraw;
        }
    }
}