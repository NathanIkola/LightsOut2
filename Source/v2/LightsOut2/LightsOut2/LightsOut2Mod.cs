using jl08lib;
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
        /// Initializes the mod
        /// </summary>
        /// <param name="content">The mod content</param>
        public LightsOut2Mod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<LightsOut2Settings>();
        }
    }
}