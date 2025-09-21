using jl08lib;
using jl08lib.Logging;
using LightsOut2.Logging;
using LightsOut2.Patches;
using Verse;

namespace LightsOut2
{
    /// <summary>
    /// The actual mod for LightsOut 2
    /// </summary>
    public class LightsOut2Mod : ModBase
    {
        /// <summary>
        /// Retrieves an instance of this mod from the game
        /// </summary>
        public static LightsOut2Mod Instance => LoadedModManager.GetMod<LightsOut2Mod>();

        /// <summary>
        /// A static logger for the mod instance
        /// </summary>
        public static LoggerBase StaticLogger => Instance.Logger;

        /// <summary>
        /// A statics ticker to be used by any comp that needs to ensure ticking behavior
        /// </summary>s
        public static Ticker StaticTicker => Instance.Ticker;

        /// <summary>
        /// Initializes the mod
        /// </summary>
        /// <param name="content">The mod content</param>
        public LightsOut2Mod(ModContentPack content) 
            : base(content, new FilteredVerseLogger(content.Name))
        {
            Settings = GetSettings<LightsOut2Settings>();
            Ticker = new Ticker();
        }

        /// <summary>
        /// An instance of a global ticker that can be subscribed to
        /// </summary>
        public Ticker Ticker { get; set; }
    }
}