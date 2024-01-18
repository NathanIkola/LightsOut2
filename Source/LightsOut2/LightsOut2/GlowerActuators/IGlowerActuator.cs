using Verse;

namespace LightsOut2.GlowerActuators
{
    /// <summary>
    /// Interface that all glower actuators must extend
    /// </summary>
    public abstract class IGlowerActuator
    {
        /// <summary>
        /// Method used to update the given <paramref name="glower"/>
        /// </summary>
        /// <param name="glower">The glower to update</param>
        public abstract void UpdateLit(ThingComp glower);

        /// <summary>
        /// Actually updates the <paramref name="glower"/>'s lit status on the map
        /// </summary>
        /// <param name="glower">The glower to update</param>
        public virtual void OnStandbyChanged(ThingComp glower)
        {
            if (glower is null) return;
            if (!(glower.parent?.Map?.regionAndRoomUpdater?.Enabled ?? false)) return;
            UpdateLit(glower);
        }
    }
}