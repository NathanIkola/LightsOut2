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
        public static float ActivePowerDraw = 1f;
        public static int LightDelaySeconds = 5;

        // the minimum amount of power that something can draw
        // this allows it to respond when the PowerNet loses power
        public static readonly float MinDraw = 0.001f;

        /// <summary>
        /// Saves or loads the settings
        /// </summary>
        public override void ExposeData()
        {
            Scribe_Values.Look(ref ShowDebugMessages, "showDebugMessages");
            base.ExposeData();

            OnSettingsChanged?.Invoke();
        }

        /// <summary>
        /// Draws the settings window
        /// </summary>
        /// <param name="inRect">A Unity Rect with the size of the settings window</param>
        public void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingSandard = new Listing_Standard();
            listingSandard.Begin(inRect);

            listingSandard.CheckboxLabeled("Settings_ShowDebugMessages".Translate(), ref ShowDebugMessages, "Settings_ShowDebugMessagesTooltip".Translate());
            
            listingSandard.End();
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