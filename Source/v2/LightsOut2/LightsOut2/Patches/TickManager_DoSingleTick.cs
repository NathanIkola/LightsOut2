using HarmonyLib;
using Verse;

namespace LightsOut2.Patches
{
    /// <summary>
    /// A class that ensures that ticking behavior happens regularly
    /// </summary>
    [HarmonyPatch(typeof(TickManager), nameof(TickManager.DoSingleTick))]
    public class TickManager_DoSingleTick
    {
        /// <summary>
        /// Invokes the ticker on every tick
        /// </summary>
        public static void Postfix()
        {
            LightsOut2Mod.StaticTicker.InvokeTick();
        }
    }

    /// <summary>
    /// A class that can be subscribed to as a means of ensuring ticking
    /// </summary>
    public class Ticker
    {
        /// <summary>
        /// A delegate for the function that should be called on ticking
        /// </summary>
        public delegate void TickHandler();

        /// <summary>
        /// An event that is invoked once every tick
        /// </summary>
        public event TickHandler OnTick;

        /// <summary>
        /// Allows other objects to invoke tick logic
        /// </summary>
        public void InvokeTick()
        {
            OnTick?.Invoke();
        }
    }
}
