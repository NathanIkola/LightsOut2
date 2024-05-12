using System;
using System.Collections.Generic;
using System.Reflection;

namespace LightsOut2.Core.ModCompatibility
{
    /// <summary>
    /// The types of patches you can apply.
    /// Goes along with the Harmony patches, but
    /// doesn't currently support transpilers
    /// </summary>
    public enum PatchType
    {
        Prefix,
        Postfix
    }

    /// <summary>
    /// A structure holding all of the information about
    /// a singular patch. Equivalent to a single
    /// Harmony patch
    /// </summary>
    public struct PatchInfo
    {
        public MethodInfo method;
        public string methodName;
        public MethodInfo patch;
        public PatchType patchType;
    }

    /// <summary>
    /// A singular patch action inside of a compatibility patch.
    /// This would be something equivalent to a file that contains
    /// multiple Harmony patches for a single type
    /// </summary>
    public abstract class IModCompatibilityPatchComponent
    {
        /// <summary>
        /// The type that this compatibility component is trying to patch
        /// </summary>
        public abstract string TypeNameToPatch { get; }

        /// <summary>
        /// Whether or not this component should be applied to multiple types or just one.
        /// This should be <see langword="false"/> unless your patch specifically intends 
        /// to handle different types
        /// </summary>
        public abstract bool TargetsMultipleTypes { get; }

        /// <summary>
        /// Whether or not the type name specified is exactly correct. If this 
        /// is <see langword="false"/> then it will search for all types that
        /// simply contain the name
        /// </summary>
        public abstract bool TypeNameIsExact { get; }

        /// <summary>
        /// Whether or not to ignore case differences when matching type names
        /// </summary>
        public virtual bool CaseSensitive => true;

        /// <summary>
        /// The name to display in the console when this component is applied
        /// </summary>
        public abstract string ComponentName { get; }

        /// <summary>
        /// Returns the list of patches to apply in this component.
        /// </summary>
        /// <param name="type">The type being patched</param>
        /// <returns>A list of patches to apply</returns>
        public abstract IEnumerable<PatchInfo> GetPatches(Type type);

        /// <summary>
        /// Easier shortcut to getting the MethodInfo from a type
        /// </summary>
        /// <typeparam name="TypeName">The type to get the method from</typeparam>
        /// <param name="methodName">The method to get</param>
        /// <returns>A <see cref="MethodInfo"/> object for <paramref name="methodName"/>, 
        /// or <see langword="null"/> if it doesn't exist</returns>
        public static MethodInfo GetMethod<TypeName>(string methodName)
        {
            return GetMethod(typeof(TypeName), methodName);
        }

        /// <summary>
        /// Easier shortcut to getting the MethodInfo from a type
        /// </summary>
        /// <param name="type">The type to get the method from</param>
        /// <param name="methodName">The method to get</param>
        /// <returns>A <see cref="MethodInfo"/> object for <paramref name="methodName"/>, 
        /// or <see langword="null"/> if it doesn't exist</returns>
        public static MethodInfo GetMethod(Type type, string methodName)
        {
            return type.GetMethod(methodName, BindingFlags);
        }

        /// <summary>
        /// Called immediately before a component is applied.
        /// </summary>
        public virtual void OnBeforeComponentApplied() { }

        /// <summary>
        /// Called immediately after a component is applied.
        /// </summary>
        public virtual void OnAfterComponentApplied() { }

        /// <summary>
        /// A good list of <see cref="BindingFlags"/> to use to get most things
        /// </summary>
        public readonly static BindingFlags BindingFlags = BindingFlags.Public
                            | BindingFlags.NonPublic
                            | BindingFlags.Instance
                            | BindingFlags.Static
                            | BindingFlags.FlattenHierarchy;
    }

    /// <summary>
    /// Overload of <see cref="ICompatibilityPatch"/> that automatically
    /// fills out some patch information if you give it a type to work with
    /// </summary>
    /// <typeparam name="TypeName"></typeparam>
    public abstract class IModCompatibilityPatchComponent<TypeName> : IModCompatibilityPatchComponent
    {
        public override string TypeNameToPatch => typeof(TypeName).Name;
        public override bool TargetsMultipleTypes => false;
        public override bool TypeNameIsExact => true;
    }
}
