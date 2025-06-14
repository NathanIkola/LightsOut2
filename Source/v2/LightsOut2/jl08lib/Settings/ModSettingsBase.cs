using jl08lib.Logging;
using jl08lib.Settings.IO;
using jl08lib.Settings.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace jl08lib.Settings
{
    public class ModSettingsBase : ModSettings
    {
        public ModSettingsBase()
        {
            StaticSettingEvents.OnLoadStaticSettings += ExposeStaticSettings;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            if (Mod?.Content?.PackageId != null)
            {
                ExposeStaticSettings();
            }
        }

        /// <summary>
        /// A logger to use
        /// </summary>
        public LoggerBase Logger => (Mod as ModBase)?.Logger;

        /// <summary>
        /// The package ID of this mod
        /// </summary>
        public string ModPackageId => Mod.Content.PackageId.ToLower();

        /// <summary>
        /// Drawsthe settings menu
        /// </summary>
        /// <param name="inRect">The rectangle to draw the settings into</param>
        public void DoSettingsWindowContent(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);

            List<ExposedSettingBase> exposedSettings = StaticSettingController.GetSettings(ModPackageId);
            foreach (ExposedSettingBase setting in exposedSettings)
            {
                setting?.DrawSetting(listing);
            }

            listing.End();
        }

        /// <summary>
        /// Exposes the static settings
        /// </summary>
        private void ExposeStaticSettings()
        {
            List<ExposedSettingBase> exposedSettings = StaticSettingController.GetSettings(ModPackageId);
            Logger?.LogDebug($"Found {exposedSettings?.Count ?? 0} settings");
            using (SettingScribeBase scribe = new XMLSettingScribe(Mod.Content, "StaticSettings"))
            {
                foreach (ExposedSettingBase setting in exposedSettings)
                {
                    setting?.ExposeData(scribe, Logger);
                    Logger?.LogDebug($"{setting.Name}: {setting.Get<bool>()}");
                }
            }
        }
    }
}