using System.IO;
using System.Xml.Linq;
using Verse;

namespace jl08lib.Settings.IO
{
    public class XMLSettingScribe : SettingScribeBase
    {
        /// <summary>
        /// Creates a setting scribe to interact with the settings
        /// </summary>
        /// <param name="content">The mod content</param>
        /// <param name="filenameSuffix">A suffix to add to the filename</param>
        public XMLSettingScribe(ModContentPack content, string filenameSuffix = null)
        {
            _filepath = GetPath(content, filenameSuffix);
            if (File.Exists(_filepath))
            {
                _document = XDocument.Load(_filepath);
            }
            else
            {
                _document = new XDocument();
            }
        }

        protected override void Save<TSettingType>(TSettingType value, string settingKey, TSettingType defaultValue, bool forceSave)
        {
            // ensure that the root element is there since we're saving
            if (_document.Root is null) { _document.Add(new XElement("SettingsRoot")); }
            XElement element = _document.Root.Element(settingKey);
            // if the value is equal to the default, just remove it entirely to keep sizes down
            if (!forceSave && value.Equals(defaultValue)) { element?.Remove(); }
            else 
            {
                // if the element didn't exist but should, create it
                if (element is null) 
                { 
                    element = new XElement(settingKey);
                    _document.Root.Add(element);
                }
                // update the value
                element.SetValue(value); 
            }
        }

        protected override TSettingType Load<TSettingType>(string settingKey, TSettingType defaultValue)
        {
            XElement element = _document.Root?.Element(settingKey);
            if (element is null) { return defaultValue; }
            return ParseHelper.FromString<TSettingType>(element.Value);
        }

        /// <summary>
        /// Saves the document
        /// </summary>
        public override void Dispose()
        {
            if (Saving && _document.Root != null)
            {
                _document.Save(_filepath);
            }
        }

        /// <summary>
        /// Gets the path for the mod
        /// </summary>
        /// <param name="content">The mod content</param>
        /// <param name="filenameSuffix">A suffix to add to the file</param>
        /// <returns>The filepath to use</returns>
        private static string GetPath(ModContentPack content, string filenameSuffix)
        {
            string folderName = content.FolderName;
            string modName = content.Name;
            string suffix = string.IsNullOrWhiteSpace(filenameSuffix) ? string.Empty : $"_{filenameSuffix}";
            return Path.Combine(GenFilePaths.ConfigFolderPath, GenText.SanitizeFilename($"Mod_{folderName}_{modName}{suffix}.xml"));
        }

        /// <summary>
        /// The filepath to the file
        /// </summary>
        private readonly string _filepath;

        /// <summary>
        /// The XML document
        /// </summary>
        private readonly XDocument _document;
    }
}