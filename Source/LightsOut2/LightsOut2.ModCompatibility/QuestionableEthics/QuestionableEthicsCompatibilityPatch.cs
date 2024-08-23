using LightsOut2.Core.ModCompatibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightsOut2.ModCompatibility.QuestionableEthics
{
    public class QuestionableEthicsCompatibilityPatch : IModCompatibilityPatch
    {
        public override string CompatibilityPatchName => "Questionable Ethics";
        public override string TargetMod => "Mlie.QuestionableEthicsEnhanced";

        public override IEnumerable<IModCompatibilityPatchComponent> GetComponents()
        {
            return new List<IModCompatibilityPatchComponent>
            {
                new Patch_Building_OrganVat(),
                new Patch_Building_PawnVatGrower(),
                new Patch_Building_VatGrower(),
            };
        }
    }
}