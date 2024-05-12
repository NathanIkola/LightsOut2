using HarmonyLib;
using LightsOut2.Core.Debug;
using System;
using System.Collections.Generic;
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
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
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
            foreach (IModCompatibilityPatch patch in GetAllPatches())
                ApplyPatch(harmonyInstance, patch);
        }

        /// <summary>
        /// Applies a whole, single compatibility patch
        /// </summary>
        /// <param name="patch">The patch to apply</param>
        private static void ApplyPatch(Harmony harmonyInstance, IModCompatibilityPatch patch)
        {
            if (string.IsNullOrWhiteSpace(patch.CompatibilityPatchName))
            {
                DebugLogger.LogWarning($"encountered compatibility patch with empty name; skipping it.");
                return;
            }

            // only load this patch if the target mod is present and accounted for
            if (!string.IsNullOrEmpty(patch.TargetMod))
            {
                if (!LoadedModManager.RunningModsListForReading.Any(x => x.Name == patch.TargetMod))
                    return;
            }

            DebugLogger.LogInfo($"applying mod compatibility patch: {patch.CompatibilityPatchName}");
            patch.OnBeforePatchApplied();
            foreach (IModCompatibilityPatchComponent component in patch.GetComponents())
            {
                component.OnBeforeComponentApplied();
                ApplyPatchComponent(harmonyInstance, component);
                component.OnAfterComponentApplied();
            }
            patch.OnAfterPatchApplied();
        }

        /// <summary>
        /// Applies a single compatibility component
        /// </summary>
        /// <param name="comp">The component to apply</param>
        private static void ApplyPatchComponent(Harmony harmonyInstance, IModCompatibilityPatchComponent comp)
        {
            if (string.IsNullOrWhiteSpace(comp.ComponentName))
            {
                DebugLogger.LogWarning("encountered a compatibility component with an empty name; skipping it.");
                return;
            }

            if (string.IsNullOrWhiteSpace(comp.TypeNameToPatch))
            {
                DebugLogger.LogWarning($"encountered a compatibility component ({comp.ComponentName}) with an empty type to patch; skipping it.");
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
                    DebugLogger.LogInfo($"    component applied: {comp.ComponentName}");
                wasApplied = true;

                foreach (PatchInfo patch in comp.GetPatches(type))
                {
                    if (patch.method is null && patch.methodName is null)
                    {
                        DebugLogger.LogWarning($"    encountered a component with a null method; skipping it.");
                        continue;
                    }

                    if (patch.patch is null)
                    {
                        DebugLogger.LogWarning($"    encountered a component with a null patch; skipping it.");
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
                            DebugLogger.LogWarning($"    encountered an invalid patch type in component {comp.ComponentName}; skipping it.");
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