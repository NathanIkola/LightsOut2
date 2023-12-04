﻿using HarmonyLib;
using LightsOut2.Common;
using LightsOut2.CompProperties;
using LightsOut2.Debug;
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
            CompProperties_Power powerProps = GetPowerProps(__instance);
            if (powerProps is null) return;

            bool startEnabled = true;
            bool isLight = !__instance.IsTable() && __instance.IsLight();
            if (!isLight && !StandbyComp.ShouldStartEnabled(__instance))
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
            DebugLogger.Assert(shouldBeLitNow.DeclaringType == shouldBeLitNow.ReflectedType, $"Class \"{glowerClass}\" does not define a ShouldBeLitNow property", true);

            MethodInfo original = shouldBeLitNow.GetMethod;
            MethodInfo patch = typeof(ThingDef_PostLoad).GetMethod(nameof(ShouldBeLitNowPatch), BindingFlags);
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
            PropertyInfo prop = glowerClass.GetProperty("ShouldBeLitNow", BindingFlags)
                ?? glowerClass.GetProperty("shouldBeLitNow", BindingFlags)
                ?? glowerClass.GetProperty("_ShouldBeLitNow", BindingFlags);
            if (prop != null) return prop;

            foreach (PropertyInfo propInfo in glowerClass.GetProperties(BindingFlags))
                if (propInfo.Name.ToLower().Contains("shouldbelitnow")) return propInfo;
            return null;
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
            StandbyLightComp standbyComp = __instance.parent.TryGetComp<StandbyLightComp>();
            if (standbyComp is null)
            {
                DebugLogger.LogWarning($"Failed to find StandbyLightComp on type: {__instance.parent.def}");
                return;
            }

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
        /// A dictionary of the glowers that have been patched by this mod
        /// </summary>
        public static Dictionary<Type, bool> PatchedGlowerTypes = new Dictionary<Type, bool>();

        /// <summary>
        /// A good list of <see cref="BindingFlags"/> to use to get most things
        /// </summary>
        public readonly static BindingFlags BindingFlags = BindingFlags.Public
                            | BindingFlags.NonPublic
                            | BindingFlags.Instance
                            | BindingFlags.Static
                            | BindingFlags.FlattenHierarchy;
    }
}