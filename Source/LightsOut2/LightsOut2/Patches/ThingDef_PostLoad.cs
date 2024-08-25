using HarmonyLib;
using LightsOut2.Common;
using LightsOut2.CompProperties;
using LightsOut2.Core.Debug;
using LightsOut2.ThingComps;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using System.Reflection;

namespace LightsOut2.Patches
{
    /// <summary>
    /// A patch that adds my power upgrade to objects with CompProperties_Power comps
    /// </summary>
    [HarmonyPatch(typeof(ThingDef), nameof(ThingDef.PostLoad))]
    public class ThingDef_PostLoad
    {
        /// <summary>
        /// The patch that adds the PowerComp to a ThingDef as they are loaded into the game
        /// </summary>
        /// <param name="__instance"></param>
        public static void Prefix(ThingDef __instance)
        {
            // no power props/power trader, nothing to do
            CompProperties_Power powerProps = GetPowerProps(__instance);
            if (powerProps is null) return;
            // already has a standby comp assigned by an XML patch
            CompProperties_Standby standbyProps = GetStandbyProps(__instance);
            if (standbyProps != null) return;

            bool startEnabled = true;
            bool isTable = __instance.IsTable();
            bool isLight = !isTable && __instance.IsLight();
            if (!isLight && !isTable)
                startEnabled = false;

            if (isLight) PatchGlower(__instance);

            // insert the additional StandbyComp
            __instance.comps.Add(new CompProperties_Standby
            {
                startEnabled = startEnabled,
                compClass = isLight 
                    ? typeof(StandbyLightComp) 
                    : typeof(StandbyComp)
            });
        }

        /// <summary>
        /// Given a def for a modded light, patch the glower associated with it
        /// </summary>
        /// <param name="def">The <see cref="ThingDef"/> to patch</param>
        public static void PatchGlower(ThingDef def)
        {
            Type glowerClass = def.GetGlowerClass();
            // only patch types once
            if (PatchedGlowerTypes.ContainsKey(glowerClass)) return;

            DebugLogger.AssertFalse(glowerClass is null, $"Failed to retrieve glower class from def \"{def}\"", true);
            PropertyInfo shouldBeLitNow = GetShouldBeLitNowPropertyInfo(glowerClass);
            bool declaresShouldBeLitNow = shouldBeLitNow.DeclaringType == shouldBeLitNow.ReflectedType;
            DebugLogger.Assert(declaresShouldBeLitNow, $"Class \"{glowerClass}\" does not define a ShouldBeLitNow property", true);
            if (!declaresShouldBeLitNow) { return; }

            MethodInfo original = shouldBeLitNow.GetMethod;
            MethodInfo patch = typeof(ThingDef_PostLoad).GetMethod(nameof(ShouldBeLitNowPatch), Utils.BindingFlags);
            LightsOut2Mod.HarmonyInstance.Patch(original, null, new HarmonyMethod(patch));
            DebugLogger.LogInfo($"Patching type \"{glowerClass}\" as a glower");
            PatchedGlowerTypes.Add(glowerClass, true);
        }

        /// <summary>
        /// Retrieves the ShouldBeLitNow getter for this glower
        /// </summary>
        /// <param name="glowerClass">The glower to retrieve it from</param>
        /// <returns></returns>
        public static PropertyInfo GetShouldBeLitNowPropertyInfo(Type glowerClass)
        {
            // most common ones first
            PropertyInfo prop = TryGetDeclaredProperty("ShouldBeLitNow", glowerClass)
                ?? TryGetDeclaredProperty("shouldBeLitNow", glowerClass)
                ?? TryGetDeclaredProperty("_ShouldBeLitNow", glowerClass);
            if (prop != null) return prop;

            foreach (PropertyInfo propInfo in glowerClass.GetProperties(Utils.BindingFlags))
                if (propInfo.Name.ToLower().Contains("shouldbelitnow") && prop.DeclaringType == prop.ReflectedType) 
                    return propInfo;
            return null;
        }

        /// <summary>
        /// Retrieves a declared property from the given type
        /// </summary>
        /// <param name="propertyName">The property to try to retrieve</param>
        /// <param name="from">The type to retrieve the property from</param>
        /// <returns>The PropertyInfo for the declared property, or null if one wasn't found</returns>
        private static PropertyInfo TryGetDeclaredProperty(string propertyName, Type from)
        {
            if (from is null) 
                return null;

            PropertyInfo prop = from.GetProperty(propertyName, Utils.BindingFlags);
            if (prop is null || prop.DeclaringType != prop.ReflectedType)
                return null;
            return prop;
        }

        /// <summary>
        /// A Harmony patch for forcing lights to appear dim when in standby
        /// </summary>
        /// <param name="__instance">The <see cref="ThingComp"/> assocaited with the glower to turn off</param>
        /// <param name="__result">Whether or not the glower should be lit</param>
        public static void ShouldBeLitNowPatch(ThingComp __instance, ref bool __result)
        {
            // if it's already false, ignore it
            if (!__result) return;

            StandbyLightComp standbyComp = __instance.parent?.TryGetComp<StandbyLightComp>();
            if (standbyComp is null) return;

            // if the light is in standby, it shouldn't be lit
            if (standbyComp.IsInStandby)
                __result = false;
        }

        /// <summary>
        /// Retrieves the power props for the given def
        /// </summary>
        /// <param name="def">The <see cref="ThingDef"/> to look up</param>
        /// <returns>The assocaited <see cref="CompProperties_Power"/> if present, <see langword="null"/> otherwise</returns>
        private static CompProperties_Power GetPowerProps(ThingDef def)
        {
            if (def.comps is null) return null;
            foreach (Verse.CompProperties props in def.comps)
                if (props is CompProperties_Power powerProps && powerProps.compClass == typeof(CompPowerTrader)) return powerProps;
            return null;
        }

        /// <summary>
        /// Retrieves the standby props for the given def
        /// </summary>
        /// <param name="def">The <see cref="ThingDef"/> to look up</param>
        /// <returns>The associated <see cref="CompProperties_Standby"/> if present, <see langword="null"/> otherwise</returns>
        private static CompProperties_Standby GetStandbyProps(ThingDef def)
        {
            if (def.comps is null) return null;
            foreach (Verse.CompProperties props in def.comps)
                if (props is CompProperties_Standby standbyProps)
                    return standbyProps;
            return null;
        }

        /// <summary>
        /// A dictionary of the glowers that have been patched by this mod
        /// </summary>
        public static Dictionary<Type, bool> PatchedGlowerTypes = new Dictionary<Type, bool>();
    }
}