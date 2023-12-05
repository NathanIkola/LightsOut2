using UnityEngine;
using Verse;

namespace LightsOut2
{
    /// <summary>
    /// All of the settings for the mod
    /// </summary>
    public class LightsOut2Settings : ModSettings
    {
        /// <summary>
        /// Whether or not to print debug messages to the console
        /// </summary>
        public static bool ShowDebugMessages = false;

        // other settings to use later
        public static bool FlickLights = true;
        public static bool TurnOffLightsInBed = true;
        public static bool AnimalsActivateLights = false;
        public static float StandbyPowerDraw = MinDraw;
        public static float ActivePowerDraw = 100f;
        public static int LightDelaySeconds = 5;

        // the minimum amount of power that something can draw
        // this allows it to respond when the PowerNet loses power
        public static readonly float MinDraw = 0.1f;

        /// <summary>
        /// Saves or loads the settings
        /// </summary>
        public override void ExposeData()
        {
            Scribe_Values.Look(ref ShowDebugMessages, "showDebugMessages", false);
            Scribe_Values.Look(ref FlickLights, "flickLights", true);
            Scribe_Values.Look(ref TurnOffLightsInBed, "turnOffLightsInBed", true);
            Scribe_Values.Look(ref AnimalsActivateLights, "animalsActivateLights", false);
            Scribe_Values.Look(ref StandbyPowerDraw, "standbyPowerDraw", MinDraw);
            Scribe_Values.Look(ref ActivePowerDraw, "activePowerDraw", 1f);
            Scribe_Values.Look(ref LightDelaySeconds, "lightDelaySeconds", 5);
            base.ExposeData();

            OnSettingsChanged?.Invoke();
        }

        /// <summary>
        /// Draws the settings window
        /// </summary>
        /// <param name="inRect">A Unity Rect with the size of the settings window</param>
        public void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            string standbyBuf = StandbyPowerDraw.ToString();
            string activeBuf = ActivePowerDraw.ToString();
            string delayBuf = LightDelaySeconds.ToString();

            listingStandard.CheckboxLabeled("Settings_ShowDebugMessages".Translate(), ref ShowDebugMessages, "Settings_ShowDebugMessagesTooltip".Translate());
            listingStandard.CheckboxLabeled("Settings_FlickLights".Translate(), ref FlickLights, "Settings_FlickLightsTooltip".Translate());
            listingStandard.CheckboxLabeled("Settings_TurnOffLightsInBed".Translate(), ref TurnOffLightsInBed, "Settings_TurnOffLightsInBedTooltip".Translate());
            listingStandard.CheckboxLabeled("Settings_AnimalsActivateLights".Translate(), ref AnimalsActivateLights, "Settings_AnimalsActivateLightsTooltip".Translate());
            listingStandard.TextFieldNumericLabeled("Settings_StandbyPowerDraw".Translate(), ref StandbyPowerDraw, ref standbyBuf, MinDraw, 100f);
            listingStandard.TextFieldNumericLabeled("Settings_ActivePowerDraw".Translate(), ref ActivePowerDraw, ref activeBuf, 100f, 1000f);
            listingStandard.TextFieldNumericLabeled("Settings_LightDelaySeconds".Translate(), ref LightDelaySeconds, ref delayBuf, 0);
            
            listingStandard.End();
        }

        /// <summary>
        /// Delegate to allow subscribers to know when settings change
        /// </summary>
        public delegate void OnSettingsChangedHandler();

        /// <summary>
        /// Event triggered whenever the settings are changed
        /// </summary>
        public static event OnSettingsChangedHandler OnSettingsChanged;
    }
}