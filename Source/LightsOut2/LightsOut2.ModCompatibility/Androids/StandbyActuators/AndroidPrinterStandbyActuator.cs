using LightsOut2.Core.Debug;
using LightsOut2.Core.StandbyActuators;
using System.Reflection;
using Verse;
using IMCPC = LightsOut2.Core.ModCompatibility.IModCompatibilityPatchComponent;

namespace LightsOut2.ModCompatibility.Androids.StandbyActuators
{
    public class AndroidPrinterStandbyActuator : IStandbyActuator
    {
        public bool IsInStandby(ThingWithComps thing, Pawn pawn)
        {
            // don't keep trying, something is wrong
            if (s_failedToResolveCrafterStatus)
                return false;
            
            if (s_pawnCrafterStatus is null)
                s_pawnCrafterStatus = IMCPC.GetMethod(thing.GetType(), "PawnCrafterStatus");

            if (s_pawnCrafterStatus is null)
            {
                DebugLogger.LogWarning("Failed to resolve PawnCrafterStatus for the Android Printer actuator");
                s_failedToResolveCrafterStatus = true;
                return false;
            }

            int status = (int)s_pawnCrafterStatus.Invoke(thing.GetType(), null);
            return status != 2; // status of 2 is printing -- the only time it isn't in standby
        }

        public bool ReadyToRun(ThingWithComps thing)
        {
            return true;
        }

        /// <summary>
        /// The method used to inspect the pawn crafter status
        /// </summary>
        private static MethodInfo s_pawnCrafterStatus = null;

        /// <summary>
        /// Used as a flag to prevent repeated failed attempts to look up the crafter status
        /// </summary>
        private static bool s_failedToResolveCrafterStatus = false;
    }
}