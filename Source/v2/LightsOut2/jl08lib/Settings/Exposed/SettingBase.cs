using jl08lib.Logging;
using jl08lib.Settings.Attributes;
using jl08lib.Settings.IO;
using System;
using System.Reflection;
using Verse;

namespace jl08lib.Settings.Exposed
{
    public abstract class SettingBase
    {
        /// <summary>
        /// The key used for this specific setting
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The name of the member this setting corresponds to on the target type
        /// </summary>
        public readonly string MemberName;

        /// <summary>
        /// Instantiates the exposed setting base object
        /// </summary>
        /// <param name="type">The type that has the setting being exposed</param>
        /// <param name="memberName">The name of the property on the given type that holds the setting</param>
        /// <param name="attribute">The attribute initializing this setting</param>
        /// <param name="logger">The logger to use for errors</param>
        public SettingBase(Type type, string memberName, SettingAttributeBase attribute, LoggerBase logger)
        {
            Name = string.IsNullOrWhiteSpace(attribute.SettingKey) ? memberName : attribute.SettingKey;
            MemberName = memberName;
            // fall back to the type that the attribute is on if the delegate type is not specified
            Type delegateType = attribute.ShowInSettingsDelegateType ?? type;
            _showInSettingsDelegate = GetShowInSettingsMenuDelegate(Name, delegateType, attribute.ShowInSettingsDelegateName, logger);
            _onSettingChangedDelegate = GetOnSettingChangedDelegate(Name, delegateType, attribute.OnSettingChangedDelegateName, logger);

            if (type.GetField(memberName, Flags) is FieldInfo field)
            {
                _field = field;
            }
            else if (type.GetProperty(memberName, Flags) is PropertyInfo property)
            {
                _property = property;
            }
        }

        /// <summary>
        /// Performs the exposing of the data in the setting
        /// </summary>
        /// <param name="scribe">The setting scribe to use</param>
        /// <param name="logger">The logger to use</param>
        /// <remarks>
        /// Should call ExposeData using the concrete type for TSettingType
        /// </remarks>
        public abstract void ExposeData(SettingScribeBase scribe, LoggerBase logger);

        /// <summary>
        /// Expose the setting with the specified type
        /// </summary>
        /// <param name="scribe">The scribe to use</param>
        /// <param name="defaultValue">The default value to use</param>
        /// <param name="forceSave">Pass true to force saving this setting even if it matches the default</param>
        protected void ExposeData<TSettingType>(SettingScribeBase scribe, TSettingType defaultValue, bool forceSave = false)
        {
            TSettingType settingValue = Get<TSettingType>();
            string key = $"ExposedSetting.{Name}".Replace(" ", string.Empty);
            scribe.Look(ref settingValue, key, defaultValue, forceSave);
            Set(settingValue);
        }

        /// <summary>
        /// Renders the setting in the settings menu
        /// </summary>
        /// <param name="settingListing">The listing to add it to</param>
        public void DrawSetting(Listing_Standard settingListing)
        {
            if (_showInSettingsDelegate?.Invoke() ?? true)
            {
                DrawSettingInner(settingListing);
            }
        }

        /// <summary>
        /// Renders the setting in the settings menu
        /// </summary>
        /// <param name="settingListing">The listing to add it to</param>
        protected abstract void DrawSettingInner(Listing_Standard settingListing);

        /// <summary>
        /// Retrieves the value of the field or property
        /// </summary>
        /// <returns>The value in the setting, or the default if not valid</returns>
        internal TSettingType Get<TSettingType>()
        {
            if (_field != null)
            {
                return (TSettingType)_field.GetValue(null);
            }

            if (_property != null)
            {
                return (TSettingType)_property.GetValue(null);
            }
            return default;
        }

        /// <summary>
        /// Sets the value into the field or property
        /// </summary>
        /// <param name="value">The value to set</param>
        internal void Set<TSettingType>(TSettingType value)
        {
            _field?.SetValue(null, value);
            _property?.SetValue(null, value);
        }

        /// <summary>
        /// Retrieves the delegate that determines if this setting should be shown in the settings menu
        /// </summary>
        /// <param name="settingName">The name of the setting getting the delegate for</param>
        /// <param name="delegateType">The type that has the delegate method</param>
        /// <param name="delegateMethodName">The name of the method</param>
        /// <param name="logger">A logger to use in the event of errors</param>
        /// <returns>A delegate to use to determine if the setting should show</returns>
        internal static ShowInSettingsMenuDelegate GetShowInSettingsMenuDelegate(string settingName, Type delegateType, string delegateMethodName, LoggerBase logger)
        {
            bool typeIsNull = delegateType is null;
            bool methodNameIsNull = string.IsNullOrWhiteSpace(delegateMethodName);

            // didn't specify a type or method name, so return nothing
            if (typeIsNull || methodNameIsNull) { return null; }
            // otherwise, try to get the method and create a delegate for it
            MethodInfo method = delegateType.GetMethod(delegateMethodName, Flags);
            if (method is null)
            {
                logger?.Error($"Error retrieving setting delegate for {settingName}: could not find method '{delegateMethodName}' on type '{delegateType}'");
                return null; // default to showing the setting if we can't find the method
            }

            // verify it has the right signature
            if (method.ReturnType != typeof(bool))
            {
                logger?.Error($"Error retrieving setting delegate for {settingName}: method '{delegateMethodName}' on type '{delegateType}' has a return type of '{method.ReturnType}' but should return 'bool'");
                return null;
            }
            else if (method.GetParameters().Length != 0)
            {
                logger?.Error($"Error retrieving setting delegate for {settingName}: method '{delegateMethodName}' on type '{delegateType}' must not take any parameters");
                return null;
            }

            // return the delegate
            return () => (bool)method.Invoke(null, null);
        }

        /// <summary>
        /// Retrieves the delegate called when the setting value is changed
        /// </summary>
        /// <param name="settingName">The name of the setting getting the delegate for</param>
        /// <param name="delegateType">The type that has the delegate method</param>
        /// <param name="delegateMethodName">The name of the method</param>
        /// <param name="logger">A logger to use in the event of errors</param>
        /// <returns>A delegate to call when the setting value is changed</returns>
        internal static OnSettingChangedDelegate GetOnSettingChangedDelegate(string settingName, Type delegateType, string delegateMethodName, LoggerBase logger)
        {
            bool typeIsNull = delegateType is null;
            bool methodNameIsNull = string.IsNullOrWhiteSpace(delegateMethodName);

            // didn't specify a type or method name, so return nothing
            if (typeIsNull || methodNameIsNull) { return null; }
            // otherwise, try to get the method and create a delegate for it
            MethodInfo method = delegateType.GetMethod(delegateMethodName, Flags);
            if (method is null)
            {
                logger?.Error($"Error retrieving setting changed delegate for {settingName}: could not find method '{delegateMethodName}' on type '{delegateType}'");
                return null; // default to showing the setting if we can't find the method
            }

            // verify it has the right signature
            if (method.ReturnType != typeof(void))
            {
                logger?.Error($"Error retrieving setting changed delegate for {settingName}: method '{delegateMethodName}' on type '{delegateType}' has a return type of '{method.ReturnType}' but should return 'void'");
                return null;
            }
            else if (method.GetParameters().Length != 0)
            {
                logger?.Error($"Error retrieving setting changed delegate for {settingName}: method '{delegateMethodName}' on type '{delegateType}' must not take any parameters");
                return null;
            }

            // return the delegate
            return () => method.Invoke(null, null);
        }

        /// <summary>
        /// The delegate that determines if this setting should be shown in the settings menu
        /// </summary>
        private readonly ShowInSettingsMenuDelegate _showInSettingsDelegate;

        /// <summary>
        /// The delegate invoked when the setting value is changed
        /// </summary>
        protected readonly OnSettingChangedDelegate _onSettingChangedDelegate;

        /// <summary>
        /// The field to get/set
        /// </summary>
        private readonly FieldInfo _field;

        /// <summary>
        /// The property to get/set
        /// </summary>
        private readonly PropertyInfo _property;

        /// <summary>
        /// The flags to use when attempting to retrieve a member via reflection
        /// </summary>
        /// <remarks>
        /// Only considers static fields/properties
        /// </remarks>
        private const BindingFlags Flags = BindingFlags.Public
            | BindingFlags.Static
            | BindingFlags.NonPublic;

        /// <summary>
        /// The delegate type for showing in the settings menu
        /// </summary>
        /// <returns>True if the setting should show, false otherwise</returns>
        public delegate bool ShowInSettingsMenuDelegate();

        /// <summary>
        /// The delegate type for a delegate called when the setting value is changed
        /// </summary>
        public delegate void OnSettingChangedDelegate();
    }

    /// <summary>
    /// An override of the SettingBase class that has a specific type
    /// </summary>
    /// <typeparam name="TSettingType">The type of this setting</typeparam>
    /// <remarks>
    /// We need a subclass for this so that we can still have an array of SettingBase without type annotation
    /// </remarks>
    public abstract class SettingBase<TSettingType> : SettingBase
    {
        /// <summary>
        /// Instantiates the exposed setting base object
        /// </summary>
        /// <param name="type">The type that has the setting being exposed</param>
        /// <param name="memberName">The name of the property on the given type that holds the setting</param>
        /// <param name="attribute">The attribute initializing this setting</param>
        /// <param name="logger">The logger to use for errors</param>
        /// <param name="defaultValue">The default value to use for the setting</param>
        public SettingBase(Type type, string memberName, SettingAttributeBase attribute, LoggerBase logger, TSettingType defaultValue)
            : base(type, memberName, attribute, logger) 
        {
            _defaultValue = defaultValue;
            Set(_defaultValue);
        }

        public override void ExposeData(SettingScribeBase scribe, LoggerBase logger)
        {
            ExposeData(scribe, _defaultValue);
            // if we are saving the value and it was changed, invoke the change delegate
            if (scribe.Saving && !Get().Equals(_startingValue))
            {
                _startingValue = Get();
                _onSettingChangedDelegate?.Invoke();
            }
            logger.Trace($"Exposing {typeof(TSettingType).Name} setting with key {Name} (value: {Get<TSettingType>()}");
        }

        protected override void DrawSettingInner(Listing_Standard settingListing)
        {
            if (_loadedStartingValue) { return; }
            _loadedStartingValue = true;
            _startingValue = Get();
        }

        /// <summary>
        /// Allows getting the setting of the current generic type
        /// </summary>
        /// <returns>The result of Get<TSettingType>()</returns>
        public TSettingType Get() { return Get<TSettingType>(); }

        /// <summary>
        /// The default value of the setting
        /// </summary>
        protected readonly TSettingType _defaultValue;

        /// <summary>
        /// The value that this was when the setting was first loaded
        /// </summary>
        protected TSettingType _startingValue;

        /// <summary>
        /// Whether or not the starting value has already been set
        /// </summary>
        private bool _loadedStartingValue;
    }
}