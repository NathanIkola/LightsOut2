using LightsOut2.Debug;
using LightsOut2.ThingComps;
using RimWorld;
using System;
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
            return IsLight(thing.def);
        }

        /// <summary>
        /// Determines if the given <paramref name="def"/> is a light
        /// </summary>
        /// <param name="def">The <see cref="ThingDef"/> to check</param>
        /// <returns><see langword="true"/> if the given <paramref name="def"/> is considered a light</returns>
        public static bool IsLight(this ThingDef def)
        {
            if (def.defName == "Autobong") return false;

            if (IsBillGiver(def)) return false;
            if (!HasGlower(def)) return false;
            if (IsTempController(def)) return false;
            if (HasMiscIllegalComp(def)) return false;

            DebugLogger.LogInfo($"{def} is officially a light");
            return true;
        }

        /// <summary>
        /// Determines if the given <paramref name="thing"/> is a table
        /// </summary>
        /// <param name="thing">The <see cref="ThingWithComps"/> to check</param>
        /// <returns><see langword="true"/> if the given <paramref name="thing"/> is considered a table</returns>
        public static bool IsTable(this ThingWithComps thing)
        {
            return IsTable(thing.def);
        }

        /// <summary>
        /// Determines if the given <paramref name="def"/> is a table
        /// </summary>
        /// <param name="def">The <see cref="ThingDef"/> to check</param>
        /// <returns><see langword="true"/> if the given <paramref name="def"/> is considered a table</returns>
        public static bool IsTable(this ThingDef def)
        {
            if (!IsBillGiver(def)) return false;
            DebugLogger.LogInfo($"{def} detected as a table");
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
        /// Determines if something is an <see cref="IBillGiver"/>
        /// </summary>
        /// <param name="def">The <see cref="ThingDef"/> to check</param>
        /// <returns><see langword="true"/> if the given <paramref name="def"/> is an <see cref="IBillGiver"/></returns>
        public static bool IsBillGiver(ThingDef def)
        {
            return typeof(IBillGiver).IsAssignableFrom(def.thingClass);
        }

        /// <summary>
        /// Determines if something has a glower
        /// </summary>
        /// <param name="thing">The <see cref="ThingWithComps"/> to check</param>
        /// <returns><see langword="true"/> if the given <paramref name="thing"/> has a glower</returns>
        public static bool HasGlower(ThingWithComps thing)
        {
            return HasGlower(thing.def);
        }

        /// <summary>
        /// Determines if something has a glower
        /// </summary>
        /// <param name="def">The <see cref="ThingDef"/> to check</param>
        /// <returns><see langword="true"/> if the given <paramref name="def"/> has a glower</returns>
        public static bool HasGlower(ThingDef def)
        {
            if (def.comps is null) return false;
            return (def.GetCompProperties<CompProperties_Glower>() != null) || HasModdedGlower(def);
        }

        /// <summary>
        /// Determines if something has a modded glower
        /// </summary>
        /// <param name="def">The <see cref="ThingDef"/> to check</param>
        /// <returns><see langword="true"/> if the given <paramref name="def"/> has a modded glower</returns>
        public static bool HasModdedGlower(ThingDef def)
        {
            if (def.comps is null) return false;
            foreach (Verse.CompProperties props in def.comps)
                if (props.compClass.Name.Contains("Glow")) return true;
            return false;
        }

        /// <summary>
        /// Function for retrieving the glower off of a ThingWithComps
        /// </summary>
        /// <param name="thing">The <see cref="ThingWithComps"/> to check</param>
        /// <returns>The <see cref="ThingComp"/> for this building's glower comp</returns>
        public static ThingComp GetGlower(this ThingWithComps thing)
        {
            ThingComp glower = thing.TryGetComp<CompGlower>();
            if (glower != null) return glower;

            // try to get modded glowers
            foreach (ThingComp comp in thing.AllComps)
                if (comp.GetType().Name.Contains("Glow")) return comp;

            return null;
        }

        /// <summary>
        /// Attempts to retrieve the glower class from a <paramref name="def"/>
        /// </summary>
        /// <param name="def">The <see cref="ThingDef"/> to check</param>
        /// <returns>The associated glower type</returns>
        public static Type GetGlowerClass(this ThingDef def)
        {
            if (def.comps is null) return null;
            foreach (Verse.CompProperties props in def.comps)
                if (props.compClass.Name.Contains("Glow")) return props.compClass;
            return null;
        }

        /// <summary>
        /// Determines if something is a temp controlling building
        /// </summary>
        /// <param name="thing">The <see cref="ThingWithComps"/> to check</param>
        /// <returns><see langword="true"/> if the given <paramref name="thing"/> is a building that controls temperature (AC, heater, etc...)</returns>
        public static bool IsTempController(ThingWithComps thing)
        {
            return IsTempController(thing.def);
        }

        /// <summary>
        /// Determines if something is a temp controlling building
        /// </summary>
        /// <param name="def">The <see cref="ThingDef"/> to check</param>
        /// <returns><see langword="true"/> if the given <paramref name="def"/> is a building that controls temperature (AC, heater, etc...)</returns>
        public static bool IsTempController(ThingDef def)
        {
            if (def.comps is null) return false;
            if (def.comps.Any(x => x.compClass == typeof(CompHeatPusher))) return true;
            if (def.comps.Any(x => x.compClass == typeof(CompTempControl))) return true;
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
            return HasMiscIllegalComp(thing.def);
        }

        /// <summary>
        /// Determines if something has a comp that is determined to be illegal for a light
        /// </summary>
        /// <param name="def">The <see cref="ThingDef"/> to check</param>
        /// <returns><see langword="true"/> if the given <paramref name="def"/> has any other disallowed comp</returns>
        /// <remarks>
        /// Currently this checks for CompShipLandingBeacon and CompSchedule (for the Sun Lamp) since both
        /// have glowers but should NOT be affected by this mod
        /// </remarks>
        public static bool HasMiscIllegalComp(ThingDef def)
        {
            if (def.comps is null) return false;
            // landing beacon
            if (def.comps.Any(x => x.compClass == typeof(CompShipLandingBeacon))) return true;
            // sun lamp
            if (def.comps.Any(x => x.compClass == typeof(CompSchedule))) return true;
            // loudspeaker
            if (def.comps.Any(x => x.compClass == typeof(CompLoudspeaker))) return true;
            // lightball
            if (def.comps.Any(x => x.compClass == typeof(CompLightball))) return true;
            return false;
        }

        /// <summary>
        /// Retrieve the first StandbyComp encountered on the given <paramref name="thing"/>
        /// </summary>
        /// <param name="thing">The <see cref="ThingWithComps"/> to check</param>
        /// <returns>The <see cref="StandbyComp"/> or derived class from thie <paramref name="thing"/></returns>
        public static StandbyComp GetStandbyComp(this ThingWithComps thing)
        {
            if (thing.AllComps is null) return null;
            foreach (ThingComp comp in thing.AllComps)
                if (comp is StandbyComp standby) 
                    return standby;

            return null;
        }

        /// <summary>
        /// A function that attempts to retrieve the room a given light is attributed to
        /// </summary>
        /// <param name="thing">The <see cref="ThingWithComps"/> to check</param>
        /// <returns>The room that thie <paramref name="thing"/> exists in</returns>
        public static Room GetLightRoom(this ThingWithComps thing)
        {
            // if the region and room updater isn't enabled it will throw lots of errors
            if (!(thing.Map?.regionAndRoomUpdater?.Enabled ?? false)) return null;
            Room room = RegionAndRoomQuery.RoomAt(thing.Position, thing.Map);
            // wall lights will not return a room from the above and require getting the room for the cell they face
            if (room is null) return RegionAndRoomQuery.RoomAt(thing.Position + thing.Rotation.FacingCell, thing.Map);
            return room;
        }
    }
}