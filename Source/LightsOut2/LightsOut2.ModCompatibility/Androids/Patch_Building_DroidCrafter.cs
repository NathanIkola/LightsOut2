using LightsOut2.Core.ExtensionMethods;
using LightsOut2.Core.ModCompatibility;
using LightsOut2.Core.StandbyComps;
using System;
using System.Collections.Generic;
using Verse;

namespace LightsOut2.ModCompatibility.Androids
{
    public class Patch_Building_DroidCrafter : IModCompatibilityPatchComponent
    {
        public override string ComponentName => "Patch Droid Crafter to actuate standby";

        public override string TypeNameToPatch => "Building_DroidCrafter";

        public override bool TargetsMultipleTypes => false;

        public override bool TypeNameIsExact => true;

        public override IEnumerable<PatchInfo> GetPatches(Type type)
        {
            List<PatchInfo> patches = new List<PatchInfo>();

            patches.Add(new PatchInfo()
            {
                methodName = "InitiatePawnCrafting",
                patch = GetMethod<Patch_Building_AndroidPrinter>(nameof(Postfix)),
                patchType = PatchType.Postfix,
            });

            patches.Add(new PatchInfo()
            {
                methodName = "FinishAction",
                patch = GetMethod<Patch_Building_AndroidPrinter>(nameof(Postfix)),
                patchType = PatchType.Postfix,
            });

            patches.Add(new PatchInfo()
            {
                methodName = "SpawnSetup",
                patch = GetMethod<Patch_Building_AndroidPrinter>(nameof(Postfix)),
                patchType = PatchType.Postfix,
            });

            return patches;
        }

        /// <summary>
        /// A patch that updates the standby from the comp's actuator
        /// </summary>
        /// <param name="__instance">The instance kicking this off</param>
        public static void Postfix(ThingWithComps __instance)
        {
            IStandbyComp standbyComp = __instance.GetStandbyComp();
            standbyComp?.UpdateStandbyFromActuator(null);
        }
    }
}