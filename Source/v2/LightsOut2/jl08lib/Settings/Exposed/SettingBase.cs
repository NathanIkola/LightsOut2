using jl08lib.Logging;
using jl08lib.Settings.Attributes;
using jl08lib.Settings.IO;
using System;
using System.Reflection;
using Verse;

namespace jl08lib.Settings.Exposed
{
    public abstract class SettingBase
    {
        /// <summary>
        /// The key used for this specific setting
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The name of the member this setting corresponds to on the target type
        /// </summary>
        public readonly string MemberName;

        /// <summary>
        /// Instantiates the exposed setting base object
        /// </summary>
        /// <param name="type">The type that has the setting being exposed</param>
        /// <param name="fieldOrPropName">The name of the property on the given type that holds the setting</param>
        /// <param name="attribute">The attribute initializing this setting</param>
        public SettingBase(Type type, string fieldOrPropName, SettingAttributeBase attribute)
        {
            Name = attribute.SettingKey;
            MemberName = fieldOrPropName;

            if (type.GetField(fieldOrPropName, Flags) is FieldInfo field)
            {
                _field = field;
            }
            else if (type.GetProperty(fieldOrPropName, Flags) is PropertyInfo property)
            {
                _property = property;
            }
        }

        /// <summary>
        /// Performs the exposing of the data in the setting
        /// </summary>
        /// <param name="scribe">The setting scribe to use</param>
        /// <param name="logger">The logger to use</param>
        /// <remarks>
        /// Should call ExposeData using the concrete type for TSettingType
        /// </remarks>
        public abstract void ExposeData(SettingScribeBase scribe, LoggerBase logger);

        /// <summary>
        /// Expose the setting with the specified type
        /// </summary>
        /// <param name="scribe">The scribe to use</param>
        /// <param name="defaultValue">The default value to use</param>
        protected void ExposeData<TSettingType>(SettingScribeBase scribe, TSettingType defaultValue)
        {
            TSettingType settingValue = Get<TSettingType>();
            string key = $"ExposedSetting.{Name}".Replace(" ", string.Empty);
            scribe.Look(ref settingValue, key, settingValue, true);
            Set(settingValue);
        }

        /// <summary>
        /// Renders the setting in the settings menu
        /// </summary>
        /// <param name="settingListing">The listing to add it to</param>
        public abstract void DrawSetting(Listing_Standard settingListing);

        /// <summary>
        /// Retrieves the value of the field or property
        /// </summary>
        /// <returns>The value in the setting, or the default if not valid</returns>
        internal TSettingType Get<TSettingType>()
        {
            if (_field != null)
            {
                return (TSettingType)_field.GetValue(null);
            }

            if (_property != null)
            {
                return (TSettingType)_property.GetValue(null);
            }
            return default;
        }

        /// <summary>
        /// Sets the value into the field or property
        /// </summary>
        /// <param name="value">The value to set</param>
        internal void Set<TSettingType>(TSettingType value)
        {
            if (_field != null)
            {
                _field.SetValue(null, value);
            }
            else if (_property != null)
            {
                _property.SetValue(null, value);
            }
        }

        /// <summary>
        /// The field to get/set
        /// </summary>
        private readonly FieldInfo _field;

        /// <summary>
        /// The property to get/set
        /// </summary>
        private readonly PropertyInfo _property;

        /// <summary>
        /// The flags to use when attempting to retrieve a member via reflection
        /// </summary>
        /// <remarks>
        /// Only considers static fields/properties
        /// </remarks>
        private const BindingFlags Flags = BindingFlags.Public
            | BindingFlags.Static
            | BindingFlags.NonPublic;
    }
}