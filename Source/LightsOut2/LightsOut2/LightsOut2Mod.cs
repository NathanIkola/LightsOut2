using HarmonyLib;
using UnityEngine;
using Verse;
using LightsOut2.Core;
using LightsOut2.Core.ModCompatibility;
using System.Reflection;

namespace LightsOut2
{
    /// <summary>
    /// The class responsible for initializing LightsOut2
    /// </summary>
    public class LightsOut2Mod : Mod
    {
        /// <summary>
        /// The currently loaded settings for the mod
        /// </summary>
        public LightsOut2Settings Settings { get; set; }

        /// <summary>
        /// Initializes the mod and loads the settings
        /// </summary>
        /// <param name="content"></param>
        public LightsOut2Mod(ModContentPack content)
            : base(content)
        {
            LightsOut2Settings.OnSettingsExposeData += OnSettingsExposeDataHandler;
            LightsOut2Settings.OnSettingsRendered += OnSettingsRenderedHandler;
            // this call loads the settings
            Settings = GetSettings<LightsOut2Settings>();
            // this string is hardcoded because the translation engine isn't loaded at this point
            // this also doesn't use the debug logger because it isn't a debug message and should always print
            Log.Message($"Initializing LightsOut 2 [{typeof(LightsOut2Mod).Assembly.GetName().Version}]");
            HarmonyInstance = new Harmony("LightsOut2");
            foreach (Assembly asm in content.assemblies.loadedAssemblies)
                HarmonyInstance.PatchAll(asm);
        }

        /// <summary>
        /// The Harmony instance used by the mod
        /// </summary>
        public static Harmony HarmonyInstance { get; private set; }

        /// <summary>
        /// Draws the settings window
        /// </summary>
        /// <param name="inRect">A Unity Rect with the size of the settings window</param>
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Settings.DoSettingsWindowContents(inRect);
            base.DoSettingsWindowContents(inRect);
        }

        /// <summary>
        /// The category that shows in the Mod Settings page
        /// </summary>
        /// <returns>The name of the mod</returns>
        public override string SettingsCategory()
        {
            return "ModName".Translate();
        }

        public static bool FlickLights = true;
        public static bool TurnOffLightsInBed = true;
        public static bool AnimalsActivateLights = false;
        private static float StandbyPowerDraw = MinDraw;
        public static float StandbyPowerDrawDecimal = StandbyPowerDraw / 100f;
        private static float ActivePowerDraw = 100f;
        public static float ActivePowerDrawDecimal = ActivePowerDraw / 100f;
        public static int LightDelaySeconds = 3;

        // the minimum amount of power that something can draw
        // this allows it to respond when the PowerNet loses power
        private static readonly float MinDraw = 0.1f;
        public static readonly float MinDrawDecimal = MinDraw / 100f;

        /// <summary>
        /// Renders the settings for the mod
        /// </summary>
        /// <param name="settingListing">The setting listing to add to</param>
        private void OnSettingsRenderedHandler(Listing_Standard settingListing)
        {

            string standbyBuf = StandbyPowerDraw.ToString();
            string activeBuf = ActivePowerDraw.ToString();
            string delayBuf = LightDelaySeconds.ToString();

            settingListing.CheckboxLabeled("Settings_FlickLights".Translate(), ref FlickLights, "Settings_FlickLightsTooltip".Translate());
            settingListing.CheckboxLabeled("Settings_TurnOffLightsInBed".Translate(), ref TurnOffLightsInBed, "Settings_TurnOffLightsInBedTooltip".Translate());
            settingListing.CheckboxLabeled("Settings_AnimalsActivateLights".Translate(), ref AnimalsActivateLights, "Settings_AnimalsActivateLightsTooltip".Translate());
            settingListing.TextFieldNumericLabeled("Settings_StandbyPowerDraw".Translate(), ref StandbyPowerDraw, ref standbyBuf, MinDraw, 100f);
            settingListing.TextFieldNumericLabeled("Settings_ActivePowerDraw".Translate(), ref ActivePowerDraw, ref activeBuf, 100f, 1000f);
            settingListing.TextFieldNumericLabeled("Settings_LightDelaySeconds".Translate(), ref LightDelaySeconds, ref delayBuf, 0);
        }

        private void OnSettingsExposeDataHandler()
        {
            Scribe_Values.Look(ref FlickLights, "flickLights", true);
            Scribe_Values.Look(ref TurnOffLightsInBed, "turnOffLightsInBed", true);
            Scribe_Values.Look(ref AnimalsActivateLights, "animalsActivateLights", false);
            Scribe_Values.Look(ref StandbyPowerDraw, "standbyPowerDraw", MinDraw);
            Scribe_Values.Look(ref ActivePowerDraw, "activePowerDraw", 1f);
            Scribe_Values.Look(ref LightDelaySeconds, "lightDelaySeconds", 5);

            // calculate the decimal values to avoid unnecessary floating point divisions
            StandbyPowerDrawDecimal = StandbyPowerDraw / 100f;
            ActivePowerDrawDecimal = ActivePowerDraw / 100f;
        }
    }
}
