using jl08lib.Settings.Attributes.Data;
using jl08lib.Settings.Exposed;
using System;

namespace jl08lib.Settings.Attributes
{
    public class BooleanSettingAttribute : SettingAttributeBase, IDefaultableSettingAttributeDate<bool>
    {
        public bool DefaultValue { get; set; }

        public override ExposedSettingBase GetExposedSetting(Type type, string fieldOrPropName)
        {
            return new ExposedBooleanSetting(type, fieldOrPropName, this);
        }
    }
}