using LightsOut2.Core.Debug;
using LightsOut2.Core.ExtensionMethods;
using LightsOut2.Core.ModCompatibility;
using LightsOut2.Core.StandbyComps;
using System;
using System.Collections.Generic;
using Verse;

namespace LightsOut2.ModCompatibility.QuestionableEthics
{
    public class Patch_Building_OrganVat : IModCompatibilityPatchComponent
    {
        public override string ComponentName => "Patch Organ Vat to actuate standby";

        public override string TypeNameToPatch => "Building_OrganVat";

        public override bool TargetsMultipleTypes => false;

        public override bool TypeNameIsExact => true;

        public override IEnumerable<PatchInfo> GetPatches(Type type)
        {
            List<PatchInfo> patches = new List<PatchInfo>();

            patches.Add(new PatchInfo()
            {
                methodName = "Notify_CraftingStarted",
                patch = GetMethod<Patch_Building_OrganVat>(nameof(Postfix)),
                patchType = PatchType.Postfix,
            });

            patches.Add(new PatchInfo()
            {
                methodName = "Notify_CraftingFinished",
                patch = GetMethod<Patch_Building_OrganVat>(nameof(Postfix)),
                patchType = PatchType.Postfix,
            });

            patches.Add(new PatchInfo()
            {
                methodName = "Notify_FillingStarted",
                patch = GetMethod<Patch_Building_OrganVat>(nameof(Postfix)),
                patchType = PatchType.Postfix,
            });

            patches.Add(new PatchInfo()
            {
                methodName = "Notify_ProductExtracted",
                patch = GetMethod<Patch_Building_OrganVat>(nameof(Postfix)),
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