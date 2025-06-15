using jl08lib.Settings.Attributes.Data;
using jl08lib.Settings.Exposed;
using jl08lib.Translation;
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
            : this(modPackageId, null) { }

        /// <summary>
        /// Instantiates a new setting attribute with a string translator
        /// </summary>
        /// <param name="modPackageId">The package ID of the mod</param>
        /// <param name="stringTranslator">The string translator</param>
        public SettingAttributeBase(string modPackageId, IStringTranslator stringTranslator)
        {
            ModPackageId = modPackageId;
            _stringTranslator = stringTranslator;
        }

        public string Label { get; set; }

        public string Tooltip { get; set; }

        public string ModPackageId { get; set; }

        public string SettingKey { get; set; }

        public bool TranslateStrings { get; set; }

        /// <summary>
        /// A localized label for display
        /// </summary>
        public string LabelLocalized => TranslateString(Label);

        /// <summary>
        /// A localized tooltip for display
        /// </summary>
        public string TooltipLocalized => TranslateString(Tooltip);

        /// <summary>
        /// Retrieves the setting type
        /// </summary>
        /// <param name="type">The type that this setting is declared on</param>
        /// <param name="memberName">The name of the field/property that holds the setting value</param>
        /// <returns>Retrieves the settings for this attribute</returns>
        public abstract SettingBase GetSetting(Type type, string memberName);

        /// <summary>
        /// Translates the given string using the translator
        /// </summary>
        /// <param name="toTranslate">The string to translate</param>
        /// <returns>The translated string</returns>
        protected string TranslateString(string toTranslate)
        {
            if (TranslateStrings && _stringTranslator is null)
            {
                _stringTranslator = new VerseStringTranslator();
            }
            return _stringTranslator?.Translate(toTranslate) ?? toTranslate;
        }

        /// <summary>
        /// The translator to use to get translated strings for settings
        /// </summary>
        private IStringTranslator _stringTranslator;
    }
}