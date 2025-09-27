using Verse;

namespace LightsOut2.Extensions
{
    public static class Pawns
    {
        /// <summary>
        /// Whether or not the given pawn should activate lights based on config
        /// </summary>
        /// <param name="pawn">The pawn to check</param>
        /// <returns>Whether or not the pawn should activate lights</returns>
        public static bool ActivatesLights(this Pawn pawn)
        {
            // pawn can't be considered an occupant at all, so ignore them
            if (!pawn.CanBeOccupant()) { return false; }

            // we want to turn off lights for sleeping pawns and the pawn is asleep, ignore them
            if (LightsOut2Settings.FlickLightsForSleepingPawns && pawn.IsAsleep()) { return false; }

            // otherwise this pawn should flick lights
            return true;
        }

        /// <summary>
        /// Determines if the pawn is asleep
        /// </summary>
        /// <param name="pawn">The pawn to check</param>
        /// <returns>Whether or not the pawn is asleep</returns>
        public static bool IsAsleep(this Pawn pawn)
        {
            return pawn?.jobs?.curDriver?.asleep ?? false;
        }

        /// <summary>
        /// Whether or not the pawn should be onsidered an animal
        /// </summary>
        /// <param name="pawn">The pawn to check</param>
        /// <returns>Whether or not the pawn is considered an animal</returns>
        public static bool IsAnimal(this Pawn pawn)
        {
            return pawn.RaceProps.Animal;
        }

        /// <summary>
        /// Whether or not this pawn can be an occupant
        /// </summary>
        /// <param name="pawn">The pawn to check</param>
        /// <returns>Whether or not this pawn can be considered an occupant at all</returns>
        public static bool CanBeOccupant(this Pawn pawn)
        {
            return LightsOut2Settings.AnimalsFlickLights;
        }
    }
}