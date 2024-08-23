using LightsOut2.Core.Debug;
using LightsOut2.Core.StandbyActuators;
using System.Reflection;
using Verse;
using IMCPC = LightsOut2.Core.ModCompatibility.IModCompatibilityPatchComponent;

namespace LightsOut2.ModCompatibility.QuestionableEthics.StandbyActuators
{
    public class VatStandbyActuator : IStandbyActuator
    {
        public bool IsInStandby(ThingWithComps thing, Pawn pawn)
        {
            if (m_failedToResolveCraftingStatus)
                return false;

            if (m_craftingStatus is null)
                m_craftingStatus = IMCPC.GetField(thing.GetType(), "status");

            if (m_craftingStatus is null)
            {
                DebugLogger.LogWarning("Failed to resolve status for the Vat actuator");
                m_failedToResolveCraftingStatus = true;
                return false;
            }

            int status = (int)m_craftingStatus.GetValue(thing);
            // a status of 0 means it's idle (based on an enum in QEthics)
            return status == 0;
        }

        public bool ReadyToRun(ThingWithComps thing)
        {
            return true;
        }

        /// <summary>
        /// The method used to inspect the crafting status
        /// </summary>
        private FieldInfo m_craftingStatus = null;

        /// <summary>
        /// Used as a flag to prevent repeated failed attempts to look up the crafter status
        /// </summary>
        private bool m_failedToResolveCraftingStatus = false;
    }
}