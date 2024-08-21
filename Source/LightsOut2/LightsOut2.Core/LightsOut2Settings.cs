using HarmonyLib;
using LightsOut2.Core.ModCompatibility;
using UnityEngine;
using Verse;

namespace LightsOut2.Core
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

        /// <summary>
        /// Saves or loads the settings
        /// </summary>
        public override void ExposeData()
        {
            Scribe_Values.Look(ref ShowDebugMessages, "showDebugMessages", false);
            OnSettingsExposeData?.Invoke();
            base.ExposeData();
        }

        /// <summary>
        /// Draws the settings window
        /// </summary>
        /// <param name="inRect">A Unity Rect with the size of the settings window</param>
        public void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            listingStandard.CheckboxLabeled("Settings_ShowDebugMessages".Translate(), ref ShowDebugMessages, "Settings_ShowDebugMessagesTooltip".Translate());
            OnSettingsRendered?.Invoke(listingStandard);

            listingStandard.End();
        }

        /// <summary>
        /// Delegate to allow subscribers to show custom settings
        /// </summary>
        /// <param name="settingListing">The settings listing to add settings to</param>
        public delegate void OnSettingsRenderedHandler(Listing_Standard settingListing);

        /// <summary>
        /// The event fired when the settings window is being rendered
        /// </summary>
        public static event OnSettingsRenderedHandler OnSettingsRendered;

        /// <summary>
        /// Delegate allowing subscribers to load and save settings data
        /// </summary>
        public delegate void OnSettingsExposeDataHandler();

        /// <summary>
        /// The event fired when loading or saving settings data
        /// </summary>
        public static event OnSettingsExposeDataHandler OnSettingsExposeData;
    }
}