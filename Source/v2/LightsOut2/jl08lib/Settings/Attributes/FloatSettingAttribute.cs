using jl08lib.Settings.Attributes.Data;
using jl08lib.Settings.Exposed;
using System;

namespace jl08lib.Settings.Attributes
{
    public class FloatSettingAttribute : SettingAttributeBase, IDefaultableSettingAttributeData<float>
    {
        public float DefaultValue { get; set; }

        /// <summary>
        /// The minimum value for this setting
        /// </summary>
        public float MinValue { get; set; } = float.MinValue;

        /// <summary>
        /// The maximum value for this setting
        /// </summary>
        public float MaxValue { get; set; } = float.MaxValue;

        public override SettingBase GetSetting(Type type, string memberName)
        {
            return new FloatSetting(type, memberName, this);
        }
    }
}