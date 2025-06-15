using HarmonyLib;
using jl08lib.Logging;
using jl08lib.Settings;
using System.Reflection;
using UnityEngine;
using Verse;

namespace jl08lib
{
    /// <summary>
    /// A base class used to create mods
    /// </summary>
    public abstract class ModBase : Mod
    {
        /// <summary>
        /// Instantiates an instance of the mod
        /// </summary>
        /// <param name="content">The mod content pack</param>
        public ModBase(ModContentPack content)
            : this(content, new VerseLogger(content.Name)) { }

        /// <summary>
        /// Instantiates the mod
        /// </summary>
        /// <param name="content">The mod content pack</param>
        /// <param name="logger">The logger to use</param>
        public ModBase(ModContentPack content, LoggerBase logger)
            : base(content)
        {
            Logger = logger;
            Log.Message($"Initializing {content.Name}");

            // do Harmony patching
            Harmony = new Harmony(content.Name);
            foreach (Assembly asm in content.assemblies.loadedAssemblies)
            {
                Harmony.PatchAll(asm);
            }
        }

        /// <summary>
        /// Draws the settings window content
        /// </summary>
        /// <param name="inRect">The rectangle to draw settings into</param>
        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            Settings?.DoSettingsWindowContent(inRect);
        }

        /// <summary>
        /// The string to draw in the menu for mod settings (if used)
        /// </summary>
        /// <returns>The string to use</returns>
        public override string SettingsCategory()
        {
            if (Settings != null)
            {
                return Content.Name;
            }
            return base.SettingsCategory();
        }

        /// <summary>
        /// The base settings for the mod
        /// </summary>
        public ModSettingsBase Settings { get; set; }

        /// <summary>
        /// The logger to use for this mod
        /// </summary>
        public LoggerBase Logger { get; }

        /// <summary>
        /// The Harmony instance used for this mod
        /// </summary>
        public Harmony Harmony { get; }
    }
}