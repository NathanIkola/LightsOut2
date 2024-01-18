using HarmonyLib;
using Verse;

namespace LightsOut2.Patches
{
    [HarmonyPatch(typeof(TickManager), nameof(TickManager.DoSingleTick))]
    public class TickManager_DoSingleTick
    {
        public static void Prefix()
        {
            OnTick?.Invoke();
        }

        /// <summary>
        /// The template for the OnTick handler methods
        /// </summary>
        public delegate void TickHandler();

        /// <summary>
        /// The event fired every time a tick happens
        /// </summary>
        public static event TickHandler OnTick;
    }
}