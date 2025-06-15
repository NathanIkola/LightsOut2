using jl08lib.Logging;
using jl08lib.Settings.Attributes.Data;
using jl08lib.Settings.Exposed;
using jl08lib.Translation;
using System;

namespace jl08lib.Settings.Attributes
{
    public class BooleanSettingAttribute : SettingAttributeBase, IDefaultableSettingAttributeData<bool>
    {
        public BooleanSettingAttribute(string modPackageId)
            : this(modPackageId, null) { }

        public BooleanSettingAttribute(string modPackageId, IStringTranslator stringTranslator)
            : base(modPackageId, stringTranslator) { }

        public bool DefaultValue { get; set; }

        public override SettingBase GetSetting(Type type, string memberName, LoggerBase logger)
        {
            return new BooleanSetting(type, memberName, this, logger);
        }
    }
}