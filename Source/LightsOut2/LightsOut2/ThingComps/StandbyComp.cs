﻿using LightsOut2.Common;
using LightsOut2.CompProperties;
using LightsOut2.Patches;
using LightsOut2.StandbyActuators;
using System;
using System.Collections.Generic;
using Verse;

namespace LightsOut2.ThingComps
{
    /// <summary>
    /// This class allows us to mark certain power-consumers as "on standby"
    /// </summary>
    public class StandbyComp : ThingComp
    {
        /// <summary>
        /// Determines if the comp should be enabled by default
        /// </summary>
        /// <param name="def">The ThingDef to inspect</param>
        public static bool ShouldStartEnabled(ThingDef def)
        {
            return def.IsTable();
        }

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

        /// <summary>
        /// Whether or not the owner of this comp is currently in standby
        /// </summary>
        public virtual bool IsInStandby
        {
            get { return IsEnabled && m_isInStandby; }
            set 
            {
                if (m_isInStandby == value) return;
                m_isInStandby = value;

                // if we are toggling this, then it must be active
                if (!IsEnabled)
                    IsEnabled = true;

                RaiseOnStandbyChanged(value);
            }
        }

        /// <summary>
        /// Backing field for IsEnabled
        /// </summary>
        private bool m_isEnabled;

        /// <summary>
        /// Whether or not this comp is even enabled
        /// </summary>
        public virtual bool IsEnabled
        {
            get => m_isEnabled;
            set => m_isEnabled = value;
        }

        /// <summary>
        /// Calculates the rate to modify the power draw by
        /// </summary>
        public float Rate
        {
            get
            {
                // if it's not enabled, don't modify anything
                if (!IsEnabled) return 1f;
                // otherwise benches are subject to the standby/active rates from the settings
                return GetRateAsStandbyStatus(IsInStandby);
            }
        }

        /// <summary>
        /// A function for determining the rate given a specific standby state
        /// </summary>
        /// <param name="isInStandby">Whether or not the thing is in standby</param>
        /// <returns>The rate to modify the power draw by</returns>
        public virtual float GetRateAsStandbyStatus(bool isInStandby)
        {
            return isInStandby
                    ? LightsOut2Settings.StandbyPowerDraw / 100f
                    : LightsOut2Settings.ActivePowerDraw / 100f;
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

            RaiseOnStandbyChanged(IsInStandby);
        }

        /// <summary>
        /// Retrieves a list of the gizmos to display for this comp
        /// </summary>
        /// <returns>A list of applicable gizmos to display</returns>
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield break;
        }

        /// <summary>
        /// A delegate for the boolean change events
        /// </summary>
        /// <param name="newValue">The new value of the boolean</param>
        public delegate void OnBoolChangedHandler(bool newValue);

        /// <summary>
        /// The event invoked when the standby status of a thing changes
        /// </summary>
        public event OnBoolChangedHandler OnStandbyChanged;

        /// <summary>
        /// A method that allows derived classes to raise this event
        /// </summary>
        /// <param name="newValue">The new standby value</param>
        protected void RaiseOnStandbyChanged(bool newValue)
        {
            OnStandbyChanged?.Invoke(newValue);
        }

        /// <summary>
        /// Updates this comp's standby state
        /// </summary>
        /// <param name="pawn">The <see cref="Pawn"/> doing the actuation</param>
        /// <remarks>
        /// Not used by lights because it would be really inefficient to check the room
        /// once for each light to have them determine if they're in standby or not. It's
        /// much easier to check it for a single pawn and apply that result to any lights.
        /// </remarks>
        public virtual void UpdateStandbyFromActuator(Pawn pawn)
        {
            if (StandbyActuator is null) return;
            if (StandbyActuator.ReadyToRun(parent))
                IsInStandby = StandbyActuator.IsInStandby(parent, pawn);
            // if the actuator is not quite ready to run, defer it until it is ready
            else
                TickManager_DoSingleTick.AddDeferredTask(
                    () => StandbyActuator.ReadyToRun(parent),
                    () => IsInStandby = StandbyActuator.IsInStandby(parent, pawn));
        }

        /// <summary>
        /// The actuator used when turning this thing on or off
        /// </summary>
        public IStandbyActuator StandbyActuator { get; set; }
    }
}