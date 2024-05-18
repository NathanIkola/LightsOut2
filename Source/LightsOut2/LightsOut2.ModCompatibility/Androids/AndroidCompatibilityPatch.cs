using LightsOut2.Core.ModCompatibility;
using System.Collections.Generic;

namespace LightsOut2.ModCompatibility.Androids
{
    public class AndroidCompatibilityPatch : IModCompatibilityPatch
    {
        public override string CompatibilityPatchName => "Androids";
        public override string TargetMod => "ChJees.Androids14";

        public override IEnumerable<IModCompatibilityPatchComponent> GetComponents()
        {
            return new List<IModCompatibilityPatchComponent>() 
            {
                new UpdateStandby(),
            };
        }
    }
}