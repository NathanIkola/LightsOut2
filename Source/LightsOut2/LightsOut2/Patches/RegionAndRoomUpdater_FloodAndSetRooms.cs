using HarmonyLib;
using LightsOut2.Common;
using Verse;

namespace LightsOut2.Patches
{
    /// <summary>
    /// A class that's intended to update the lit status of a light as rooms are updated.
    /// </summary>
    [HarmonyPatch(typeof(RegionAndRoomUpdater), "FloodAndSetRooms")]
    public class RegionAndRoomUpdater_FloodAndSetRooms
    {
        /// <summary>
        /// Runs after a room is resized, automatically enabling/disabling lights as needed
        /// </summary>
        /// <param name="room">The <see cref="Room"/> that was updated</param>
        public static void Postfix(Room room)
        {
            bool hasOccupants = !Utils.IsRoomEmpty(room, null);
            Pawn_PathFollower_TryEnterNextPathCell.RaiseOnRoomOccupancyChangedEvent(room, hasOccupants);
        }
    }
}