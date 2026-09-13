using RimWorld;
using Verse;

namespace LightsOut2.Gizmos
{
    /// <summary>
    /// A simple gizmo that allows the user to toggle a light to "keep on" mode
    /// </summary>
    public class KeepOnGizmo : Command_Toggle
    {
        public KeepOnGizmo()
        {
            defaultLabel = "Keep On";
            defaultDesc = "Prevent this light from turning off";
            icon = Widgets.GetIconFor(ThingDefOf.StandingLamp);
            isActive = () => KeepOn;
            toggleAction = () => KeepOn = !KeepOn;
            Order = 69420;
        }

        /// <summary>
        /// Whether or not this gizmo is currently in "keep on" mode
        /// </summary>
        public bool KeepOn { get; set; }
    }
}