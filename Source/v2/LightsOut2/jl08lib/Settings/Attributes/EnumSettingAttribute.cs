using jl08lib.Logging;
using jl08lib.Settings.Attributes.Data;
using jl08lib.Settings.Exposed;
using jl08lib.Translation;
using System;

namespace jl08lib.Settings.Attributes
{
    /// <summary>
    /// A class that allows showing a select list based on a set of enum values
    /// </summary>
    public class EnumSettingAttribute : SettingAttributeBase, IDefaultableSettingAttributeData<int>
    {
        public EnumSettingAttribute(string modPackageId)
            : base(modPackageId, null) { }

        public EnumSettingAttribute(string modPackageId, IStringTranslator stringTranslator)
            : base(modPackageId, stringTranslator) { }

        public int DefaultValue { get; set; }

        public override SettingBase GetSetting(Type type, string memberName, LoggerBase logger)
        {
            return new EnumSetting(type, memberName, this, logger);
        }
    }

    /// <summary>
    /// A class that allows specifying strings for enum values when displaying them
    /// </summary>
    public class EnumOptionAttribute : Attribute
    {
        public EnumOptionAttribute() { }

        /// <summary>
        /// The caption to show when displaying this setting
        /// </summary>
        public string Caption { get; set; }

        /// <summary>
        /// The caption, translated if necessary
        /// </summary>
        public string CaptionLocalized => TranslateString(Caption);


        /// <summary>
        /// The description of this option
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The description, translated if necessary
        /// </summary>
        public string DescriptionLocalized => TranslateString(Description);

        /// <summary>
        /// Whether or not to translate the strings
        /// </summary>
        public bool TranslateStrings { get; set; }

        /// <summary>
        /// The translator to use for the strings
        /// </summary>
        private IStringTranslator _stringTranslator;

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
    }
}
