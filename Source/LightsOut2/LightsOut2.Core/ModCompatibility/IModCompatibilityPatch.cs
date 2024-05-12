using System.Collections.Generic;

namespace LightsOut2.Core.ModCompatibility
{
    /// <summary>
    /// The parent of a set of compatibility patches to apply
    /// solely dependent upon whether a mod is loaded or not
    /// </summary>
    public abstract class IModCompatibilityPatch
    {
        /// <summary>
        /// Called immediately before a patch is applied.
        /// This is not called unless the patch is actually being applied.
        /// </summary>
        public virtual void OnBeforePatchApplied() { }

        /// <summary>
        /// Called immediately after a patch is applied.
        /// </summary>
        public virtual void OnAfterPatchApplied() { }

        /// <summary>
        /// Returns a list of compatibility components to apply
        /// </summary>
        /// <returns>A list of compatibility components to apply</returns>
        public virtual IEnumerable<IModCompatibilityPatchComponent> GetComponents()
        {
            return new List<IModCompatibilityPatchComponent>();
        }

        /// <summary>
        /// The mod that causes this patch to take effect,
        /// or <see langword="null"/> to always apply it
        /// </summary>
        public virtual string TargetMod => null;

        /// <summary>
        /// The name to display in the console for this patch
        /// </summary>
        public abstract string CompatibilityPatchName { get; }
    }
}