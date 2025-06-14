using jl08lib.Logging;
using jl08lib.Settings.Exposed;
using jl08lib.Settings.IO;
using jl08lib.Settings.Management;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace jl08lib.Settings
{
    /// <summary>
    /// A base class for mod settings
    /// </summary>
    public class ModSettingsBase : ModSettings
    {
        /// <summary>
        /// Instantiates the mod settings base
        /// </summary>
        public ModSettingsBase()
        {
            StaticSettingEvents.OnLoadStaticSettings += ExposeStaticSettings;
        }

        /// <summary>
        /// Instantiates the mod settings with an overridden scribe
        /// </summary>
        /// <param name="overrideScribe">The scribe to use as an override</param>
        public ModSettingsBase(SettingScribeBase overrideScribe)
        {
            _overrideScribe = overrideScribe;
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
        /// Gets the setting scribe to use
        /// </summary>
        public SettingScribeBase SettingScribe => _overrideScribe ?? new XMLSettingScribe(Mod.Content, "StaticSettings");

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

            SettingScribeBase scribe = SettingScribe;
            foreach (ExposedSettingBase setting in exposedSettings)
            {
                setting?.ExposeData(scribe, Logger);
                Logger?.LogDebug($"{setting.Name}: {setting.Get<bool>()}");
            }
            // do not dispose of the override scribe since it has global scope
            if (scribe != _overrideScribe) { scribe.Dispose(); }
        }

        /// <summary>
        /// The scribe to use instead of creating one
        /// </summary>
        /// <remarks>This should only really be used for unit testing</remarks>
        private readonly SettingScribeBase _overrideScribe;
    }
}