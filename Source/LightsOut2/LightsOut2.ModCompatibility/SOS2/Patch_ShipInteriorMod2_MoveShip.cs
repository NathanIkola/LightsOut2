using LightsOut2.Common;
using LightsOut2.Core.ModCompatibility;
using LightsOut2.Patches;
using System;
using System.Collections.Generic;
using Verse;

namespace LightsOut2.ModCompatibility.SOS2
{
    public class Patch_ShipInteriorMod2_MoveShip : IModCompatibilityPatchComponent
    {
        public override string TypeNameToPatch => "ShipInteriorMod2";

        public override bool TargetsMultipleTypes => false;

        public override bool TypeNameIsExact => true;

        public override string ComponentName => "Patch MoveShip to turn on lights after moving";

        public override IEnumerable<PatchInfo> GetPatches(Type type)
        {
            PatchInfo patch = new PatchInfo
            {
                method = GetMethod(type, "MoveShip"),
                patch = GetMethod<Patch_ShipInteriorMod2_MoveShip>(nameof(Postfix)),
                patchType = PatchType.Postfix,
            };

            return new List<PatchInfo>() { patch };
        }

        public static void Postfix()
        {
            Map map = Find.CurrentMap;
            if (map is null)
                return;

            // loop over all rooms on the map and update their occupancy
            foreach(Room room in map.regionGrid.allRooms)
            {
                if (room is null) 
                    continue;

                bool hasOccupants = !Utils.IsRoomEmpty(room, null);
                Pawn_PathFollower_TryEnterNextPathCell.RaiseOnRoomOccupancyChangedEvent(room, hasOccupants, false);
            }
        }
    }
}