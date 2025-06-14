using jl08lib.Settings.Exposed;
using System;

namespace jl08lib.Settings.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public abstract class ExposeSettingAttribute : Attribute
    {
        /// <summary>
        /// The package ID for the mod (same as the packageId node in About.xml)
        /// </summary>
        public string ModPackageId { get; set; }

        /// <summary>
        /// The key for this setting
        /// </summary>
        public string SettingKey { get; set; }

        /// <summary>
        /// Retrieves the setting type
        /// </summary>
        /// <param name="type">The type that this setting is declared on</param>
        /// <param name="fieldOrPropName">The name of the field/property that holds the setting value</param>
        /// <returns>Retrieves the settings for this attribute</returns>
        public abstract ExposedSettingBase GetSettingBase(Type type, string fieldOrPropName);
    }
}