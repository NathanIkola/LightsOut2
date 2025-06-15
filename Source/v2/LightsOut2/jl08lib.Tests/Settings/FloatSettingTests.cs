using jl08lib.Logging;
using jl08lib.Settings.Attributes;
using jl08lib.Settings.Exposed;
using jl08lib.Settings.IO;
using jl08lib.Settings.Management;
using jl08lib.Tests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace jl08lib.Tests.Settings
{
    [TestClass]
    public class FloatSettingTests
    {
        /// <summary>
        /// Verifies that the setting search finds the correct fields and properties for floats
        /// </summary>
        [TestMethod]
        public void SettingController_LocateAllSettingsOnType_FindsAllFloat()
        {
            List<Type> attributeTypes = new List<Type>()
            {
                typeof(FloatSettingAttribute),
            };

            List<Tuple<SettingAttributeBase, SettingBase>> settings = SettingController.LocateAllSettingsOnType(typeof(FloatSettings), attributeTypes).ToList();

            int numSettingsExpected = 2;
            int numSettingsFound = settings.Count;
            Assert.AreEqual(numSettingsExpected, numSettingsFound, "Number of settings found on type");

            List<string> expectedMembers = new List<string>()
            {
                nameof(FloatSettings.FieldSetting),
                nameof(FloatSettings.PropertySetting),
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
        /// Verifies that the float setting exposes data correctly
        /// </summary>
        [TestMethod]
        public void FloatSetting_ExposeData_GetsFields()
        {
            FloatSettingAttribute attr = new FloatSettingAttribute("Test")
            {
                SettingKey = "field",
            };
            FloatSetting field = new FloatSetting(typeof(FloatSettings), nameof(FloatSettings.FieldSetting), attr);

            FloatSettings.FieldSetting = 3.14f; // Set the field to a known value
            float expectedValue = FloatSettings.FieldSetting;
            float actual = field.Get<float>();
            Assert.AreEqual(expectedValue, actual, "Retrieved the correct value from the static field");

            FloatSettings.FieldSetting = 2.71f; // Update the field to a new value
            expectedValue = FloatSettings.FieldSetting;
            actual = field.Get<float>();
            Assert.AreEqual(expectedValue, actual, "Retrieved the updated value from the static field");
        }
        /// <summary>
        /// Verifies that the float setting exposes data correctly for properties
        /// </summary>
        [TestMethod]
        public void FloatSetting_ExposeData_GetsProperties()
        {
            FloatSettingAttribute attr = new FloatSettingAttribute("Test")
            {
                SettingKey = "property",
            };
            FloatSetting property = new FloatSetting(typeof(FloatSettings), nameof(FloatSettings.PropertySetting), attr);

            FloatSettings.PropertySetting = 3.14f; // Set the property to a known value
            float expectedValue = FloatSettings.PropertySetting;
            float actual = property.Get<float>();
            Assert.AreEqual(expectedValue, actual, "Retrieved the correct value from the static property");

            FloatSettings.PropertySetting = 2.71f; // Update the property to a new value
            expectedValue = FloatSettings.PropertySetting;
            actual = property.Get<float>();
            Assert.AreEqual(expectedValue, actual, "Retrieved the updated value from the static property");
        }

        /// <summary>
        /// Verifies that setting the value via FloatSetting updates the static field
        /// </summary>
        [TestMethod]
        public void FloatSetting_SetValue_SetsField()
        {
            FloatSettingAttribute attr = new FloatSettingAttribute("Test")
            {
                SettingKey = "field",
            };
            FloatSetting field = new FloatSetting(typeof(FloatSettings), nameof(FloatSettings.FieldSetting), attr);

            FloatSettings.FieldSetting = 3.14f;
            float expectedValue = 2.71f;
            field.Set(expectedValue);
            float actual = field.Get<float>();
            Assert.AreEqual(expectedValue, actual, "Set the static field value via FloatSetting");
        }

        /// <summary>
        /// Verifies that setting the value via FloatSetting updates the static property
        /// </summary>
        [TestMethod]
        public void FloatSetting_SetValue_SetsProperty()
        {
            FloatSettingAttribute attr = new FloatSettingAttribute("Test")
            {
                SettingKey = "property",
            };
            FloatSetting property = new FloatSetting(typeof(FloatSettings), nameof(FloatSettings.PropertySetting), attr);

            FloatSettings.PropertySetting = 3.14f;
            float expectedValue = 2.71f;
            property.Set(expectedValue);
            float actual = property.Get<float>();
            Assert.AreEqual(expectedValue, actual, "Set the static property value via FloatSetting");
        }

        /// <summary>
        /// Verifies that ExposeData will not set the value below the minimum specified in the attribute
        /// </summary>
        [TestMethod]
        public void FloatSetting_ExposeData_BelowMin_DoesNotSet()
        {
            MockSettingScribe scribe = new MockSettingScribe();
            LoggerBase logger = new MockLogger();
            FloatSettingAttribute attr = new FloatSettingAttribute("Test")
            {
                SettingKey = "property",
                MinValue = 9.0f,
                DefaultValue = 10.0f,
            };
            FloatSetting property = new FloatSetting(typeof(FloatSettings), nameof(FloatSettings.PropertySetting), attr);

            float expectedValue = 10.0f; // Default value
            property.Set(expectedValue); // Set the default value
            property._curValue = 5.0f; // Set input to a value below the minimum
            scribe.SimulateSaving = true; // Simulate saving mode for ExposeData
            property.ExposeData(scribe, logger);

            float actual = property.Get<float>();

            Assert.AreEqual(expectedValue, actual, "ExposeData did not set the value below the minimum specified in the attribute");
        }

        /// <summary>
        /// Verifies that ExposeData will not set the value above the maximum specified in the attribute
        /// </summary>
        [TestMethod]
        public void FloatSetting_ExposeData_AboveMax_DoesNotSet()
        {
            MockSettingScribe scribe = new MockSettingScribe();
            LoggerBase logger = new MockLogger();
            FloatSettingAttribute attr = new FloatSettingAttribute("Test")
            {
                SettingKey = "property",
                MaxValue = 9.0f,
                DefaultValue = 8.0f,
            };
            FloatSetting property = new FloatSetting(typeof(FloatSettings), nameof(FloatSettings.PropertySetting), attr);
            float expectedValue = 8.0f; // Default value
            property.Set(expectedValue); // Set the default value
            property._curValue = 10.0f; // Set input to a value above the maximum
            scribe.SimulateSaving = true; // Simulate saving mode for ExposeData
            property.ExposeData(scribe, logger);
            float actual = property.Get<float>();
            Assert.AreEqual(expectedValue, actual, "ExposeData did not set the value above the maximum specified in the attribute");
        }

        /// <summary>
        /// Ensures that the float setting respects the default value when first loaded
        /// </summary>
        [TestMethod]
        public void FloatSetting_RespectsDefault()
        {
            float expectedValue = 3.14f;
            FloatSettingAttribute attr = new FloatSettingAttribute("Test")
            {
                SettingKey = "prop",
                DefaultValue = expectedValue,
            };

            FloatSettings.PropertySetting = 9.0f;
            FloatSetting prop = new FloatSetting(typeof(FloatSettings), nameof(FloatSettings.PropertySetting), attr);

            float actualValue = prop.Get<float>();
            Assert.AreEqual(expectedValue, actualValue, "The setting should get set to the default value on initial load");
        }
    }

    /// <summary>
    /// A class that has boolean settings set up for testing
    /// </summary>
    internal class FloatSettings
    {
        /// <summary>
        /// A field which should be a setting
        /// </summary>
        [FloatSetting("Test")]
        public static float FieldSetting;

        /// <summary>
        /// A field which should not be a setting
        /// </summary>
        public static float FieldNonSetting;

        /// <summary>
        /// A property which should be a setting
        /// </summary>
        [FloatSetting("Test")]
        public static float PropertySetting { get; set; }

        /// <summary>
        /// A property which should not be a setting
        /// </summary>
        public static float PropertyNonSetting { get; set; }
    }
}