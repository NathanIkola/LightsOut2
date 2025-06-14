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

            List<Tuple<ExposeSettingAttribute, ExposedSettingBase>> settings = SettingController.LocateAllSettingsOnType(typeof(BooleanSettings), attributeTypes).ToList();

            int numSettingsExpected = 2;
            int numSettingsFound = settings.Count;
            Assert.AreEqual(numSettingsExpected, numSettingsFound, "Number of settings found on type");

            List<string> expectedMembers = new List<string>()
            {
                nameof(BooleanSettings.BooleanFieldSetting),
                nameof(BooleanSettings.BooleanPropertySetting),
            };
            foreach(Tuple<ExposeSettingAttribute, ExposedSettingBase> setting in settings)
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
        public void ExposedBooleanSetting_ExposeData_GetsField()
        {

            ExposedBooleanSetting field = new ExposedBooleanSetting(typeof(BooleanSettings), nameof(BooleanSettings.BooleanFieldSetting), "field", "", "");

            BooleanSettings.BooleanFieldSetting = false;
            bool expectedValue = BooleanSettings.BooleanFieldSetting;
            bool actual = field.Get<bool>();
            Assert.AreEqual(expectedValue, actual, "Retrieved the correct value from the static");

            BooleanSettings.BooleanFieldSetting = true;
            expectedValue = BooleanSettings.BooleanFieldSetting;
            actual = field.Get<bool>();
            Assert.AreEqual(expectedValue, actual, "Retrieved the correct value from the static after update");
        }

        /// <summary>
        /// Ensures that the value retrieved from the setting entry matches the underlying property
        /// </summary>
        [TestMethod]
        public void ExposedBooleanSetting_ExposeData_GetsProperty()
        {

            ExposedBooleanSetting prop = new ExposedBooleanSetting(typeof(BooleanSettings), nameof(BooleanSettings.BooleanPropertySetting), "prop", "", "");

            BooleanSettings.BooleanPropertySetting = false;
            bool expectedValue = BooleanSettings.BooleanPropertySetting;
            bool actual = prop.Get<bool>();
            Assert.AreEqual(expectedValue, actual, "Retrieved the correct value from the static");

            BooleanSettings.BooleanPropertySetting = true;
            expectedValue = BooleanSettings.BooleanPropertySetting;
            actual = prop.Get<bool>();
            Assert.AreEqual(expectedValue, actual, "Retrieved the correct value from the static after update");
        }

        /// <summary>
        /// Ensures that updating the setting entry actually updates the underlying field
        /// </summary>
        [TestMethod]
        public void ExposedBooleanSetting_ExposeData_SetsField()
        {

            ExposedBooleanSetting field = new ExposedBooleanSetting(typeof(BooleanSettings), nameof(BooleanSettings.BooleanFieldSetting), "field", "", "");

            BooleanSettings.BooleanFieldSetting = false;
            bool expectedValue = true;
            field.Set(expectedValue);
            bool actual = BooleanSettings.BooleanFieldSetting;
            Assert.AreEqual(expectedValue, actual, "Updated the value in the static");
        }

        /// <summary>
        /// Ensures that updating the setting entry actually updates the underlying property
        /// </summary>
        [TestMethod]
        public void ExposedBooleanSetting_ExposeData_SetsProperty()
        {

            ExposedBooleanSetting prop = new ExposedBooleanSetting(typeof(BooleanSettings), nameof(BooleanSettings.BooleanPropertySetting), "prop", "", "");

            BooleanSettings.BooleanPropertySetting = false;
            bool expectedValue = true;
            prop.Set(expectedValue);
            bool actual = BooleanSettings.BooleanPropertySetting;
            Assert.AreEqual(expectedValue, actual, "Updated the value in the static");
        }
    }

    #region bool
    /// <summary>
    /// A class that has boolean settings set up for testing
    /// </summary>
    internal class BooleanSettings
    {
        /// <summary>
        /// A field which should be a setting
        /// </summary>
        [BooleanSetting]
        public static bool BooleanFieldSetting = true;

        /// <summary>
        /// A field which should not be a setting
        /// </summary>
        public static bool BooleanFieldNonSetting = true;

        /// <summary>
        /// A property which should be a setting
        /// </summary>
        [BooleanSetting]
        public static bool BooleanPropertySetting { get; set; } = true;

        /// <summary>
        /// A property which should not be a setting
        /// </summary>
        public static bool BooleanPropertyNonSetting { get; set; } = true;
    }
    #endregion
}