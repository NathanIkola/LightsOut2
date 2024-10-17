using LightsOut2.Core.ExtensionMethods;
using LightsOut2.Core.ModCompatibility;
using LightsOut2.Core.StandbyComps;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

namespace LightsOut2.ModCompatibility.HelixienGas
{
    public class Patch_CompResourceTrader_Consumption : IModCompatibilityPatchComponent
    {
        public override string TypeNameToPatch => "CompResourceTrader";

        public override bool TargetsMultipleTypes => false;

        public override bool TypeNameIsExact => true;

        public override string ComponentName => "Patch CompResourceTrader to respect StandbyComps";

        public override IEnumerable<PatchInfo> GetPatches(Type type)
        {
            PatchInfo patch = new PatchInfo
            {
                method = GetGetMethod(type, "Consumption"),
                patch = GetMethod<Patch_CompResourceTrader_Consumption>(nameof(Postfix)),
                patchType = PatchType.Postfix,
            };

            return new List<PatchInfo> { patch };
        }

        /// <summary>
        /// Actually takes care of patching the comp to react to the standby status
        /// </summary>
        /// <param name="__instance">The comp instance to patch</param>
        /// <param name="__result">The output of the patched method</param>
        public static void Postfix(ThingComp __instance, ref float __result)
        {
            // ignore it if it's already not pulling power
            if (__result <= 0) return;

            IStandbyComp standbyComp = __instance.parent?.GetStandbyComp();
            if (standbyComp is null) return;

            // detect VFE standby as well as LightsOut standby
            float standbyRate = DefaultStandbyRate(__instance);
            bool isInStandby = standbyComp.IsInStandby || (__result == standbyRate && !Mathf.Approximately(standbyRate, -1));
            __result *= standbyComp.GetRateAsStandbyStatus(isInStandby);
        }

        /// <summary>
        /// Retrieves the standby draw rate for this comp's properties
        /// </summary>
        /// <param name="comp">The comp to pull from</param>
        /// <returns>The standby rate</returns>
        private static float DefaultStandbyRate(ThingComp comp)
        {
            if (comp is null) return 0f;

            Type compType = comp.GetType();
            FieldInfo idleFieldInfo = compType.GetField("idleConsumptionPerTick", BindingFlags);
            if (idleFieldInfo is null) return 0f;

            return (float)idleFieldInfo.GetValue(comp);
        }
    }
}