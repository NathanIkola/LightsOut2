using jl08lib.Settings.Attributes.Data;
using jl08lib.Settings.Exposed;
using System;

namespace jl08lib.Settings.Attributes
{
    public class BooleanSettingAttribute : SettingAttributeBase, IDefaultableSettingAttributeData<bool>
    {
        public bool DefaultValue { get; set; }

        public override SettingBase GetSetting(Type type, string memberName)
        {
            return new BooleanSetting(type, memberName, this);
        }
    }
}