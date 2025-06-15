namespace jl08lib.Settings.Attributes.Data
{
    /// <summary>
    /// An interface for attribute data which should provide a default value
    /// </summary>
    /// <typeparam name="TSettingType">The type of setting</typeparam>
    public interface IDefaultableSettingAttributeDate<TSettingType> : ISettingAttributeData
    {
        /// <summary>
        /// The default value of the setting
        /// </summary>
        TSettingType DefaultValue { get; set; }
    }
}