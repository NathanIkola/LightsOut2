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

            OnStandbyChanged?.Invoke(m_isInStandby);
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
                m_isInStandby = value;
                OnStandbyChanged?.Invoke(value);
            }
        }

        /// <summary>
        /// Whether or not the owner of this comp is a light
        /// (as determined by LightsOut 2, at least)
        /// </summary>
        public bool IsLight;

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
            bool isInStandby = false;
            Scribe_Values.Look(ref isInStandby, "IsInStandby", false, false);
            IsInStandby = isInStandby;
            Scribe_Values.Look(ref IsLight, "IsLight", false, false);
            Scribe_Values.Look(ref KeepOnGizmo.KeepOn, "KeepOn", false, false);
        }

        /// <summary>
        /// Retrieves a list of the gizmos to display for this comp
        /// </summary>
        /// <returns>A list of applicable gizmos to display</returns>
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            //if (!IsLight) yield break;
            yield return KeepOnGizmo;
        }

        /// <summary>
        /// Handler for when the standby status changes
        /// </summary>
        /// <param name="isInStandby">Whether or not this is curently in standby mode</param>
        public delegate void OnStandbyChangedHandler(bool isInStandby);

        /// <summary>
        /// The event raised when the standby status changes
        /// </summary>
        public event OnStandbyChangedHandler OnStandbyChanged;
    }
}