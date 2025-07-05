using jl08lib.Logging;
using jl08lib.Settings.Attributes;
using System;
using Verse;

namespace jl08lib.Settings.Exposed
{
    public class BooleanSetting : SettingBase<bool>
    {
        public BooleanSetting(Type type, string memberName, BooleanSettingAttribute attribute, LoggerBase logger)
            : base(type, memberName, attribute, logger, attribute.DefaultValue)
        {
            _label = attribute.LabelLocalized;
            _tooltip = attribute.TooltipLocalized;
        }

        protected override void DrawSettingInner(Listing_Standard settingListing)
        {
            base.DrawSettingInner(settingListing);

            bool value = Get();
            settingListing.CheckboxLabeled(_label, ref value, _tooltip);
            Set(value);
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