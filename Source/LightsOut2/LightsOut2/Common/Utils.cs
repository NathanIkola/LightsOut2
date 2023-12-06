using LightsOut2.Debug;
using LightsOut2.ThingComps;
using RimWorld;
using System;
using System.Reflection;
using Verse;

namespace LightsOut2.Common
{
    /// <summary>
    /// A class that holds many utility functions for the mod
    /// </summary>
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
            if (IsTable(def)) return false;
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
            bool isTable = IsProductionBuilding(def)
                && !SupportsPlants(def);
            if (isTable) DebugLogger.LogInfo($"{def} detected as a table");
            return isTable;
        }

        /// <summary>
        /// Determines if the given <paramref name="def"/> is a produciton building
        /// </summary>
        /// <param name="def">The <see cref="ThingDef"/> to check</param>
        /// <returns>Whether or not the given <paramref name="def"/> is a production building</returns>
        public static bool IsProductionBuilding(ThingDef def)
        {
            return def.building?.buildingTags?.Contains("Production") ?? false;
        }

        /// <summary>
        /// Determines if the given <paramref name="def"/> supports plants
        /// </summary>
        /// <param name="def">The <see cref="ThingDef"/> to check</param>
        /// <returns>Whether or not this <paramref name="def"/> supports plants</returns>
        public static bool SupportsPlants(ThingDef def)
        {
            return def.building?.SupportsPlants ?? false;
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
            // autobong
            if (def.comps.Any(x => x.compClass == typeof(CompGiveHediffSeverity))) return true;
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

        /// <summary>
        /// Goes over the list of things in a <paramref name="room"/> to see if any of
        /// them are <see cref="Pawn"/>s that count as occupant, excluding the given <paramref name="excludedPawn"/>
        /// </summary>
        /// <param name="room">The <see cref="Room"/> to check</param>
        /// <param name="excludedPawn">The <see cref="Pawn"/> that initiated the request and should be ignored</param>
        /// <returns>Whether or not the given <paramref name="room"/> will still be occupied if <paramref name="excludedPawn"/> leaves</returns>
        public static bool IsRoomEmpty(Room room, Pawn excludedPawn)
        {
            if (room is null || room.OutdoorsForWork)
                return false;

            Thing[] things = GetThingsInRoom(room);
            foreach (Thing thing in things)
                if (thing is Pawn pawn && pawn != excludedPawn && PawnCountsAsOccupant(pawn))
                    return false;
            return true;
        }

        /// <summary>
        /// Determines if the given <paramref name="pawn"/> counts as an occupant
        /// </summary>
        /// <param name="pawn">The <see cref="Pawn"/> to check</param>
        /// <returns>Whether or not the given <paramref name="pawn"/> is considered an occupant</returns>
        public static bool PawnCountsAsOccupant(Pawn pawn)
        {
            // verify that this pawn even counts as a valid pawn for our considerations
            bool isValidPawn = pawn.RaceProps.ToolUser || (LightsOut2Settings.AnimalsActivateLights && pawn.RaceProps.Animal);
            if (!isValidPawn) return false;

            // pawn is asleep, and we turn off lights on sleeping pawns
            if (LightsOut2Settings.TurnOffLightsInBed && PawnIsAsleep(pawn))
                return false;

            // if light flicking for waking pawns is disabled, return true for all valid pawns
            if (!LightsOut2Settings.FlickLights)
                return true;

            // pawn is leaving the room, ignore them
            if ((pawn.pather.nextCell.GetEdifice(pawn.Map) as Building_Door) != null)
                return false;

            // pawn is currently in a doorway, ignore them
            if ((pawn.Position.GetEdifice(pawn.Map) as Building_Door) != null)
                return false;

            // pawn passed all above exceptions, consider them to be an occupant
            return true;
        }

        /// <summary>
        /// Determines if the given <paramref name="pawn"/> is asleep
        /// </summary>
        /// <param name="pawn">The <see cref="Pawn"/> to check</param>
        /// <returns>Whether or not the <paramref name="pawn"/> is asleep</returns>
        public static bool PawnIsAsleep(Pawn pawn)
        {
            return pawn.jobs?.curDriver?.asleep ?? false;
        }

        /// <summary>
        /// A function which repeatedly attempts to get all the things in a room.
        /// Needs to happen in a try/catch block + while loop because the collection
        /// does end up being modified fairly frequently
        /// </summary>
        /// <param name="room">The <see cref="Room"/> to get things in</param>
        /// <returns>A list of the <see cref="Thing"/>s in the <paramref name="room"/></returns>
        private static Thing[] GetThingsInRoom(Room room)
        {
            int attempts = 0;
            bool done = false;
            Thing[] things = null;
            while (!done)
            {
                try
                {
                    things = room.ContainedAndAdjacentThings.ToArray();
                    done = true;
                }
                catch (InvalidOperationException ex)
                {
                    if (ex.Message.ToLower().Contains("modified"))
                        done = (++attempts > 100);
                    else
                    {
                        DebugLogger.LogWarning($"InvalidOperationException: {ex.Message}");
                        done = true;
                    }
                }
            }
            return things;
        }

        /// <summary>
        /// A good list of <see cref="BindingFlags"/> to use to get most things
        /// </summary>
        public readonly static BindingFlags BindingFlags = BindingFlags.Public
                            | BindingFlags.NonPublic
                            | BindingFlags.Instance
                            | BindingFlags.Static
                            | BindingFlags.FlattenHierarchy;
    }
}