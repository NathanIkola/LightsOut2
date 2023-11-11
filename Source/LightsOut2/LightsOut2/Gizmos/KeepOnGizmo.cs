using RimWorld;
using Verse;

namespace LightsOut2.Gizmos
{
    /// <summary>
    /// The gizmo that allows us to keep a light on
    /// </summary>
    public class KeepOnGizmo : Command_Toggle
    {
        private bool m_keepOn;

        /// <summary>
        /// Whether or not this gizmo is set to on or off
        /// </summary>
        public bool KeepOn
        {
            get => m_keepOn;
            set
            {
                m_keepOn = value;
                OnKeepOnChanged?.Invoke(value);
            }
        }

        public KeepOnGizmo()
        {
            defaultLabel = "Gizmos_KeepOnLabel".Translate();
            defaultDesc = "Gizmos_KeepOnTooltip".Translate();
            icon = Widgets.GetIconFor(ThingDefOf.StandingLamp);
            isActive = () => KeepOn;
            toggleAction = () => KeepOn = !KeepOn;
            Order = 69420;
        }

        /// <summary>
        /// Handler for boolean change evens
        /// </summary>
        /// <param name="newValue">The new boolean value</param>
        public delegate void OnBoolChangedHandler(bool newValue);

        /// <summary>
        /// The event fired whenever the KeepOn value is changed
        /// </summary>
        public event OnBoolChangedHandler OnKeepOnChanged;
    }
}