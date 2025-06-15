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
    public class IntSettingTests
    {
        /// <summary>
        /// Verifies that the setting search finds the correct fields and properties for ints
        /// </summary>
        [TestMethod]
        public void SettingController_LocateAllSettingsOnType_FindsAllInt()
        {
            List<Type> attributeTypes = new List<Type>()
            {
                typeof(IntSettingAttribute),
            };

            List<Tuple<SettingAttributeBase, SettingBase>> settings = SettingController.LocateAllSettingsOnType(typeof(IntSettings), attributeTypes).ToList();

            int numSettingsExpected = 2;
            int numSettingsFound = settings.Count;
            Assert.AreEqual(numSettingsExpected, numSettingsFound, "Number of settings found on type");

            List<string> expectedMembers = new List<string>()
            {
                nameof(IntSettings.FieldSetting),
                nameof(IntSettings.PropertySetting),
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
        /// Verifies that the int setting exposes data correctly
        /// </summary>
        [TestMethod]
        public void IntSetting_ExposeData_GetsFields()
        {
            IntSettingAttribute attr = new IntSettingAttribute("Test", new MockStringTranslator())
            {
                SettingKey = "field",
            };
            IntSetting field = new IntSetting(typeof(IntSettings), nameof(IntSettings.FieldSetting), attr);

            IntSettings.FieldSetting = 3; // Set the field to a known value
            int expectedValue = IntSettings.FieldSetting;
            int actual = field.Get<int>();
            Assert.AreEqual(expectedValue, actual, "Retrieved the correct value from the static field");

            IntSettings.FieldSetting = 2; // Update the field to a new value
            expectedValue = IntSettings.FieldSetting;
            actual = field.Get<int>();
            Assert.AreEqual(expectedValue, actual, "Retrieved the updated value from the static field");
        }

        /// <summary>
        /// Verifies that the int setting exposes data correctly for properties
        /// </summary>
        [TestMethod]
        public void IntSetting_ExposeData_GetsProperties()
        {
            IntSettingAttribute attr = new IntSettingAttribute("Test", new MockStringTranslator())
            {
                SettingKey = "property",
            };
            IntSetting property = new IntSetting(typeof(IntSettings), nameof(IntSettings.PropertySetting), attr);

            IntSettings.PropertySetting = 42; // Set the property to a known value
            int expectedValue = IntSettings.PropertySetting;
            int actual = property.Get<int>();
            Assert.AreEqual(expectedValue, actual, "Retrieved the correct value from the static property");

            IntSettings.PropertySetting = 17; // Update the property to a new value
            expectedValue = IntSettings.PropertySetting;
            actual = property.Get<int>();
            Assert.AreEqual(expectedValue, actual, "Retrieved the updated value from the static property");
        }

        /// <summary>
        /// Verifies that setting the value via IntSetting updates the static field
        /// </summary>
        [TestMethod]
        public void IntSetting_SetValue_SetsField()
        {
            IntSettingAttribute attr = new IntSettingAttribute("Test", new MockStringTranslator())
            {
                SettingKey = "field",
            };
            IntSetting field = new IntSetting(typeof(IntSettings), nameof(IntSettings.FieldSetting), attr);

            IntSettings.FieldSetting = 10;
            int expectedValue = 20;
            field.Set(expectedValue);
            int actual = field.Get<int>();
            Assert.AreEqual(expectedValue, actual, "Set the static field value via IntSetting");
        }

        /// <summary>
        /// Verifies that setting the value via IntSetting updates the static property
        /// </summary>
        [TestMethod]
        public void IntSetting_SetValue_SetsProperty()
        {
            IntSettingAttribute attr = new IntSettingAttribute("Test", new MockStringTranslator())
            {
                SettingKey = "property",
            };
            IntSetting property = new IntSetting(typeof(IntSettings), nameof(IntSettings.PropertySetting), attr);

            IntSettings.PropertySetting = 10;
            int expectedValue = 20;
            property.Set(expectedValue);
            int actual = property.Get<int>();
            Assert.AreEqual(expectedValue, actual, "Set the static property value via IntSetting");
        }

        /// <summary>
        /// Verifies that ExposeData will not set the value below the minimum specified in the attribute
        /// </summary>
        [TestMethod]
        public void IntSetting_ExposeData_BelowMin_DoesNotSet()
        {
            MockSettingScribe scribe = new MockSettingScribe();
            LoggerBase logger = new MockLogger();
            IntSettingAttribute attr = new IntSettingAttribute("Test", new MockStringTranslator())
            {
                SettingKey = "property",
                MinValue = 9,
                DefaultValue = 10,
            };
            IntSetting property = new IntSetting(typeof(IntSettings), nameof(IntSettings.PropertySetting), attr);

            int expectedValue = 10; // Default value
            property.Set(expectedValue); // Set the default value
            property._curValue = 5; // Set input to a value below the minimum
            scribe.SimulateSaving = true; // Simulate saving mode for ExposeData
            property.ExposeData(scribe, logger);

            int actual = property.Get<int>();

            Assert.AreEqual(expectedValue, actual, "ExposeData did not set the value below the minimum specified in the attribute");
        }

        /// <summary>
        /// Verifies that ExposeData will not set the value above the maximum specified in the attribute
        /// </summary>
        [TestMethod]
        public void IntSetting_ExposeData_AboveMax_DoesNotSet()
        {
            MockSettingScribe scribe = new MockSettingScribe();
            LoggerBase logger = new MockLogger();
            IntSettingAttribute attr = new IntSettingAttribute("Test", new MockStringTranslator())
            {
                SettingKey = "property",
                MaxValue = 9,
                DefaultValue = 8,
            };
            IntSetting property = new IntSetting(typeof(IntSettings), nameof(IntSettings.PropertySetting), attr);
            int expectedValue = 8; // Default value
            property.Set(expectedValue); // Set the default value
            property._curValue = 10; // Set input to a value above the maximum
            scribe.SimulateSaving = true; // Simulate saving mode for ExposeData
            property.ExposeData(scribe, logger);
            int actual = property.Get<int>();
            Assert.AreEqual(expectedValue, actual, "ExposeData did not set the value above the maximum specified in the attribute");
        }

        /// <summary>
        /// Ensures that the int setting respects the default value when first loaded
        /// </summary>
        [TestMethod]
        public void IntSetting_RespectsDefault()
        {
            int expectedValue = 42;
            IntSettingAttribute attr = new IntSettingAttribute("Test", new MockStringTranslator())
            {
                SettingKey = "prop",
                DefaultValue = expectedValue,
            };

            IntSettings.PropertySetting = 99;
            IntSetting prop = new IntSetting(typeof(IntSettings), nameof(IntSettings.PropertySetting), attr);

            int actualValue = prop.Get<int>();
            Assert.AreEqual(expectedValue, actualValue, "The setting should get set to the default value on initial load");
        }
    }

    /// <summary>
    /// A class that has int settings set up for testing
    /// </summary>
    internal class IntSettings
    {
        /// <summary>
        /// A field which should be a setting
        /// </summary>
        [IntSetting("Test")]
        public static int FieldSetting;

        /// <summary>
        /// A field which should not be a setting
        /// </summary>
        public static int FieldNonSetting;

        /// <summary>
        /// A property which should be a setting
        /// </summary>
        [IntSetting("Test")]
        public static int PropertySetting { get; set; }

        /// <summary>
        /// A property which should not be a setting
        /// </summary>
        public static int PropertyNonSetting { get; set; }
    }
}