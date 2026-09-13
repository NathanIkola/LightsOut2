using System;

namespace jl08lib.Settings.IO
{
    public abstract class SettingScribeBase : IDisposable
    {
        /// <summary>
        /// Clean up any state required by the scribe
        /// </summary>
        public virtual void Dispose() { }

        /// <summary>
        /// Handles saving/loading the value from the settings
        /// </summary>
        /// <typeparam name="TSettingType">The type of the setting</typeparam>
        /// <param name="value">The value of the setting</param>
        /// <param name="settingKey">The key of the setting</param>
        /// <param name="defaultValue">The default value for the setting</param>
        /// <param name="forceSave">If true, will force saving to the document even if set to the default value</param>
        public virtual void Look<TSettingType>(ref TSettingType value, string settingKey, TSettingType defaultValue = default, bool forceSave = false)
        {
            if (Saving)
            {
                Save(value, settingKey, defaultValue, forceSave);
            }
            else
            {
                value = Load(settingKey, defaultValue);
            }
        }

        /// <summary>
        /// Whether or not Scribe is in save mode
        /// </summary>
        public abstract bool Saving { get; }

        /// <summary>
        /// Saves the setting to the document
        /// </summary>
        /// <typeparam name="TSettingType">The type of the setting</typeparam>
        /// <param name="value">The value of the setting</param>
        /// <param name="settingKey">The key of the setting in the document</param>
        /// <param name="defaultValue">The default value to use</param>
        /// <param name="forceSave">If true, will force saving even if set to the default value</param>
        protected abstract void Save<TSettingType>(TSettingType value, string settingKey, TSettingType defaultValue, bool forceSave);

        /// <summary>
        /// Loads the setting from the document
        /// </summary>
        /// <typeparam name="TSettingType">The type for this setting</typeparam>
        /// <param name="settingKey">The key of this setting</param>
        /// <param name="defaultValue">The default value to return if there is no existing setting</param>
        /// <returns>The loaded value or the default if none has been set</returns>
        protected abstract TSettingType Load<TSettingType>(string settingKey, TSettingType defaultValue);
    }
}