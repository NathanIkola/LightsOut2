using jl08lib.Logging;
using jl08lib.Settings.Attributes;
using jl08lib.Settings.IO;
using System;
using UnityEngine;
using Verse;

namespace jl08lib.Settings.Exposed
{
    public class BooleanSetting : SettingBase
    {
        public BooleanSetting(Type type, string memberName, BooleanSettingAttribute attribute, LoggerBase logger)
            : base(type, memberName, attribute, logger)
        {
            _label = attribute.LabelLocalized;
            _tooltip = attribute.TooltipLocalized;
            _defaultValue = attribute.DefaultValue;
            Set(_defaultValue);
        }

        protected override void DrawSettingInner(Listing_Standard settingListing)
        {
            bool value = Get<bool>();
            settingListing.CheckboxLabeled(_label, ref value, _tooltip);
            Set(value);
        }

        public override void ExposeData(SettingScribeBase scribe, LoggerBase logger)
        {
            ExposeData(scribe, _defaultValue);
            logger.Trace($"Exposing boolean setting with key {Name} (value: {Get<bool>()})");
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