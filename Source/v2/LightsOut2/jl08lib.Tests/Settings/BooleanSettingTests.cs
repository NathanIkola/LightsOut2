using jl08lib.Settings.Attributes;
using jl08lib.Settings.Exposed;
using jl08lib.Settings.Management;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace jl08lib.Tests.Settings
{
    [TestClass]
    public class BooleanSettingTests
    {
        /// <summary>
        /// Verifies that the settings search finds the correct fields and properties for booleans
        /// </summary>
        [TestMethod]
        public void SettingController_LocateAllSettingsOnType_FindsAllBool()
        {
            List<Type> attributeTypes = new List<Type>()
            {
                typeof(BooleanSettingAttribute),
            };

            List<Tuple<SettingAttributeBase, SettingBase>> settings = SettingController.LocateAllSettingsOnType(typeof(BooleanSettings), attributeTypes).ToList();

            int numSettingsExpected = 2;
            int numSettingsFound = settings.Count;
            Assert.AreEqual(numSettingsExpected, numSettingsFound, "Number of settings found on type");

            List<string> expectedMembers = new List<string>()
            {
                nameof(BooleanSettings.FieldSetting),
                nameof(BooleanSettings.PropertySetting),
            };
            foreach(Tuple<SettingAttributeBase, SettingBase> setting in settings)
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
        /// Ensures that the value retrieved from the setting entry matches the underlying field
        /// </summary>
        [TestMethod]
        public void BooleanSetting_ExposeData_GetsField()
        {
            BooleanSettingAttribute attr = new BooleanSettingAttribute("Test")
            {
                SettingKey = "field",
            };
            BooleanSetting field = new BooleanSetting(typeof(BooleanSettings), nameof(BooleanSettings.FieldSetting), attr);

            BooleanSettings.FieldSetting = false;
            bool expectedValue = BooleanSettings.FieldSetting;
            bool actual = field.Get<bool>();
            Assert.AreEqual(expectedValue, actual, "Retrieved the correct value from the static");

            BooleanSettings.FieldSetting = true;
            expectedValue = BooleanSettings.FieldSetting;
            actual = field.Get<bool>();
            Assert.AreEqual(expectedValue, actual, "Retrieved the correct value from the static after update");
        }

        /// <summary>
        /// Ensures that the value retrieved from the setting entry matches the underlying property
        /// </summary>
        [TestMethod]
        public void BooleanSetting_ExposeData_GetsProperty()
        {
            BooleanSettingAttribute attr = new BooleanSettingAttribute("Test")
            {
                SettingKey = "prop",
            };
            BooleanSetting prop = new BooleanSetting(typeof(BooleanSettings), nameof(BooleanSettings.PropertySetting), attr);

            BooleanSettings.PropertySetting = false;
            bool expectedValue = BooleanSettings.PropertySetting;
            bool actual = prop.Get<bool>();
            Assert.AreEqual(expectedValue, actual, "Retrieved the correct value from the static");

            BooleanSettings.PropertySetting = true;
            expectedValue = BooleanSettings.PropertySetting;
            actual = prop.Get<bool>();
            Assert.AreEqual(expectedValue, actual, "Retrieved the correct value from the static after update");
        }

        /// <summary>
        /// Ensures that updating the setting entry actually updates the underlying field
        /// </summary>
        [TestMethod]
        public void BooleanSetting_ExposeData_SetsField()
        {
            BooleanSettingAttribute attr = new BooleanSettingAttribute("Test")
            {
                SettingKey = "field",
            };
            BooleanSetting field = new BooleanSetting(typeof(BooleanSettings), nameof(BooleanSettings.FieldSetting), attr);

            BooleanSettings.FieldSetting = false;
            bool expectedValue = true;
            field.Set(expectedValue);
            bool actual = BooleanSettings.FieldSetting;
            Assert.AreEqual(expectedValue, actual, "Updated the value in the static");
        }

        /// <summary>
        /// Ensures that updating the setting entry actually updates the underlying property
        /// </summary>
        [TestMethod]
        public void BooleanSetting_ExposeData_SetsProperty()
        {
            BooleanSettingAttribute attr = new BooleanSettingAttribute("Test")
            {
                SettingKey = "prop",
            };
            BooleanSetting prop = new BooleanSetting(typeof(BooleanSettings), nameof(BooleanSettings.PropertySetting), attr);

            BooleanSettings.PropertySetting = false;
            bool expectedValue = true;
            prop.Set(expectedValue);
            bool actual = BooleanSettings.PropertySetting;
            Assert.AreEqual(expectedValue, actual, "Updated the value in the static");
        }

        /// <summary>
        /// Verifies that the default value is set when the setting is first loaded
        /// </summary>
        [TestMethod]
        public void BooleanSetting_RespectsDefault()
        {
            bool expectedValue = true; // Default value for the setting
            BooleanSettingAttribute attr = new BooleanSettingAttribute("Test")
            {
                SettingKey = "prop",
                DefaultValue = expectedValue,
            };

            BooleanSettings.PropertySetting = false; // Set to a non-default value
            BooleanSetting prop = new BooleanSetting(typeof(BooleanSettings), nameof(BooleanSettings.PropertySetting), attr);

            bool actualValue = prop.Get<bool>();
            Assert.AreEqual(expectedValue, actualValue, "The setting should get set to the default value on initial load");
        }
    }

    /// <summary>
    /// A class that has boolean settings set up for testing
    /// </summary>
    internal class BooleanSettings
    {
        /// <summary>
        /// A field which should be a setting
        /// </summary>
        [BooleanSetting("Test")]
        public static bool FieldSetting;

        /// <summary>
        /// A field which should not be a setting
        /// </summary>
        public static bool FieldNonSetting;

        /// <summary>
        /// A property which should be a setting
        /// </summary>
        [BooleanSetting("Test")]
        public static bool PropertySetting { get; set; }

        /// <summary>
        /// A property which should not be a setting
        /// </summary>
        public static bool PropertyNonSetting { get; set; }
    }
}