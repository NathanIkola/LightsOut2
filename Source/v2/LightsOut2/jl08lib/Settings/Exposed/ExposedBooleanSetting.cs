using jl08lib.Logging;
using jl08lib.Settings.Attributes;
using jl08lib.Settings.IO;
using System;
using Verse;

namespace jl08lib.Settings.Exposed
{
    public class ExposedBooleanSetting : ExposedSettingBase
    {
        public ExposedBooleanSetting(Type type, string fieldOrPropName, BooleanSettingAttribute attribute)
            : base(type, fieldOrPropName, attribute.SettingKey)
        {
            _label = attribute.Label;
            _tooltip = attribute.Tooltip;
            _defaultValue = attribute.DefaultValue;
        }

        public override void DrawSetting(Listing_Standard settingListing)
        {
            bool value = Get<bool>();
            settingListing.CheckboxLabeled(_label, ref value, _tooltip);
            Set(value);
        }

        public override void ExposeData(SettingScribeBase scribe, LoggerBase _logger)
        {
            ExposeData<bool>(scribe, _defaultValue);
        }

        /// <summary>
        /// The label to use when drawing the setting
        /// </summary>
        private readonly string _label;

        /// <summary>
        /// The tooltip to show when hovering over this setting
        /// </summary>
        private readonly string _tooltip;

        /// <summary>
        /// The default value for this setting
        /// </summary>
        private readonly bool _defaultValue;
    }
}