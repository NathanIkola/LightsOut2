using Verse;

namespace LightsOut2.Comps
{
    /// <summary>
    /// A class that handles the ticking needs
    /// </summary>
    public sealed class TickerGameComp : GameComponent
    {
        /// <summary>
        /// Constructor required by the game to initialize the component
        /// </summary>
        /// <param name="_">The game this is being initialized for</param>
        public TickerGameComp(Game _) { }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
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