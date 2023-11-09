﻿using LightsOut2.Common;
using LightsOut2.Gizmos;
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
        /// <param name="thing">The ThingWithComps to inspect</param>
        public static bool ShouldStartEnabled(ThingWithComps thing)
        {
            return thing.IsTable();
        }

        /// <summary>
        /// Initialize the comp and set up the gizmo
        /// </summary>
        /// <param name="props">The properties for this comp</param>
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            KeepOnGizmo = new KeepOnGizmo();
            if (!IsLight) IsLight = parent.IsLight();
            if (IsLight || ShouldStartEnabled(parent)) IsEnabled = true;
        }

        /// <summary>
        /// Backing field for IsInStandby
        /// </summary>
        private bool m_isInStandby;

        /// <summary>
        /// Whether or not the owner of this comp is currently in standby
        /// </summary>
        public bool IsInStandby
        {
            get { return !KeepOn && m_isInStandby; }
            set 
            {
                if (m_isInStandby == value) return;
                m_isInStandby = value;

                // if we are toggling this, then it must be active
                if (!IsEnabled)
                    IsEnabled = true;
            }
        }

        /// <summary>
        /// Whether or not this comp is even enabled
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Backing field for IsLight
        /// </summary>
        private bool m_isLight;

        /// <summary>
        /// Whether or not the owner of this comp is a light
        /// (as determined by LightsOut 2, at least)
        /// </summary>
        public bool IsLight
        {
            get { return m_isLight; }
            set
            {
                if (m_isLight == value) return;
                m_isLight = value;
                if (value) IsEnabled = true;
            }
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
                // lights are either on or off
                if (IsLight) return IsInStandby ? 0f : 1f;
                // otherwise benches are subject to the standby/active rates from the settings
                return IsInStandby 
                    ? LightsOut2Settings.StandbyPowerDraw 
                    : LightsOut2Settings.ActivePowerDraw;
            }
        }

        /// <summary>
        /// Whether or not this light is being kept on
        /// </summary>
        public bool KeepOn => KeepOnGizmo.KeepOn;

        /// <summary>
        /// The gizmo that allows us to keep a light on
        /// </summary>
        public KeepOnGizmo KeepOnGizmo { get; set; }

        /// <summary>
        /// Saves the state of the comp
        /// </summary>
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref KeepOnGizmo.KeepOn, "KeepOn", false);

            bool isInStandby = IsInStandby;
            bool isEnabled = IsEnabled;
            bool isLight = IsLight;
            Scribe_Values.Look(ref isInStandby, "IsInStandby", false);
            Scribe_Values.Look(ref isEnabled, "IsEnabled", false);
            Scribe_Values.Look(ref isLight, "IsLight", false);
            IsInStandby = isInStandby;
            IsEnabled = isEnabled;
            IsLight = isLight;
        }

        /// <summary>
        /// Retrieves a list of the gizmos to display for this comp
        /// </summary>
        /// <returns>A list of applicable gizmos to display</returns>
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (!IsEnabled || !IsLight) yield break;
            yield return KeepOnGizmo;
        }
    }
}