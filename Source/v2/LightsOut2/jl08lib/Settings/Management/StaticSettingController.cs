using jl08lib.Settings.Exposed;
using System.Collections.Generic;
using Verse;

namespace jl08lib.Settings.Management
{
    /// <summary>
    /// A static form of the setting controller
    /// </summary>
    [StaticConstructorOnStartup]
    public class StaticSettingController
    {
        /// <summary>
        /// The singleton instance that can be used
        /// </summary>
        public static SettingController Instance { get; }

        /// <summary>
        /// Static initializer for the singleton instance
        /// </summary>
        static StaticSettingController()
        {
            Instance = new SettingController();
            StaticSettingEvents.InvokeLoadStaticSettings();
        }

        /// <summary>
        /// Retrieves the list of settings from the singleton for the given mod packageId
        /// </summary>
        /// <param name="modPackageId">The packageId of the mod that should show the setting</param>
        /// <returns>The list of settings to show for this mod</returns>
        public static List<SettingBase> GetSettings(string modPackageId)
        {
            return Instance?.GetSettings(modPackageId);
        }
    }
}