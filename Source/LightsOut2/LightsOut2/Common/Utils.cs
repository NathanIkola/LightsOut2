using LightsOut2.Debug;
using RimWorld;
using Verse;

namespace LightsOut2.Common
{
    public static class Utils
    {
        /// <summary>
        /// Determines if the given <paramref name="thing"/> is a light
        /// </summary>
        /// <param name="thing">The <see cref="ThingWithComps"/> to check</param>
        /// <returns><see langword="true"/> if the given <paramref name="thing"/> is considered a light</returns>
        public static bool IsLight(this ThingWithComps thing)
        {
            if (IsBillGiver(thing)) return false;
            if (!HasGlower(thing)) return false;
            if (IsTempController(thing)) return false;
            if (HasMiscIllegalComp(thing)) return false;

            DebugLogger.LogInfo($"{thing} is officially a light");
            return true;
        }

        /// <summary>
        /// Determines if the given <paramref name="thing"/> is a table
        /// </summary>
        /// <param name="thing">The <see cref="ThingWithComps"/> to check</param>
        /// <returns><see langword="true"/> if the given <paramref name="thing"/> is considered a table</returns>
        public static bool IsTable(this ThingWithComps thing)
        {
            if (!IsBillGiver(thing)) return false;

            DebugLogger.LogInfo($"{thing} is officially a table");
            return true;
        }

        /// <summary>
        /// Determines if something is an <see cref="IBillGiver"/>
        /// </summary>
        /// <param name="thing">The <see cref="ThingWithComps"/> to check</param>
        /// <returns><see langword="true"/> if the given <paramref name="thing"/> is an <see cref="IBillGiver"/></returns>
        public static bool IsBillGiver(ThingWithComps thing)
        {
            return thing is IBillGiver;
        }

        /// <summary>
        /// Determines if something has a glower
        /// </summary>
        /// <param name="thing">The <see cref="ThingWithComps"/> to check</param>
        /// <returns><see langword="true"/> if the given <paramref name="thing"/> has a glower</returns>
        public static bool HasGlower(ThingWithComps thing)
        {
            if (thing.def.GetCompProperties<CompProperties_Glower>() is null) return false;
            return true;
        }

        /// <summary>
        /// Determines if something is a temp controlling building
        /// </summary>
        /// <param name="thing">The <see cref="ThingWithComps"/> to check</param>
        /// <returns><see langword="true"/> if the given <paramref name="thing"/> is a building that controls temperature (AC, heater, etc...)</returns>
        public static bool IsTempController(ThingWithComps thing)
        {
            if (thing.TryGetComp<CompHeatPusher>() != null) return true;
            if (thing.TryGetComp<CompTempControl>() != null) return true;
            return false;
        }

        /// <summary>
        /// Determines if something has a comp that is determined to be illegal for a light
        /// </summary>
        /// <param name="thing">The <see cref="ThingWithComps"/> to check</param>
        /// <returns><see langword="true"/> if the given <paramref name="thing"/> has any other disallowed comp</returns>
        /// <remarks>
        /// Currently this checks for CompShipLandingBeacon and CompSchedule (for the Sun Lamp) since both
        /// have glowers but should NOT be affected by this mod
        /// </remarks>
        public static bool HasMiscIllegalComp(ThingWithComps thing)
        {
            // landing beacon
            if (thing.TryGetComp<CompShipLandingBeacon>() != null) return true;

            // sun lamp
            if (thing.TryGetComp<CompSchedule>() != null) return true;
            return false;
        }
    }
}