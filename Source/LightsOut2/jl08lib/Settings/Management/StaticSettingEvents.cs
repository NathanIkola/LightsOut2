using jl08lib.Logging;

namespace jl08lib.Settings.Management
{
    public static class StaticSettingEvents
    {
        /// <summary>
        /// The delegate for loading static settings
        /// </summary>
        public delegate void LoadStaticSettingsHandler();

        /// <summary>
        /// The event invoked when static settings are ready to load
        /// </summary>
        public static event LoadStaticSettingsHandler OnLoadStaticSettings;

        /// <summary>
        /// Load the static settings from XML
        /// </summary>
        public static void InvokeLoadStaticSettings()
        {
            OnLoadStaticSettings?.Invoke();
        }
    }
}