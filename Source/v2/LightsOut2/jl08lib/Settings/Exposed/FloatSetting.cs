using jl08lib.Logging;
using jl08lib.Settings.Attributes;
using jl08lib.Settings.Extensions;
using jl08lib.Settings.IO;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Verse;

namespace jl08lib.Settings.Exposed
{
    public class FloatSetting : SettingBase
    {
        public FloatSetting(Type type, string memberName, FloatSettingAttribute attribute)
            : base(type, memberName, attribute)
        {
            _label = attribute.GetString(attribute.Label);
            _tooltip = attribute.GetString(attribute.Tooltip);
            _defaultValue = attribute.DefaultValue;
            Set(_defaultValue);
            _minValue = attribute.MinValue;
            _maxValue = attribute.MaxValue;
        }

        public override void DrawSetting(Listing_Standard settingListing)
        {
            // initialize the buffer
            if (_buffer is null)
            {
                _curValue = Get<float>();
                _buffer = _curValue.ToString();
            }
            
            settingListing.TextFieldNumericLabelled(_label, ref _curValue, ref _buffer, _tooltip);
        }

        

        public override void ExposeData(SettingScribeBase scribe, LoggerBase logger)
        {
            // if there was user input and it's valid, store it before saving
            if (_buffer != null && IsValid(_curValue))
            {
                Set(_curValue);
            }

            ExposeData(scribe, _defaultValue);
            // clear out the buffer so it gets refreshed on the next load
            _buffer = null;
            _curValue = Get<float>();
        }

        /// <summary>
        /// Whether or not the input is valid
        /// </summary>
        /// <param name="input">The input to check</param>
        /// <returns>Whether or not the given input is valid</returns>
        private bool IsValid(float input)
        {
            return input >= _minValue && input <= _maxValue;
        }

        /// <summary>
        /// The buffer for the input field
        /// </summary>
        private string _buffer;

        /// <summary>
        /// The current value for the input field
        /// </summary>
        internal float _curValue;

        /// <summary>
        /// The label to use when drawing the setting
        /// </summary>
        private readonly string _label;

        /// <summary>
        /// The tooltip to show when hovering over this setting
        /// </summary>
        private readonly string _tooltip;

        /// <summary>
        /// The default value for this setting
        /// </summary>
        private readonly float _defaultValue;

        /// <summary>
        /// The minimum value for this setting
        /// </summary>
        private readonly float _minValue;

        /// <summary>
        /// The maximum value for this setting
        /// </summary>
        private readonly float _maxValue;
    }
}