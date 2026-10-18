using System.Collections.Generic;
using LightsOut2.Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LightsOut2.Tests
{
    /// <summary>
    /// Tests room state changes without requiring a running game.
    /// </summary>
    [TestClass]
    public sealed class OccupancyStateTests
    {
        /// <summary>
        /// First observation and real changes emit once each.
        /// </summary>
        [TestMethod]
        public void InitialAndChangedOccupancyEmitOnce()
        {
            var state = new FakeOccupancyState();
            var changes = new List<string>();
            state.Changed += (room, occupied) => changes.Add($"{room}:{occupied}");
            state.MarkRoomDirty("A", null);
            state.EvaluateDirtyRooms();
            state.MarkRoomDirty("A", null);
            state.EvaluateDirtyRooms();
            state.MarkRoomDirty("A", null);
            state.OccupiedRooms.Add("A");
            state.EvaluateDirtyRooms();
            CollectionAssert.AreEqual(new[] { "A:False", "A:True" }, changes);
        }

        /// <summary>
        /// Moving out of a room reevaluates it while entering a room marks it occupied.
        /// </summary>
        [TestMethod]
        public void MoveToAnotherRoomAndThenNullReconcilesBothRooms()
        {
            var state = new FakeOccupancyState();
            state.PawnMoved("pawn", "A", null);
            state.PawnMoved("pawn", "B", "A");
            state.EvaluateDirtyRooms();
            Assert.IsTrue(state.TryGetOccupancyStatus("A", out bool a) && !a);
            Assert.IsTrue(state.TryGetOccupancyStatus("B", out bool b) && b);
            state.PawnMoved("pawn", null, "B");
            state.EvaluateDirtyRooms();
            Assert.IsTrue(state.TryGetOccupancyStatus("B", out b) && !b);
        }

        /// <summary>
        /// A reported old room does not hide a different previously tracked room.
        /// </summary>
        [TestMethod]
        public void TeleportReevaluatesKnownAndReportedOldRooms()
        {
            var state = new FakeOccupancyState();
            state.PawnMoved("pawn", "A", null);
            state.PawnMoved("pawn", "C", "B");
            state.EvaluateDirtyRooms();
            Assert.IsTrue(state.TryGetOccupancyStatus("A", out bool a) && !a);
            Assert.IsTrue(state.TryGetOccupancyStatus("B", out bool b) && !b);
            Assert.IsTrue(state.TryGetOccupancyStatus("C", out bool c) && c);
        }

        /// <summary>
        /// Multiple occupants keep a room active when one pawn leaves.
        /// </summary>
        [TestMethod]
        public void OtherOccupantsRemainCounted()
        {
            var state = new FakeOccupancyState();
            state.PawnMoved("one", "A", null);
            state.PawnMoved("two", "A", null);
            state.PawnMoved("one", null, "A");
            state.Occupants["A"] = new HashSet<string> { "two" };
            state.EvaluateDirtyRooms();
            Assert.IsTrue(state.TryGetOccupancyStatus("A", out bool occupied) && occupied);
        }

        /// <summary>
        /// A pawn returning before evaluation must count as an occupant again.
        /// </summary>
        [TestMethod]
        public void ReturningPawnKeepsFormerRoomOccupied()
        {
            var state = new FakeOccupancyState();
            state.PawnMoved("pawn", "A", null);
            state.PawnMoved("pawn", "B", "A");
            state.PawnMoved("pawn", "A", "B");
            state.Occupants["A"] = new HashSet<string> { "pawn" };
            state.EvaluateDirtyRooms();
            Assert.IsTrue(state.TryGetOccupancyStatus("A", out bool occupied) && occupied);
            Assert.IsTrue(state.EvaluatedRooms.Contains("A"));
        }

        /// <summary>
        /// A pawn still present in a flagged room is not excluded from its occupancy check.
        /// </summary>
        [TestMethod]
        public void FlaggedRoomStillCountsItsTriggeringPawn()
        {
            var state = new FakeOccupancyState();
            state.PawnMoved("pawn", "A", null);
            state.Occupants["A"] = new HashSet<string> { "pawn" };
            state.MarkRoomDirty("A", "pawn");
            state.EvaluateDirtyRooms();

            Assert.IsFalse(state.LastIgnoredPawns.Contains("pawn"));
            Assert.IsTrue(state.TryGetOccupancyStatus("A", out bool occupied) && occupied);
        }

        /// <summary>
        /// Doorways never receive an occupied cache entry.
        /// </summary>
        [TestMethod]
        public void DoorwaysAreNotCachedAsOccupied()
        {
            var state = new FakeOccupancyState();
            state.Doorways.Add("door");
            state.PawnMoved("pawn", "door", null);
            state.EvaluateDirtyRooms();
            Assert.IsFalse(state.TryGetOccupancyStatus("door", out _));
        }

        /// <summary>
        /// Despawn removes a pawn from tracking and reevaluates its last room.
        /// </summary>
        [TestMethod]
        public void ForgetReevaluatesFormerRoom()
        {
            var state = new FakeOccupancyState();
            state.PawnMoved("pawn", "A", null);
            state.Forget("pawn");
            state.EvaluateDirtyRooms();
            Assert.IsTrue(state.TryGetOccupancyStatus("A", out bool occupied) && !occupied);
            CollectionAssert.AreEqual(new string[0], new List<string>(state.TrackedPawns));
        }
    }

    /// <summary>Supplies configurable room properties to occupancy tests.</summary>
    internal sealed class FakeOccupancyState : OccupancyStateBase<string, string>
    {
        /// <summary>Rooms treated as doorways.</summary>
        public HashSet<string> Doorways { get; } = new HashSet<string>();

        /// <summary>Rooms considered occupied without inspecting occupants.</summary>
        public HashSet<string> OccupiedRooms { get; } = new HashSet<string>();

        /// <summary>Current occupants by room.</summary>
        public Dictionary<string, HashSet<string>> Occupants { get; } = new Dictionary<string, HashSet<string>>();

        /// <summary>The ignored pawns supplied to the most recent occupancy check.</summary>
        public HashSet<string> LastIgnoredPawns { get; } = new HashSet<string>();

        /// <summary>Rooms passed to the occupancy check.</summary>
        public List<string> EvaluatedRooms { get; } = new List<string>();

        /// <inheritdoc />
        protected override bool IsDoorway(string room) => Doorways.Contains(room);

        /// <inheritdoc />
        protected override RoomOccupancy GetState(string room, ISet<string> ignoredPawns)
        {
            EvaluatedRooms.Add(room);
            LastIgnoredPawns.Clear();
            foreach (string pawn in ignoredPawns)
            {
                LastIgnoredPawns.Add(pawn);
            }

            if (Occupants.TryGetValue(room, out HashSet<string> occupants))
            {
                foreach (string pawn in occupants)
                {
                    if (!ignoredPawns.Contains(pawn))
                    {
                        return RoomOccupancy.OccupiedWithAwakePawn;
                    }
                }
            }

            return OccupiedRooms.Contains(room) ? RoomOccupancy.OccupiedWithAwakePawn : RoomOccupancy.Empty;
        }
    }
}
