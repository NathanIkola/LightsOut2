using System.Collections.Generic;
using Verse;

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
        /// Called when drawing the settings window for the mod, allows patches
        /// to add mod-specific settings to the window as needed
        /// </summary>
        /// <param name="listingStandard">The listing to add to</param>
        public virtual void OnDrawPatchSettings(Listing_Standard listingStandard) { }

        /// <summary>
        /// Allows patches to save/store settings via the mod's ExposeData method
        /// </summary>
        public virtual void OnPatchSettingsExposeData() { }

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