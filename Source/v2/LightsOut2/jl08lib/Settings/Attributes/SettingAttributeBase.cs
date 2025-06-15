using jl08lib.Settings.Attributes.Data;
using jl08lib.Settings.Exposed;
using System;
using Verse;

namespace jl08lib.Settings.Attributes
{
    /// <summary>
    /// The base attribute to use for settings
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public abstract class SettingAttributeBase : Attribute, ISettingAttributeData
    {
        /// <summary>
        /// Constructs a new setting attribute
        /// </summary>
        /// <param name="modPackageId">The package ID of the mod</param>
        public SettingAttributeBase(string modPackageId)
        {
            ModPackageId = modPackageId;
        }

        public string Label { get; set; }

        public string Tooltip { get; set; }

        public string ModPackageId { get; set; }

        public string SettingKey { get; set; }

        public bool TranslateStrings { get; set; } = true;

        /// <summary>
        /// Returns the appropriate string for the input, translating it if necessary
        /// </summary>
        /// <param name="input">The string to retrieve</param>
        /// <returns>The string, translated if necessary</returns>
        public string GetString(string input)
        {
            if (TranslateStrings)
            {
                return input.Translate();
            }
            return input;
        }

        /// <summary>
        /// Retrieves the setting type
        /// </summary>
        /// <param name="type">The type that this setting is declared on</param>
        /// <param name="memberName">The name of the field/property that holds the setting value</param>
        /// <returns>Retrieves the settings for this attribute</returns>
        public abstract SettingBase GetSetting(Type type, string memberName);
    }
}