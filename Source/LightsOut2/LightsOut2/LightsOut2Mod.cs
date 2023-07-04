using LightsOut2.Debug;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace LightsOut2
{
    /// <summary>
    /// The class responsible for initializing LightsOut2
    /// </summary>
    public class LightsOut2Mod : Mod
    {
        /// <summary>
        /// The currently loaded settings for the mod
        /// </summary>
        public LightsOut2Settings Settings { get; set; }

        /// <summary>
        /// Initializes the mod and loads the settings
        /// </summary>
        /// <param name="content"></param>
        public LightsOut2Mod(ModContentPack content)
            : base(content)
        {
            Settings = GetSettings<LightsOut2Settings>();
            // this string is hardcoded because the translation engine isn't loaded at this point
            // this also doesn't use the debug logger because it isn't a debug message and should always print
            Log.Message($"Initializing LightsOut 2 [{typeof(LightsOut2Mod).Assembly.GetName().Version}]");
        }
    }
}
