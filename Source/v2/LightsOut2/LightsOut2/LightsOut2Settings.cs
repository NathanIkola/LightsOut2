using jl08lib.Settings;
using jl08lib.Settings.Attributes;
using Verse;

namespace LightsOut2
{
    /// <summary>
    /// The settings used in LightsOut 2
    /// </summary>
    public class LightsOut2Settings : ModSettingsBase
    {
        /// <summary>
        /// The mod's package ID
        /// </summary>
        private const string PackageId = "juanlopez2008.lightsout2";

        /// <summary>
        /// Whether or not to turn off lights in empty rooms
        /// </summary>
        [BooleanSetting(PackageId,
            DefaultValue = true,
            Label = "Turn off lights in empty rooms",
            Tooltip = "If enabled, lights in empty rooms will be turned off to save power")]
        public static bool FlickLights;

        /// <summary>
        /// The coefficient (in percent) of power devices use in standby mode
        /// </summary>
        /// <remarks>
        /// This does not affect lights, which always use 0% when in standby
        /// </remarks>
        public static int StandbyCoefficientPercent = 0;

        /// <summary>
        /// The coefficient (in percent) of power devices use when active
        /// </summary>
        /// <remarks>
        /// This does not affect lights, which always use 100% when active
        /// </remarks>
        public static int ActiveCoefficientPercent = 100;

        /// <summary>
        /// Whether or not to keep lights on when pawns are sleeping
        /// </summary>
        [BooleanSetting(PackageId,
            DefaultValue = false,
            Label = "Keep lights on when pawns are sleeping",
            Tooltip = "If enabled, lights will stay on when all pawns in the room are sleeping")]
        public static bool NightLights;

        // handle debug logging settings somehow

        /// <summary>
        /// The amount of time (in seconds) to wait before turning off lights
        /// </summary>
        [FloatSetting(PackageId,
            DefaultValue = 1.5f,
            MinValue = 0f,
            Label = "Seconds to delay turning off lights",
            Tooltip = "The number of seconds to wait before turning off the lights (to combat rapid flickering at higher game speeds)")]
        public static float LightDelaySeconds;
    }
}