using LightsOut2.Common;
using LightsOut2.CompProperties;
using LightsOut2.Debug;
using LightsOut2.Gizmos;
using LightsOut2.GlowerActuators;
using LightsOut2.Patches;
using System;
using System.Collections.Generic;
using Verse;

namespace LightsOut2.ThingComps
{
    /// <summary>
    /// A class that handles standby specifically for lights
    /// </summary>
    public class StandbyLightComp : StandbyComp
    {
        /// <summary>
        /// Initialize the comp and set up the gizmo
        /// </summary>
        /// <param name="props">The properties for this comp</param>
        public override void Initialize(Verse.CompProperties props)
        {
            IsEnabled = true;
            base.Initialize(props);
            OnStandbyChanged += OnStandbyChangedHandler;
            Pawn_PathFollower_TryEnterNextPathCell.OnRoomOccupancyChanged += OnRoomOccupancyChangedHandler; ;
            KeepOnGizmo = new KeepOnGizmo();
            KeepOnGizmo.OnKeepOnChanged += OnKeepOnChangedHandler;
            GlowerComp = parent.GetGlower();
            DebugLogger.Assert(GlowerComp != null, "Couldn't find glower for light", true);
            if (props is CompProperties_Standby standbyProps)
            {
                Type actuatorType = standbyProps.glowerActuatorClass ?? typeof(VanillaGlowerActuator);
                GlowerActuator = Activator.CreateInstance(actuatorType) as IGlowerActuator;
                DebugLogger.Assert(GlowerActuator != null, $"Failed to create glower actuator of type: {actuatorType}", true);
            }
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            Cleanup();
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            Cleanup();
        }

        /// <summary>
        /// A tag that performs necessary cleanup when this comp is being retired
        /// </summary>
        private void Cleanup()
        {
            OnStandbyChanged -= OnStandbyChangedHandler;
            Pawn_PathFollower_TryEnterNextPathCell.OnRoomOccupancyChanged -= OnRoomOccupancyChangedHandler;
            KeepOnGizmo.OnKeepOnChanged -= OnKeepOnChangedHandler;
        }

        /// <summary>
        /// Retrieves a list of the gizmos to display for this comp
        /// </summary>
        /// <returns>A list of applicable gizmos to display</returns>
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
                yield return gizmo;

            if (!IsEnabled) yield break;
            yield return KeepOnGizmo;
        }

        /// <summary>
        /// Saves the state of the comp
        /// </summary>
        public override void PostExposeData()
        {
            base.PostExposeData();
            bool keepOn = KeepOnGizmo.KeepOn;
            Scribe_Values.Look(ref keepOn, "KeepOn", false);
            KeepOnGizmo.KeepOn = keepOn;
        }

        public override bool IsInStandby
        {
            get { return !KeepOn && base.IsInStandby; }
            set { base.IsInStandby = value; }
        }

        /// <summary>
        /// Handler for the event fired when the keep on status changes
        /// </summary>
        /// <param name="newValue">Whether or not it is being kept on</param>
        private void OnKeepOnChangedHandler(bool newValue)
        {
            RaiseOnStandbyChanged(IsInStandby);
        }

        /// <summary>
        /// Handles invoking the glower actuator when standby changes
        /// </summary>
        /// <param name="newValue">Ignored</param>
        private void OnStandbyChangedHandler(bool newValue)
        {
            GlowerActuator.OnStandbyChanged(GlowerComp);
        }

        /// <summary>
        /// Handles changing the standby mode of this comp when its room occupancy changes
        /// </summary>
        /// <param name="room">The room that has an occupancy change</param>
        /// <param name="isOccupied">Whether or not the room is occupied</param>
        private void OnRoomOccupancyChangedHandler(Room room, bool isOccupied)
        {
            if (parent.GetLightRoom() == room)
                IsInStandby = !isOccupied;
        }

        /// <summary>
        /// The gizmo that allows us to keep a light on
        /// </summary>
        public KeepOnGizmo KeepOnGizmo { get; set; }

        /// <summary>
        /// Whether or not this light is being kept on
        /// </summary>
        public bool KeepOn => KeepOnGizmo.KeepOn;

        public override float GetRateAsStandbyStatus(bool isInStandby)
        {
            return IsInStandby
                ? LightsOut2Settings.MinDraw / 100f
                : 1f;
        }

        /// <summary>
        /// This light's glower comp
        /// </summary>
        public ThingComp GlowerComp { get; set; }

        /// <summary>
        /// The actuator for this comp's glower
        /// </summary>
        public IGlowerActuator GlowerActuator { get; set; }
    }
}