using jl08lib.Settings.Attributes.Data;
using jl08lib.Settings.Exposed;
using System;

namespace jl08lib.Settings.Attributes
{
    /// <summary>
    /// The base attribute to use for settings
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public abstract class SettingAttributeBase : Attribute, ISettingAttributeData
    {
        public string Label { get; set; }

        public string Tooltip { get; set; }

        public string ModPackageId { get; set; }

        public string SettingKey { get; set; }

        /// <summary>
        /// Retrieves the setting type
        /// </summary>
        /// <param name="type">The type that this setting is declared on</param>
        /// <param name="memberName">The name of the field/property that holds the setting value</param>
        /// <returns>Retrieves the settings for this attribute</returns>
        public abstract SettingBase GetSetting(Type type, string memberName);
    }
}