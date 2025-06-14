using jl08lib.Logging;
using jl08lib.Settings.IO;
using System;
using Verse;

namespace jl08lib.Settings.Exposed
{
    public class ExposedBooleanSetting : ExposedSettingBase
    {
        public ExposedBooleanSetting(Type type, string fieldOrPropName, string settingKey, string label, string tooltip)
            : base(type, fieldOrPropName, settingKey)
        {
            _label = label;
            _tooltip = tooltip;
        }

        public override void DrawSetting(Listing_Standard settingListing)
        {
            bool value = Get<bool>();
            settingListing.CheckboxLabeled(_label, ref value, _tooltip);
            Set(value);
        }

        public override void ExposeData(SettingScribeBase scribe, LoggerBase _logger)
        {
            ExposeData<bool>(scribe);
        }

        /// <summary>
        /// The label to use when drawing the setting
        /// </summary>
        private readonly string _label;

        /// <summary>
        /// The tooltip to show when hovering over this setting
        /// </summary>
        private readonly string _tooltip;
    }
}