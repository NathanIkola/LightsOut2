using HarmonyLib;
using LightsOut2.Core.Debug;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Verse;

namespace LightsOut2.Core.ModCompatibility
{
    /// <summary>
    /// The class responsible for loading any mod compatibility patches
    /// that are present, even ones from outside of LightsOut2
    /// </summary>
    [StaticConstructorOnStartup]
    public static class ModCompatibilityManager
    {
        /// <summary>
        /// The constructor that actually performs the loading
        /// </summary>
        static ModCompatibilityManager()
        {

        }

        /// <summary>
        /// Loop over all assemblies currently loaded to find all of the mod compatibility patches automatically
        /// </summary>
        /// <returns>A list of the compatibility patch objects</returns>
        private static IEnumerable<IModCompatibilityPatch> GetAllPatches()
        {
            foreach (ModContentPack modContentPack in LoadedModManager.RunningMods)
                foreach (Assembly assembly in modContentPack.assemblies.loadedAssemblies)
                    foreach (Type type in assembly.GetTypes())
                        if (type.IsSubclassOf(typeof(IModCompatibilityPatch)))
                        {
                            IModCompatibilityPatch patch = null;
                            try { patch = Activator.CreateInstance(type) as IModCompatibilityPatch; }
                            catch (Exception ex) { DebugLogger.LogWarning($"Failed to instantiate mod compatibility patch of type {type}: {ex.Message}"); }
                            if (patch != null)
                                yield return patch;
                        }
            yield break;
        }

        /// <summary>
        /// The method that actually iterates over the patches and applies them
        /// </summary>
        public static void LoadCompatibilityPatches(Harmony harmonyInstance)
        {
            DebugLogger.LogInfo("Searching for compatibility patches...");
            Stopwatch watch = new Stopwatch();
            watch.Start();
            int patchCount = 0;
            int appliedPatches = 0;
            foreach (IModCompatibilityPatch patch in GetAllPatches())
            {
                ++patchCount;
                if (ApplyPatch(harmonyInstance, patch))
                    ++appliedPatches;
            }
            watch.Stop();
            DebugLogger.LogInfo($"Found {patchCount} compatibility patche(s) in {watch.Elapsed.Seconds} seconds, applied {appliedPatches} patche(s)");
        }

        /// <summary>
        /// Applies a whole, single compatibility patch
        /// </summary>
        /// <param name="patch">The patch to apply</param>
        /// <returns><see langword="true"/> if the patch was applied, <see langword="false"/> otherwise</returns>
        private static bool ApplyPatch(Harmony harmonyInstance, IModCompatibilityPatch patch)
        {
            if (string.IsNullOrWhiteSpace(patch.CompatibilityPatchName))
            {
                DebugLogger.LogWarning($"Encountered compatibility patch with empty name; skipping it.");
                return false;
            }

            // only load this patch if the target mod is present and accounted for
            if (!string.IsNullOrEmpty(patch.TargetMod))
            {
                if (!LoadedModManager.RunningModsListForReading.Any(x => (x.Name == patch.TargetMod) || (x.PackageId == patch.TargetMod) || (x.PackageIdPlayerFacing == patch.TargetMod)))
                    return false;
            }

            DebugLogger.LogInfo($"Applying mod compatibility patch: {patch.CompatibilityPatchName}");
            patch.OnBeforePatchApplied();
            foreach (IModCompatibilityPatchComponent component in patch.GetComponents())
            {
                component.OnBeforeComponentApplied();
                ApplyPatchComponent(harmonyInstance, component);
                component.OnAfterComponentApplied();
            }
            patch.OnAfterPatchApplied();
            return true;
        }

        /// <summary>
        /// Applies a single compatibility component
        /// </summary>
        /// <param name="comp">The component to apply</param>
        private static void ApplyPatchComponent(Harmony harmonyInstance, IModCompatibilityPatchComponent comp)
        {
            if (string.IsNullOrWhiteSpace(comp.ComponentName))
            {
                DebugLogger.LogWarning("Encountered a compatibility component with an empty name; skipping it.");
                return;
            }

            if (string.IsNullOrWhiteSpace(comp.TypeNameToPatch))
            {
                DebugLogger.LogWarning($"Encountered a compatibility component ({comp.ComponentName}) with an empty type to patch; skipping it.");
                return;
            }

            bool wasApplied = false;

            // get all patchable types from loaded mods
            IEnumerable<Type> typesToPatch = GetTypesToPatch();

            foreach (Type type in typesToPatch)
            {
                // rule out types
                if (comp.TypeNameIsExact && !type.Name.Equals(comp.TypeNameToPatch))
                    continue;
                else if (comp.CaseSensitive && !type.Name.Contains(comp.TypeNameToPatch))
                    continue;
                else if (!comp.CaseSensitive && !type.Name.ToLower().Contains(comp.TypeNameToPatch.ToLower()))
                    continue;

                if (!wasApplied)
                    DebugLogger.LogInfo($"    Component applied: {comp.ComponentName}");
                wasApplied = true;

                foreach (PatchInfo patch in comp.GetPatches(type))
                {
                    if (patch.method is null && patch.methodName is null)
                    {
                        DebugLogger.LogWarning($"    Encountered a component with a null method; skipping it.");
                        continue;
                    }

                    if (patch.patch is null)
                    {
                        DebugLogger.LogWarning($"    Encountered a component with a null patch; skipping it.");
                        continue;
                    }


                    switch (patch.patchType)
                    {
                        case PatchType.Prefix:
                            harmonyInstance.Patch(GetMethod(type, patch), new HarmonyMethod(patch.patch));
                            break;
                        case PatchType.Postfix:
                            harmonyInstance.Patch(GetMethod(type, patch), null, new HarmonyMethod(patch.patch));
                            break;
                        default:
                            DebugLogger.LogWarning($"    Encountered an invalid patch type in component {comp.ComponentName}; skipping it.");
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Get all types from loaded mods
        /// </summary>
        /// <returns>A list of types to patch</returns>
        private static IEnumerable<Type> GetTypesToPatch()
        {
            List<Assembly> assemblies = new List<Assembly>() { Assembly.GetAssembly(typeof(Pawn)) };
            foreach (ModContentPack mod in LoadedModManager.RunningModsListForReading)
            {
                assemblies.AddRange(mod.assemblies.loadedAssemblies);
            }

            List<Type> patchableTypes = new List<Type>();
            foreach (Assembly assembly in assemblies)
            {
                try
                {
                    Type[] types = assembly.GetTypes();
                    if (types is null) continue;
                    patchableTypes.AddRange(types);
                }
                catch (ReflectionTypeLoadException) { }
            }

            return patchableTypes;
        }

        /// <summary>
        /// Gets the method out of a type when given
        /// a <see cref="PatchInfo"/> object
        /// </summary>
        /// <param name="type">The type to patch</param>
        /// <param name="patch">The patch being applied to <paramref name="type"/></param>
        /// <returns></returns>
        static MethodInfo GetMethod(Type type, PatchInfo patch)
        {
            if (patch.method != null) return patch.method;

            return IModCompatibilityPatchComponent.GetMethod(type, patch.methodName);
        }
    }
}