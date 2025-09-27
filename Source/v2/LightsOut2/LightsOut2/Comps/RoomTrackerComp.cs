using Verse;

namespace LightsOut2.Comps
{
    /// <summary>
    /// A comp which detects room changes on a Thing
    /// </summary>
    public sealed class RoomTrackerComp : TickingCompBase
    {
        protected override void Tick()
        {
            // have not reached the tick counter yet, ignore it
            if (++_tickCounter < LightsOut2Settings.TicksBetweenRoomChecks) { return; }
            // we reached the tick counter, so reset it for the next tick
            _tickCounter = 0;
            Room newRoom = parent.GetRoom();
            // no room change detected
            if (newRoom == _lastRoom) { return; }
            // update the room and broadcast the room change
            RoomOccupancyTrackerGameComp.Instance.UpdateRoom(parent as Pawn, newRoom, _lastRoom);
            _lastRoom = newRoom;
        }

        /// <summary>
        /// The room that this Thing was last observed to be in
        /// </summary>
        private Room _lastRoom;

        /// <summary>
        /// The tick counter between refreshes
        /// </summary>
        private int _tickCounter;
    }
}