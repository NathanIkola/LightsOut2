using System;

namespace jl08lib.Settings.Attributes
{
    public class BooleanSettingAttribute : ExposeSettingAttribute
    {
        /// <summary>
        /// The label to show when drawing this setting in the menu
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// The tooltip to show when hovering over this setting
        /// </summary>
        public string Tooltip { get; set; }

        public override ExposedSettingBase GetSettingBase(Type type, string fieldOrPropName)
        {
            return new ExposedBooleanSetting(type, fieldOrPropName, SettingKey, Label, Tooltip);
        }
    }
}