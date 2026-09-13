using jl08lib.Settings.IO;
using System.Collections.Generic;

namespace jl08lib.Tests.Mocks
{
    internal class MockSettingScribe : SettingScribeBase
    {
        public override bool Saving => SimulateSaving;

        protected override TSettingType Load<TSettingType>(string settingKey, TSettingType defaultValue)
        {
            if (_settingStore.ContainsKey(settingKey))
            {
                return (TSettingType)_settingStore[settingKey];
            }
            return defaultValue;
        }

        protected override void Save<TSettingType>(TSettingType value, string settingKey, TSettingType defaultValue, bool forceSave)
        {
            if (_settingStore.ContainsKey(settingKey))
            {
                _settingStore[settingKey] = value;
            }
            else
            {
                _settingStore.Add(settingKey, value);
            }
        }

        public override void Look<TSettingType>(ref TSettingType value, string settingKey, TSettingType defaultValue = default, bool forceSave = false)
        {
            if (SimulateSaving)
            {
                Save(value, settingKey, defaultValue, forceSave);
            }
            else
            {
                value = Load(settingKey, defaultValue);
            }
        }

        /// <summary>
        /// Whether or not to simulate saving settings
        /// </summary>
        public bool SimulateSaving { get; set; } = false;

        /// <summary>
        /// The dictionary to store the settings values in
        /// </summary>
        private readonly Dictionary<string, object> _settingStore = new Dictionary<string, object>();
    }
}