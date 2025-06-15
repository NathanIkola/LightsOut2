using jl08lib.Settings;
using jl08lib.Settings.Attributes;

namespace LightsOut2
{
    /// <summary>
    /// The settings used in LightsOut 2
    /// </summary>
    public class LightsOut2Settings : ModSettingsBase
    {
        /// <summary>
        /// Whether or not to turn off lights in empty rooms
        /// </summary>
        [BooleanSetting(PackageId,
            DefaultValue = true,
            Label = "Turn off lights in empty rooms",
            Tooltip = "If enabled, lights in empty rooms will be turned off to save power")]
        public static bool FlickLights;

        /// <summary>
        /// Whether or not to keep lights on when pawns are sleeping
        /// </summary>
        [BooleanSetting(PackageId,
            DefaultValue = false,
            Label = "Keep lights on when pawns are sleeping",
            Tooltip = "If enabled, lights will stay on when all pawns in the room are sleeping")]
        public static bool NightLights;

        /// <summary>
        /// Whether or not animals should be flicking lights
        /// </summary>
        [BooleanSetting(PackageId,
            DefaultValue = false,
            Label = "Allow animals to turn on lights",
            Tooltip = "If enabled, animals will be treated like normal pawns when activating lights",
            ShowInSettingsDelegateName = nameof(FlickingLightsEnabled))]
        public static bool AnimalsFlickLights;

        /// <summary>
        /// The coefficient (in percent) of power devices use in standby mode
        /// </summary>
        /// <remarks>
        /// This does not affect lights, which always use 0% when in standby
        /// </remarks>
        [IntSetting(PackageId,
            DefaultValue = 0,
            MinValue = 0,
            MaxValue = 100,
            Label = "Standby energy draw rate (%)",
            Tooltip = "The energy draw (in percent) that buildings should have when in standby")]
        public static int StandbyCoefficientPercent;

        /// <summary>
        /// The coefficienc (in decimal) of power devices use in standby mode
        /// </summary>
        public static float StandbyCoefficientDecimal => StandbyCoefficientPercent / 100f;

        /// <summary>
        /// The coefficient (in percent) of power devices use when active
        /// </summary>
        /// <remarks>
        /// This does not affect lights, which always use 100% when active
        /// </remarks>
        [IntSetting(PackageId,
            DefaultValue = 0,
            MinValue = 100,
            Label = "Active energy draw rate (%)",
            Tooltip = "The energy draw (in percent) that buildings should have when in use")]
        public static int ActiveCoefficientPercent;

        /// <summary>
        /// The coefficient (in decimal) of power devices use when active
        /// </summary>
        public static float ActiveCoefficientDecimal => ActiveCoefficientPercent / 100f;

        /// <summary>
        /// The amount of time (in seconds) to wait before turning off lights
        /// </summary>
        [FloatSetting(PackageId,
            DefaultValue = 1.5f,
            MinValue = 0f,
            Label = "Seconds to delay turning off lights",
            Tooltip = "The number of seconds to wait before turning off the lights (to combat rapid flickering at higher game speeds)",
            ShowInSettingsDelegateName = nameof(FlickingLightsEnabled))]
        public static float LightDelaySeconds;

        /// <summary>
        /// The minimum log level to show in the logs
        /// </summary>
        [IntSetting(PackageId,
            DefaultValue = 3,
            MinValue = 0,
            MaxValue = 6,
            Label = "Log message minimum severity",
            Tooltip = "Only logs messages with the specified severity or higher. 0-Tracing, 1-Debug, 2-Info, 3-Warning, 4-Error, 5-Critical, 6-None")]
        public static int MinimumLogLevel;

        /// <summary>
        /// Whether or not the mod should perform internal integrity checking
        /// </summary>
        [BooleanSetting(PackageId,
            DefaultValue = false,
            Label = "Enable internal integrity checks",
            Tooltip = "Allow the mod to perform checks to verify data integrity in its core data. This may have a performance impact and should only be used for debugging purposes.")]
        public static bool EnableIntegrityChecks;

        /// <summary>
        /// The number of frames to wait between integrity checks
        /// </summary>
        [IntSetting(PackageId,
            DefaultValue = 1800,
            MinValue = 1,
            Label = "Frames between integrity checks",
            Tooltip = "The number of frames to wait between integrity checks. A lower value will increase the frequency of checks, which may impact performance.",
            ShowInSettingsDelegateName = nameof(IntegrityChecksEnabled))]
        public static int FramesBetweenIntegrityChecks;

        /// <summary>
        /// The minimum percentage that a building must draw
        /// </summary>
        public const float MinDrawCoefficientPercent = 0.1f;

        /// <summary>
        /// The minimum decimal value that a building must draw
        /// </summary>
        public const float MinDrawCoefficientDecimal = MinDrawCoefficientPercent / 100f;

        /// <summary>
        /// The mod's package ID
        /// </summary>
        private const string PackageId = "juanlopez2008.lightsout2";

        /// <summary>
        /// Whether or not light flicking is enabled
        /// </summary>
        /// <returns>True if light flicking is enabled, false otherwise</returns>
        public static bool FlickingLightsEnabled()
        {
            return FlickLights;
        }

        /// <summary>
        /// Whether or not integrity checks are enabled
        /// </summary>
        /// <returns>True if integrity checking is enabled, false otherwise</returns>
        public static bool IntegrityChecksEnabled()
        {
            return EnableIntegrityChecks;
        }
    }
}