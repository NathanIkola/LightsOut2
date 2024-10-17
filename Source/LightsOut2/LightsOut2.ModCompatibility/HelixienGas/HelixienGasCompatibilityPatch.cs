using LightsOut2.Core.ModCompatibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightsOut2.ModCompatibility.HelixienGas
{
    public class HelixienGasCompatibilityPatch : IModCompatibilityPatch
    {
        public override string CompatibilityPatchName => "Helixien Gas";

        public override string TargetMod => "VanillaExpanded.HelixienGas";

        public override IEnumerable<IModCompatibilityPatchComponent> GetComponents()
        {
            return new List<IModCompatibilityPatchComponent>()
            {
                new Patch_CompResourceTrader_Consumption(),
            };
        }
    }
}