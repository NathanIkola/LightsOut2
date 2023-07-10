using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace LightsOut2
{
    public class LightsOut2Settings : ModSettings
    {
        /// <summary>
        /// Whether or not to print debug messages to the console
        /// </summary>
        public static bool ShowDebugMessages;

        /// <summary>
        /// Saves or loads the settings
        /// </summary>
        public override void ExposeData()
        {
            Scribe_Values.Look(ref ShowDebugMessages, "showDebugMessages");

            base.ExposeData();
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
    }
}
