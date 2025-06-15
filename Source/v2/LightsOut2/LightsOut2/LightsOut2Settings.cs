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

        [BooleanSetting(
            ModPackageId = PackageId, 
            Label = "My field", 
            Tooltip = "My field tooltip",
            SettingKey = "MySetting1")]
        public static bool MySetting1;

        [BooleanSetting(
            ModPackageId = PackageId, 
            Label = "My property", 
            Tooltip = "My property tooltip", 
            SettingKey = "MySetting2")]
        public static bool MySetting2 { get; set; }

        [FloatSetting(
            ModPackageId = PackageId,
            Label = "This is a float (DO NOT DO MORE THAN 100)",
            SettingKey = "MyFloat",
            DefaultValue = 4.7f,
            MaxValue = 100f)]
        public static float MyFloatSetting { get; set; }
    }
}