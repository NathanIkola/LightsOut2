using jl08lib.Logging;
using jl08lib.Settings.Attributes;
using jl08lib.Settings.Exposed;
using jl08lib.Settings.Management;
using jl08lib.Tests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace jl08lib.Tests.Settings
{
    [TestClass]
    public class EnumSettingTests
    {
        /// <summary>
        /// Verifies that the setting search finds the correct fields and properties for enums
        /// </summary>
        [TestMethod]
        public void SettingController_LocateAllSettingsOnType_FindsAllEnum()
        {
            List<Type> attributeTypes = new List<Type>()
            {
                typeof(EnumSettingAttribute),
            };

            LoggerBase logger = new MockLogger();
            List<Tuple<SettingAttributeBase, SettingBase>> settings = SettingController.LocateAllSettingsOnType(typeof(EnumSettings), attributeTypes, logger).ToList();

            int numSettingsExpected = 2;
            int numSettingsFound = settings.Count;
            Assert.AreEqual(numSettingsExpected, numSettingsFound, "Number of settings found on type");

            List<string> expectedMembers = new List<string>()
            {
                nameof(EnumSettings.FieldSetting),
                nameof(EnumSettings.PropertySetting),
            };

            foreach (Tuple<SettingAttributeBase, SettingBase> setting in settings)
            {
                string memberName = setting.Item2.MemberName;
                bool isExpectedMember = expectedMembers.Contains(memberName);
                if (!isExpectedMember)
                {
                    Assert.Fail($"Encountered unexpected member: {memberName}");
                }
            }
        }

        /// <summary>
        /// Verifies that the enum setting exposes data correctly 
        /// </summary>
        [TestMethod]
        public void EnumSetting_ExposeData_GetsFields()
        {
            EnumSettingAttribute attr = new EnumSettingAttribute("Test")
            {
                SettingKey = "field",
            };
            LoggerBase logger = new MockLogger();
            EnumSetting field = new EnumSetting(typeof(EnumSettings), nameof(EnumSettings.FieldSetting), attr, logger);

            EnumSettings.FieldSetting = EnumForTest.Value2; // Set the field to a known value
            EnumForTest expectedValue = EnumSettings.FieldSetting;
            EnumForTest actual = field.Get<EnumForTest>();
            Assert.AreEqual(expectedValue, actual, "Retrieved the correct value from the static field");

            EnumSettings.FieldSetting = EnumForTest.Value1; // Update the field to a new value
            expectedValue = EnumSettings.FieldSetting;
            actual = field.Get<EnumForTest>();
            Assert.AreEqual(expectedValue, actual, "Retrieved the updated value from the static field");
        }

        /// <summary>
        /// Verifies that the enum setting exposes data correctly
        /// </summary>
        [TestMethod]
        public void EnumSetting_ExposeData_GetsProperties()
        {
            EnumSettingAttribute attr = new EnumSettingAttribute("Test")
            {
                SettingKey = "property",
            };
            LoggerBase logger = new MockLogger();
            EnumSetting field = new EnumSetting(typeof(EnumSettings), nameof(EnumSettings.PropertySetting), attr, logger);

            EnumSettings.PropertySetting = EnumForTest.Value2; // Set the property to a known value
            EnumForTest expectedValue = EnumSettings.PropertySetting;
            EnumForTest actual = field.Get<EnumForTest>();
            Assert.AreEqual(expectedValue, actual, "Retrieved the correct value from the static property");

            EnumSettings.PropertySetting = EnumForTest.Value1; // Update the property to a new value
            expectedValue = EnumSettings.PropertySetting;
            actual = field.Get<EnumForTest>();
            Assert.AreEqual(expectedValue, actual, "Retrieved the updated value from the static property");
        }

        /// <summary>
        /// Verifies that setting the value via EnumSetting updates the static field
        /// </summary>
        [TestMethod]
        public void EnumSetting_SetValue_SetsField()
        {
            EnumSettingAttribute attr = new EnumSettingAttribute("Test")
            {
                SettingKey = "field",
            };
            LoggerBase logger = new MockLogger();
            EnumSetting field = new EnumSetting(typeof(EnumSettings), nameof(EnumSettings.FieldSetting), attr, logger);

            EnumSettings.FieldSetting = EnumForTest.Value2;
            EnumForTest expectedValue = EnumForTest.Value1;
            field.Set(expectedValue);
            EnumForTest actual = field.Get<EnumForTest>();
            Assert.AreEqual(expectedValue, actual, "Set the static field value via EnumSetting");
        }

        /// <summary>
        /// Verifies that setting the value via EnumSetting updates the static property
        /// </summary>
        [TestMethod]
        public void EnumSetting_SetValue_SetsProperty()
        {
            EnumSettingAttribute attr = new EnumSettingAttribute("Test")
            {
                SettingKey = "property",
            };
            LoggerBase logger = new MockLogger();
            EnumSetting field = new EnumSetting(typeof(EnumSettings), nameof(EnumSettings.PropertySetting), attr, logger);

            EnumSettings.PropertySetting = EnumForTest.Value2;
            EnumForTest expectedValue = EnumForTest.Value1;
            field.Set(expectedValue);
            EnumForTest actual = field.Get<EnumForTest>();
            Assert.AreEqual(expectedValue, actual, "Set the static property value via EnumSetting");
        }

        /// <summary>
        /// Ensures that the setting respects the default value when first loaded
        /// </summary>
        [TestMethod]
        public void EnumSetting_RespectsDefault()
        {
            EnumForTest expectedValue = EnumForTest.Value1;
            EnumSettingAttribute attr = new EnumSettingAttribute("Test")
            {
                SettingKey = "prop",
                DefaultValue = (int)expectedValue,
            };

            LoggerBase logger = new MockLogger();
            EnumSettings.PropertySetting = EnumForTest.Value2;
            EnumSetting prop = new EnumSetting(typeof(EnumSettings), nameof(EnumSettings.PropertySetting), attr, logger);

            EnumForTest actualValue = prop.Get<EnumForTest>();
            Assert.AreEqual(expectedValue, actualValue, "The setting should get set to the default value on initial load");
        }

        /// <summary>
        /// An enum to test with
        /// </summary>
        internal enum EnumForTest
        {
            Value0 = 0,
            Value1 = 1,
            Value2 = 2,
        }

        /// <summary>
        /// A class that has enum settings set up for testing
        /// </summary>
        internal class EnumSettings
        {
            /// <summary>
            /// A field which should be a setting
            /// </summary>
            [EnumSetting("Test")]
            public static EnumForTest FieldSetting;

            /// <summary>
            /// A field which should not be a setting
            /// </summary>
            public static EnumForTest FieldNonSetting;

            /// <summary>
            /// A property which should be a setting
            /// </summary>
            [EnumSetting("Test")]
            public static EnumForTest PropertySetting { get; set; }

            /// <summary>
            /// A property which should not be a setting
            /// </summary>
            public static EnumForTest PropertyNonSetting { get; set; }
        }
    }
}