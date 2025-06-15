namespace jl08lib.Settings.Attributes.Data
{
    /// <summary>
    /// Basic data that all settings should have
    /// </summary>
    public interface ISettingAttributeData
    {
        /// <summary>
        /// The package ID for the mod (same as the packageId node in About.xml)
        /// </summary>
        string ModPackageId { get; set; }

        /// <summary>
        /// The key for this setting
        /// </summary>
        string SettingKey { get; set; }

        /// <summary>
        /// The label to show when drawing this setting in the menu
        /// </summary>
        string Label { get; set; }

        /// <summary>
        /// The tooltip to show when hovering over this setting
        /// </summary>
        string Tooltip { get; set; }
    }
}