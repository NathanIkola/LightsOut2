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
        /// <param name="isInStandby">Whether or not the comp is in standby</param>
        public void OnStandbyChanged(bool isInStandby)
        {
            if (IsEnabled)
                factor = isInStandby 
                    ? LightsOut2Settings.StandbyPowerDraw 
                    : LightsOut2Settings.ActivePowerDraw;
            else
                factor = 1;
        }

        /// <summary>
        /// Handler that sets whether or not this power upgrade should even be enabled
        /// </summary>
        /// <param name="isEnabled">Whether or not the comp is enabled</param>
        public void OnEnabledChanged(bool isEnabled)
        {
            IsEnabled = isEnabled;
        }

        /// <summary>
        /// Whether or not this power upgrade should affect anything
        /// </summary>
        private bool IsEnabled { get; set; }
    }
}