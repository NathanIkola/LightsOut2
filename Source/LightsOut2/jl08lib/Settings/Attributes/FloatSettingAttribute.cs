using jl08lib.Logging;
using jl08lib.Settings.Attributes.Data;
using jl08lib.Settings.Exposed;
using jl08lib.Translation;
using System;

namespace jl08lib.Settings.Attributes
{
    public class FloatSettingAttribute : SettingAttributeBase, IDefaultableSettingAttributeData<float>
    {
        public FloatSettingAttribute(string modPackageId)
            : this(modPackageId, null) { }

        public FloatSettingAttribute(string modPackageId, IStringTranslator stringTranslator)
            : base(modPackageId, stringTranslator) { }

        public float DefaultValue { get; set; }

        /// <summary>
        /// The minimum value for this setting
        /// </summary>
        public float MinValue { get; set; } = float.MinValue;

        /// <summary>
        /// The maximum value for this setting
        /// </summary>
        public float MaxValue { get; set; } = float.MaxValue;

        public override SettingBase GetSetting(Type type, string memberName, LoggerBase logger)
        {
            return new FloatSetting(type, memberName, this, logger);
        }
    }
}