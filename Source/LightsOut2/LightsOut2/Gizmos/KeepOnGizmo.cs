using RimWorld;
using Verse;

namespace LightsOut2.Gizmos
{
    /// <summary>
    /// The gizmo that allows us to keep a light on
    /// </summary>
    public class KeepOnGizmo : Command_Toggle
    {
        /// <summary>
        /// Whether or not this gizmo is set to on or off
        /// </summary>
        public bool KeepOn;

        public KeepOnGizmo()
        {
            defaultLabel = "Gizmos_KeepOnLabel".Translate();
            defaultDesc = "Gizmos_KeepOnTooltip".Translate();
            icon = Widgets.GetIconFor(ThingDefOf.StandingLamp);
            isActive = () => KeepOn;
            toggleAction = () => KeepOn = !KeepOn;
            Order = 420;
        }
    }
}