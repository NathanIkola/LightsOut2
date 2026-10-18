using HarmonyLib;
using LightsOut2.Comps;
using LightsOut2.Comps.Properties;
using LightsOut2.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace LightsOut2.StandbyEffects
{
    /// <summary>
    /// A standby effect that handles disabling glowers
    /// </summary>
    public sealed class DisableGlowerEffect : StandbyEffectBase
    {
        #region main class
        public DisableGlowerEffect(ThingWithComps parent) 
            : base(parent) { }

        /// <summary>
        /// Updates the glower when the standby status changes
        /// </summary>
        /// <param name="wasInStandby">Whether the glower was in standby before</param>
        /// <param name="isInStandby">Whether the glower should be in standby now</param>
        protected override void OnStandbyChanged(bool wasInStandby, bool isInStandby)
        {
            UpdateGlower();
        }

        /// <summary>
        /// Captures the glower to update during the gameplay
        /// </summary>
        /// <param name="respawningAfterLoad">Whether or not this a respawn after load</param>
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            _glower = GetGlower(_parent);
        }

        /// <summary>
        /// Updates the glower so it picks up the current status
        /// </summary>
        private void UpdateGlower()
        {
            if (_glower is null)
            {
                LightsOut2Mod.StaticLogger.Warning($"DisableGlowerEffect on '{_parent.def.defName}' has no compatible glower component", true);
                return;
            }

            Map map = _glower.parent?.Map;
            if (map is null) { return; }

            if (_glower is CompGlower vanillaGlower)
            {
                vanillaGlower.UpdateLit(map);
                return;
            }

            // otherwise try updating lit on a modded glower
            MethodInfo updateLit = GetUpdateLitMethod(_glower.GetType());
            LightsOut2Mod.StaticLogger.Assert(updateLit != null, $"Failed to find UpdateLit method on type: {_glower.GetType()}", true);
            if (updateLit is null) { return; }
            try
            {
                int paramLength = updateLit.GetParameters().Length;
                if (paramLength == 0)
                {
                    updateLit.Invoke(_glower, null);
                }
                else if (paramLength == 1 && updateLit.GetParameters()[0].ParameterType == typeof(Map))
                {
                    updateLit.Invoke(_glower, new object[] { map });
                }
                else
                {
                    LightsOut2Mod.StaticLogger.Error($"Parameter error on type: {_glower.GetType()} - {updateLit.GetParameters()}");
                }
            }
            catch (Exception ex)
            {
                LightsOut2Mod.StaticLogger.Error($"Having trouble with modded glower '{_glower.GetType()}': {ex}");
            }
        }

        /// <summary>
        /// Retrieves the UpdateLit method for the given <paramref name="type"/>
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to check</param>
        /// <returns>The <see cref="MethodInfo"/> associated with the UpdateLit method of this type</returns>
        private MethodInfo GetUpdateLitMethod(Type type)
        {
            if (type is null) { return null; }
            if (UpdateLitMethods.TryGetValue(type, out MethodInfo cached)) { return cached; }
            MethodInfo method = type.GetMethod("UpdateLit", Flags);
            UpdateLitMethods.Add(type, method);
            return method;
        }

        /// <summary>
        /// Function for retrieving the glower off of a ThingWithComps
        /// </summary>
        /// <param name="thing">The <see cref="ThingWithComps"/> to check</param>
        /// <returns>The <see cref="ThingComp"/> for this building's glower comp</returns>
        private static ThingComp GetGlower(ThingWithComps thing)
        {
            ThingComp glower = thing.TryGetComp<CompGlower>();
            if (glower != null) { return glower; }

            // try to get modded glowers
            foreach (ThingComp comp in thing.AllComps)
                if (comp.GetType().Name.Contains("Glow")) { return comp; }

            return null;
        }

        /// <summary>
        /// The glower to update
        /// </summary>
        private ThingComp _glower;

        /// <summary>
        /// Dictionary of all types which have already been patched. Used to avoid re-patching the same glower multiple times.
        /// </summary>
        public static Dictionary<Type, MethodInfo> UpdateLitMethods = new Dictionary<Type, MethodInfo>();

        /// <summary>
        /// A good list of <see cref="BindingFlags"/> to use to get most things
        /// </summary>
        public const BindingFlags Flags = BindingFlags.Public
                            | BindingFlags.NonPublic
                            | BindingFlags.Instance
                            | BindingFlags.Static
                            | BindingFlags.FlattenHierarchy;
        #endregion

        #region patch the glower lit methods
        /// <summary>
        /// Patches all glowers in every def so that they are able to update
        /// </summary>
        [HarmonyPatch(typeof(ThingDef), nameof(ThingDef.PostLoad))]
        public class ThingDef_PostLoad
        {
            public static void Prefix(ThingDef __instance)
            {
                // don't need to patch anything that doesn't have this
                if (!HasDisableGlowerEffect(__instance)) { return; }
                PatchGlower(__instance);
            }

            /// <summary>
            /// Checks to see if the def has the disable glower effect attached
            /// </summary>
            /// <param name="def">The def to check</param>
            /// <returns>Whether or not the disable glower effect is attached</returns>
            private static bool HasDisableGlowerEffect(ThingDef def)
            {
                CompProperties_Standby standbyProps = GetStandbyProps(def);
                if (standbyProps?.standbyEffects is null) { return false; }

                foreach (Type effectType in standbyProps?.standbyEffects)
                {
                    if (effectType == typeof(DisableGlowerEffect))
                    {
                        return true;
                    }
                }
                return false;
            }

            /// <summary>
            /// Retrieves the standby props for the given def
            /// </summary>
            /// <param name="def">The <see cref="ThingDef"/> to look up</param>
            /// <returns>The associated <see cref="CompProperties_Standby"/> if present, <see langword="null"/> otherwise</returns>
            private static CompProperties_Standby GetStandbyProps(ThingDef def)
            {
                if (def.comps is null) { return null; }
                foreach (CompProperties props in def.comps)
                {
                    if (props is CompProperties_Standby standbyProps)
                    {
                        return standbyProps;
                    }
                }
                return null;
            }

            /// <summary>
            /// Patches the glower class attached to the def
            /// </summary>
            /// <param name="def">The def to patch</param>
            private static void PatchGlower(ThingDef def)
            {
                // check to see if we need to patch this type
                Type glowerClass = GetGlowerClass(def);
                if (glowerClass is null)
                {
                    return;
                }

                PropertyInfo shouldBeLitNowProp = GetShouldBeLitNowPropertyInfo(glowerClass);
                MethodInfo original = shouldBeLitNowProp?.GetMethod;
                MethodInfo patch = typeof(ThingDef_PostLoad).GetMethod(nameof(ShouldBeLitNowPatch), Flags);

                if (original is null || patch is null)
                {
                    LightsOut2Mod.StaticLogger.Warning($"Skipping glower patch for '{glowerClass}': no usable boolean ShouldBeLitNow property was found", true);
                    return;
                }

                if (_patchedGetters.Contains(original)) { return; }

                LightsOut2Mod.Instance.Harmony.Patch(original, null, new HarmonyMethod(patch));
                LightsOut2Mod.StaticLogger.Information($"Patching type \"{glowerClass}\" as a glower");
                _patchedGetters.Add(original);
            }

            /// <summary>
            /// Attempts to retrieve the glower class from a <paramref name="def"/>
            /// </summary>
            /// <param name="def">The <see cref="ThingDef"/> to check</param>
            /// <returns>The associated glower type</returns>
            public static Type GetGlowerClass(ThingDef def)
            {
                if (def?.comps is null) { return null; }
                foreach (CompProperties props in def.comps)
                {
                    if (props?.compClass?.Name.Contains("Glow") == true)
                    {
                        return props.compClass;
                    }
                }
                return null;
            }

            /// <summary>
            /// Attempt to look up the property to patch to affect glowing
            /// </summary>
            /// <param name="glowerClass">The class to get the property from</param>
            /// <returns>The property, or null if not found</returns>
            private static PropertyInfo GetShouldBeLitNowPropertyInfo(Type glowerClass)
            {
                // ensure this type is actually a comp
                if (!glowerClass.IsSubclassOf(typeof(ThingComp)))
                {
                    return null;
                }
                // most common ones first
                PropertyInfo prop = TryGetDeclaredProperty("ShouldBeLitNow", glowerClass)
                    ?? TryGetDeclaredProperty("shouldBeLitNow", glowerClass)
                    ?? TryGetDeclaredProperty("_ShouldBeLitNow", glowerClass);
                if (prop != null) { return prop; }

                // otherwise brute force trying to find one
                foreach (PropertyInfo candidate in glowerClass.GetProperties(Flags))
                {
                    if (candidate.Name.IndexOf("ShouldBeLitNow", StringComparison.OrdinalIgnoreCase) >= 0
                        && candidate.DeclaringType == candidate.ReflectedType
                        && candidate.PropertyType == typeof(bool)
                        && candidate.GetMethod != null)
                    { return candidate; }
                }
                return null;
            }

            /// <summary>
            /// Finds a supported property declared directly on the supplied type.
            /// </summary>
            /// <param name="propertyName">The property name to find.</param>
            /// <param name="from">The type to inspect.</param>
            /// <returns>The property when it has a boolean getter declared on the type.</returns>
            private static PropertyInfo TryGetDeclaredProperty(string propertyName, Type from)
            {
                PropertyInfo property = from?.GetProperty(propertyName, Flags);
                return property != null && property.DeclaringType == property.ReflectedType
                    && property.PropertyType == typeof(bool) && property.GetMethod != null ? property : null;
            }

            /// <summary>
            /// The patch for determining if a light should be lit, reacts to the standby comp
            /// </summary>
            /// <param name="__instance">The instance to check</param>
            /// <param name="__result">The result to modify</param>
            public static void ShouldBeLitNowPatch(ThingComp __instance, ref bool __result)
            {
                // if it's already false, ignore it
                if (!__result) { return; }

                StandbyComp standbyComp = __instance.parent.TryGetComp<StandbyComp>();
                if (standbyComp is null) { return; }

                // if the light is in standby, it shouldn't be lit
                if (standbyComp.InStandby) { __result = false; }
            }

            /// <summary>
            /// Getters that have already been patched.
            /// </summary>
            private static HashSet<MethodInfo> _patchedGetters = new HashSet<MethodInfo>();
        }
        #endregion
    }
}
