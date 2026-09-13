using LightsOut2.Extensions;
using System.Text;
using Verse;

namespace LightsOut2.Comps
{
    /// <summary>
    /// A comp which detects room changes on a Thing
    /// </summary>
    public sealed class RoomTrackerComp : TickingCompBase
    {
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            // this comp only works for pawns, so no reason to subscribe to ticking if the parent isn't a pawn
            if (parent is Pawn pawn)
            {
                base.PostSpawnSetup(respawningAfterLoad);
                RoomOccupancyTrackerGameComp.Instance.UpdateRoom(pawn, pawn.GetRoom(), null);
            }
        }

        protected override bool AllowTicking()
        {
            // respect the base setting
            if (!base.AllowTicking()) { return false; }
            // also only allow ticking if this pawn is allowed to be an occupant
            return parent is Pawn pawn && pawn.CanBeOccupant();
        }

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

        public override string CompInspectStringExtra()
        {
            StringBuilder sb = new StringBuilder(base.CompInspectStringExtra());

            // show nothing unless we're in debug mode
            if (!DebugSettings.ShowDevGizmos)
            {
                return sb.ToString().Trim();
            }

            if (parent is Pawn pawn)
            {
                sb.AppendLine($"Activates lights: {pawn.ActivatesLights()}");
                sb.AppendLine($"Is room occupant: {pawn.CanBeOccupant()}");
                sb.AppendLine($"Sleeping: {pawn.IsAsleep()}");
            }

            return sb.ToString().Trim();
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