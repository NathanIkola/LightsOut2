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
        /// <param name="logger">The logger to use</param>
        public static void InvokeLoadStaticSettings(LoggerBase logger)
        {
            if (OnLoadStaticSettings is null || OnLoadStaticSettings.GetInvocationList().Length == 0)
            {
                logger?.LogTrace("Found no static settings to expose");
            }
            OnLoadStaticSettings?.Invoke();
        }
    }
}