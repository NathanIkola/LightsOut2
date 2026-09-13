using jl08lib.Logging;
using jl08lib.Settings.Attributes;
using jl08lib.Settings.IO;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

namespace jl08lib.Settings.Exposed
{
    /// <summary>
    /// A setting that allows the user to select from a list of enum values
    /// </summary>
    public class EnumSetting : SettingBase<int>
    {
        public EnumSetting(Type type, string memberName, EnumSettingAttribute attribute, LoggerBase logger)
            : base(type, memberName, attribute, logger, attribute.DefaultValue)
        {
            Type enumType = null;

            if (type.GetField(memberName, Flags) is FieldInfo field)
            {
                enumType = field.FieldType;
            }
            else if (type.GetProperty(memberName, Flags) is PropertyInfo property)
            {
                enumType = property.PropertyType;
            }

            _enumOptions = new Dictionary<int, EnumOptionData>();
            // build out the options from the enum values
            foreach(int value in Enum.GetValues(enumType))
            {
                EnumOptionData displayData = GetDisplayData(enumType, value);
                _enumOptions.Add(value, displayData);
            }
        }

        /// <summary>
        /// Looks up the option's display data
        /// </summary>
        /// <param name="type">The enum type</param>
        /// <param name="value">The enum value</param>
        /// <returns>The display data for this option</returns>
        private EnumOptionData GetDisplayData(Type type, int value)
        {
            string valueName = Enum.GetName(type, value);
            MemberInfo[] memberInfos = type.GetMember(valueName);
            EnumOptionAttribute attribute = memberInfos[0].GetCustomAttribute<EnumOptionAttribute>();

            string caption = null;
            string description = null;

            if (attribute != null)
            {
                caption = attribute.CaptionLocalized;
                description = attribute.DescriptionLocalized;
            }

            // if there wasn't a caption specified (or no attribute at all), default to the enum value name
            if (string.IsNullOrWhiteSpace(caption))
            {
                caption = valueName;
            }
            
            return new EnumOptionData()
            {
                Caption = caption,
                Description = description,
            };
        }

        protected override void DrawSettingInner(Listing_Standard settingListing)
        {
            EnumOptionData selectedOption = _enumOptions.GetValueOrDefault(Get<int>());
            if (settingListing.ButtonTextLabeledPct(_label, selectedOption.Caption, 0.6f, TextAnchor.MiddleLeft, null, _tooltip))
            {
                List<FloatMenuOption> menuOptions = new List<FloatMenuOption>();
                foreach(KeyValuePair<int, EnumOptionData> option in _enumOptions)
                {
                    EnumOptionData optionData = option.Value;
                    menuOptions.Add(new FloatMenuOption(optionData.Caption, () =>
                    {
                        Set(option.Key);
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(menuOptions));
            }
        }

        public override void ExposeData(SettingScribeBase scribe, LoggerBase logger)
        {
            ExposeData(scribe, _defaultValue);
            EnumOptionData selectedOption = _enumOptions.GetValueOrDefault(Get<int>());
            logger.Trace($"Exposing enum setting with key {Name} (value: {selectedOption.Caption} ({Get<int>()})");
        }

        /// <summary>
        /// The list of options that can be displayed
        /// </summary>
        private readonly Dictionary<int, EnumOptionData> _enumOptions;

        /// <summary>
        /// A class that holds the data for an individual option
        /// </summary>
        private class EnumOptionData
        {
            /// <summary>
            /// The enum's display caption
            /// </summary>
            public string Caption { get; set; }

            /// <summary>
            /// The enum's description
            /// </summary>
            public string Description { get; set; }
        }
    }
}