using jl08lib.Logging;
using jl08lib.Settings.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace jl08lib.Settings.Management
{
    [StaticConstructorOnStartup]
    public static class StaticSettingController
    {
        /// <summary>
        /// Initializes the setting controller, locating all static settings that can be loaded
        /// </summary>
        static StaticSettingController()
        {
            _settings = new Dictionary<string, List<ExposedSettingBase>>();
            _logger = new VerseLogger("jl08lib");

            using (var section = _logger.OpenSection("Searching for auto-expose settings", LogLevel.Trace))
            {
                LocateAllSettings();
            }

            using (var section = _logger.OpenSection("Exposing discovered settings", LogLevel.Trace))
            {
                StaticSettingEvents.InvokeLoadStaticSettings(_logger);
            }
        }

        /// <summary>
        /// Retrieves the list of settings for the given mod packageId
        /// </summary>
        /// <param name="modPackageId">The packageId of the mod that should show the setting</param>
        /// <returns>The list of settings to show for this mod</returns>
        public static List<ExposedSettingBase> GetSettings(string modPackageId)
        {
            if (!_settings.ContainsKey(modPackageId))
            {
                return new List<ExposedSettingBase>();
            }
            return _settings[modPackageId];
        }

        #region private helper methods
        /// <summary>
        /// Looks at all loaded mods to find any attributed settings
        /// </summary>
        private static void LocateAllSettings()
        {
            // get the list of attributes to search for on types
            List<Type> attributeTypes = AttributeTypes();
            foreach (ModContentPack mod in LoadedModManager.RunningModsListForReading)
            {
                foreach (Assembly asm in mod.assemblies.loadedAssemblies)
                {
                    foreach (Type type in asm.GetTypes())
                    {
                        LocateAllSettingsOnType(type, attributeTypes);
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves the list of attributes to check for on types
        /// </summary>
        /// <returns>The list of attributes to check for</returns>
        private static List<Type> AttributeTypes()
        {
            Assembly asm = typeof(StaticSettingController).Assembly;
            Type baseAttribute = typeof(ExposeSettingAttribute);
            List<Type> attributes = new List<Type>();
            foreach (Type type in asm.GetTypes())
            {
                // skip the base attribute type or classes that aren't subclasses of it
                if (!type.IsSubclassOf(baseAttribute) || type == baseAttribute) { continue; }
                attributes.Add(type);
            }
            return attributes;
        }

        /// <summary>
        /// Searches the given type for attributed settings
        /// </summary>
        /// <param name="type">The type to load the settings from</param>
        /// <param name="attributeTypes">The list of attribute types to search for</param>
        private static void LocateAllSettingsOnType(Type type, List<Type> attributeTypes)
        {
            // first look at each field
            foreach (FieldInfo fieldInfo in type.GetFields())
            {
                foreach (Type attributeType in attributeTypes)
                {
                    if (fieldInfo.GetCustomAttribute(attributeType) is ExposeSettingAttribute settingAttr)
                    {
                        ExposedSettingBase settingbase = settingAttr.GetSettingBase(type, fieldInfo.Name);
                        AddSetting(settingAttr.ModPackageId, settingbase);
                        break;
                    }
                }
            }

            // now look at each property
            foreach (PropertyInfo propertyInfo in type.GetProperties())
            {
                foreach (Type attributeType in attributeTypes)
                {
                    if (propertyInfo.GetCustomAttribute(attributeType) is ExposeSettingAttribute settingAttr)
                    {
                        ExposedSettingBase settingBase = settingAttr.GetSettingBase(type, propertyInfo.Name);
                        AddSetting(settingAttr.ModPackageId, settingBase);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Adds a setting for the specified mod
        /// </summary>
        /// <param name="modPackageId">The packageId of the mod that the setting should be shown for</param>
        /// <param name="setting">The setting to show</param>
        private static void AddSetting(string modPackageId, ExposedSettingBase setting)
        {
            _logger.LogTrace($"Found setting '{setting.Name}' for mod '{modPackageId}'");
            if (!_settings.ContainsKey(modPackageId))
            {
                _settings.Add(modPackageId, new List<ExposedSettingBase>());
            }
            _settings[modPackageId].Add(setting);
        }
        #endregion

        /// <summary>
        /// The dictionary of settings keyed by the mod packageId
        /// </summary>
        private static readonly Dictionary<string, List<ExposedSettingBase>> _settings;

        /// <summary>
        /// The logger to use when logging
        /// </summary>
        private static readonly LoggerBase _logger;
    }
}