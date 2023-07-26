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
        /// Create and Initialize a StandbyComp, and add it to a ThingWithComps
        /// </summary>
        /// <param name="thing">The parent for this comp</param>
        /// <returns>The associated StandbyPowerUpgrade</returns>
        public static StandbyComp CreateOnThing(ThingWithComps thing)
        {
            CompProperties props = new CompProperties() { compClass = typeof(StandbyComp) };
            StandbyComp comp = new StandbyComp { parent = thing };
            thing.AllComps.Add(comp);
            comp.Initialize(props);

            return comp;
        }

        /// <summary>
        /// Initialize the comp and set up the gizmo
        /// </summary>
        /// <param name="props">The properties for this comp</param>
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            KeepOnGizmo = new KeepOnGizmo();
            OnStandbyChanged?.Invoke(IsInStandby);
            OnEnabledChanged?.Invoke(IsEnabled);
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
            get { return m_isInStandby; }
            set 
            {
                if (m_isInStandby == value) return;
                m_isInStandby = value;

                // if we are toggling this, then it must be active
                if (!IsEnabled)
                    IsEnabled = true;

                OnStandbyChanged?.Invoke(value);
            }
        }

        /// <summary>
        /// Backing field for IsEnabled
        /// </summary>
        private bool m_isEnabled;

        /// <summary>
        /// Whether or not this comp is even enabled
        /// </summary>
        public bool IsEnabled
        {
            get { return m_isEnabled; }
            set 
            {
                if (m_isEnabled == value) return;
                m_isEnabled = value;
                OnEnabledChanged?.Invoke(value);
            }
        }

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
            Scribe_Values.Look(ref KeepOnGizmo.KeepOn, "KeepOn", false, false);

            bool isInStandby = false;
            bool isEnabled = false;
            bool isLight = false;
            Scribe_Values.Look(ref isInStandby, "IsInStandby", false, false);
            Scribe_Values.Look(ref isEnabled, "IsEnabled", false, false);
            Scribe_Values.Look(ref isLight, "IsLight", false, false);
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

        /// <summary>
        /// Handler for when a boolean status changes
        /// </summary>
        /// <param name="value">The value it was changed to</param>
        public delegate void OnBoolChangedHandler(bool value);

        /// <summary>
        /// The event raised when the standby status changes
        /// </summary>
        public event OnBoolChangedHandler OnStandbyChanged;

        /// <summary>
        /// The event raised whtn the enabled status changes
        /// </summary>
        public event OnBoolChangedHandler OnEnabledChanged;
    }
}