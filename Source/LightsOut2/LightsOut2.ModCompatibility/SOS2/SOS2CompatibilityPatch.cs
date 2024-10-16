using LightsOut2.Core.ModCompatibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightsOut2.ModCompatibility.SOS2
{
    public class SOS2CompatibilityPatch : IModCompatibilityPatch
    {
        public override string CompatibilityPatchName => "SOS2";

        public override string TargetMod => "kentington.saveourship2";

        public override IEnumerable<IModCompatibilityPatchComponent> GetComponents()
        {
            return new List<IModCompatibilityPatchComponent>()
            {
                new Patch_ShipInteriorMod2_MoveShip(),
            };
        }
    }
}