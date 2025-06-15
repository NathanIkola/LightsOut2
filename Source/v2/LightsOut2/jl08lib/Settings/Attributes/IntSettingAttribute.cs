using jl08lib.Settings.Attributes.Data;
using jl08lib.Settings.Exposed;
using jl08lib.Translation;
using System;

namespace jl08lib.Settings.Attributes
{
    public class IntSettingAttribute : SettingAttributeBase, IDefaultableSettingAttributeData<int>
    {
        public IntSettingAttribute(string modPackageId)
            : this(modPackageId, null) { }

        public IntSettingAttribute(string modPackageId, IStringTranslator stringTranslator)
            : base(modPackageId, stringTranslator) { }

        public int DefaultValue { get; set; }

        /// <summary>
        /// The minimum value for this setting
        /// </summary>
        public int MinValue { get; set; } = int.MinValue;

        /// <summary>
        /// The maximum value for this setting
        /// </summary>
        public int MaxValue { get; set; } = int.MaxValue;

        public override SettingBase GetSetting(Type type, string memberName)
        {
            return new IntSetting(type, memberName, this);
        }
    }
}