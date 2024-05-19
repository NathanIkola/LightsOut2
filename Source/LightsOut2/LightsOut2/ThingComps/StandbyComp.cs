using LightsOut2.CompProperties;
using LightsOut2.StandbyActuators;
using System;
using System.Collections.Generic;
using Verse;
using LightsOut2.Core.StandbyActuators;
using LightsOut2.Core.StandbyComps;

namespace LightsOut2.ThingComps
{
    /// <summary>
    /// This class allows us to mark certain power-consumers as "on standby"
    /// </summary>
    public class StandbyComp : IStandbyComp
    {
        /// <summary>
        /// Initialize the comp and set up the gizmo
        /// </summary>
        /// <param name="props">The properties for this comp</param>
        public override void Initialize(Verse.CompProperties props)
        {
            base.Initialize(props);
            if (props is CompProperties_Standby standbyProps)
            {
                IsEnabled = standbyProps.startEnabled;
                Type standbyActuatorType = standbyProps.standbyActuatorClass;
                if (standbyActuatorType is null)
                    standbyActuatorType = typeof(BenchStandbyActuator);
                StandbyActuator = Activator.CreateInstance(standbyActuatorType) as IStandbyActuator;
            }
        }

        /// <summary>
        /// Adds some info to the comp inspect string if in dev mode and god mode
        /// </summary>
        /// <returns>A string with some extra info, or nothing</returns>
        public override string CompInspectStringExtra()
        {
            if (!DebugSettings.ShowDevGizmos) return base.CompInspectStringExtra();
            return $"Standby: {IsInStandby}\n" +
                $"Enabled: {IsEnabled}\n" +
                $"Rate: {Rate}";
        }

        /// <summary>
        /// Backing field for IsInStandby
        /// </summary>
        private bool m_isInStandby = true;

        public override bool IsInStandby
        {
            get { return IsEnabled && m_isInStandby; }
            set 
            {
                if (m_isInStandby == value) return;
                m_isInStandby = value;

                // if we are toggling this, then it must be active
                if (!IsEnabled)
                    IsEnabled = true;

                RaiseOnStandbyChanged(value, false);
            }
        }

        /// <summary>
        /// Backing field for IsEnabled
        /// </summary>
        private bool m_isEnabled;

        public override bool IsEnabled
        {
            get => m_isEnabled;
            set => m_isEnabled = value;
        }

        /// <summary>
        /// A function for determining the rate given a specific standby state
        /// </summary>
        /// <param name="isInStandby">Whether or not the thing is in standby</param>
        /// <returns>The rate to modify the power draw by</returns>
        public override float GetRateAsStandbyStatus(bool isInStandby)
        {
            return isInStandby
                    ? LightsOut2Mod.StandbyPowerDrawDecimal
                    : LightsOut2Mod.ActivePowerDrawDecimal;
        }

        /// <summary>
        /// Saves the state of the comp
        /// </summary>
        public override void PostExposeData()
        {
            base.PostExposeData();
            bool isInStandby = IsInStandby;
            bool isEnabled = IsEnabled;
            Scribe_Values.Look(ref isInStandby, "IsInStandby", isInStandby);
            Scribe_Values.Look(ref isEnabled, "IsEnabled", isEnabled);
            IsInStandby = isInStandby;
            IsEnabled = isEnabled;

            RaiseOnStandbyChanged(IsInStandby, true);
        }

        /// <summary>
        /// Retrieves a list of the gizmos to display for this comp
        /// </summary>
        /// <returns>A list of applicable gizmos to display</returns>
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield break;
        }
    }
}