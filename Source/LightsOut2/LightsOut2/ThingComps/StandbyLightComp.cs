using LightsOut2.Common;
using LightsOut2.CompProperties;
using LightsOut2.Core.Debug;
using LightsOut2.Gizmos;
using LightsOut2.GlowerActuators;
using LightsOut2.Patches;
using LightsOut2.Core;
using System;
using System.Collections.Generic;
using Verse;
using LightsOut2.Core.GlowerActuators;

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
            KeepOnGizmo = new KeepOnGizmo();
            GlowerComp = parent.GetGlower();
            DebugLogger.Assert(GlowerComp != null, "Couldn't find glower for light", true);
            if (props is CompProperties_Standby standbyProps)
            {
                Type actuatorType = standbyProps.glowerActuatorClass ?? typeof(VanillaGlowerActuator);
                GlowerActuator = Activator.CreateInstance(actuatorType) as IGlowerActuator;
                DebugLogger.Assert(GlowerActuator != null, $"Failed to create glower actuator of type: {actuatorType}", true);
            }

            DoSetup();
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            DoCleanup();
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            DoCleanup();
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            DoSetup();
        }

        /// <summary>
        /// Does any necessary setup when this comp is being initialized or respawned
        /// </summary>
        private void DoSetup()
        {
            if (IsSetUp) 
                return;

            OnStandbyChanged += OnStandbyChangedHandler;
            Pawn_PathFollower_TryEnterNextPathCell.OnRoomOccupancyChanged += OnRoomOccupancyChangedHandler;
            KeepOnGizmo.OnKeepOnChanged += OnKeepOnChangedHandler;
            if (StandbyDelayTicks >= 0)
                TickManager_DoSingleTick.OnTick += OnTickHandler;
            IsSetUp = true;

            // now initialize the lit state as needed
            Room room = parent?.GetLightRoom();
            bool hasOccupants = !Utils.IsRoomEmpty(room, null);
            OnRoomOccupancyChangedHandler(room, hasOccupants, false);
        }

        /// <summary>
        /// A tag that performs necessary cleanup when this comp is being retired
        /// </summary>
        private void DoCleanup()
        {
            if (!IsSetUp)
                return;

            OnStandbyChanged -= OnStandbyChangedHandler;
            Pawn_PathFollower_TryEnterNextPathCell.OnRoomOccupancyChanged -= OnRoomOccupancyChangedHandler;
            KeepOnGizmo.OnKeepOnChanged -= OnKeepOnChangedHandler;
            TickManager_DoSingleTick.OnTick -= OnTickHandler;
            IsSetUp = false;
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

        public override string CompInspectStringExtra()
        {
            if (!DebugSettings.ShowDevGizmos) return base.CompInspectStringExtra();
            return base.CompInspectStringExtra() + "\n" +
                $"Keep On: {KeepOn}\n" + 
                $"StandbyDelayTicks: {StandbyDelayTicks}\n" +
                $"Outside: {parent?.GetRoom()?.OutdoorsForWork}";
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
            RaiseOnStandbyChanged(base.IsInStandby, true);
        }

        /// <summary>
        /// Handles invoking the glower actuator when standby changes
        /// </summary>
        /// <param name="newValue">Ignored</param>
        /// <param name="fromSettings">Whether or not this change is from settings</param>
        private void OnStandbyChangedHandler(bool newValue, bool fromSettings)
        {
            if (!fromSettings && newValue && StandbyDelayTicks == -1 && LightsOut2Mod.LightDelaySeconds > 0)
            {
                StandbyDelayTicks = GenTicks.SecondsToTicks(LightsOut2Mod.LightDelaySeconds);
                TickManager_DoSingleTick.OnTick += OnTickHandler;
            }
            else 
                GlowerActuator.OnStandbyChanged(GlowerComp);
        }

        /// <summary>
        /// Handles changing the standby mode of this comp when its room occupancy changes
        /// </summary>
        /// <param name="room">The room that has an occupancy change</param>
        /// <param name="isOccupied">Whether or not the room is occupied</param>
        /// <param name="respectTimeout">Whether or not to respect the delay timeout setting</param>
        private void OnRoomOccupancyChangedHandler(Room room, bool isOccupied, bool respectTimeout)
        {
            if (room is null || parent.GetLightRoom() == room)
                SetIsInStandby(!isOccupied, !respectTimeout);
        }

        /// <summary>
        /// The gizmo that allows us to keep a light on
        /// </summary>
        public KeepOnGizmo KeepOnGizmo { get; set; }

        /// <summary>
        /// Whether or not this light is being kept on, either by the gizmo or the delay ticks
        /// </summary>
        public bool KeepOn => KeepOnGizmo.KeepOn || StandbyDelayTicks > 0;

        public override float GetRateAsStandbyStatus(bool isInStandby)
        {
            return IsInStandby
                ? LightsOut2Mod.MinDrawDecimal
                : 1f;
        }

        /// <summary>
        /// The handler that is invoked when this comp is paying attention to ticking
        /// </summary>
        public void OnTickHandler()
        {
            if (StandbyDelayTicks <= 0)
            {
                DebugLogger.LogWarning($"StandbyDelayTicks is {StandbyDelayTicks}; why is this still ticking?");
                return;
            }
            if (--StandbyDelayTicks > 0) return;

            OnKeepOnChangedHandler(KeepOn);
            StandbyDelayTicks = -1;
            TickManager_DoSingleTick.OnTick -= OnTickHandler;
        }

        /// <summary>
        /// Lights do not use any actuators for standby status for performance reasons
        /// </summary>
        public override void UpdateStandbyFromActuator(Pawn pawn) { }

        /// <summary>
        /// This light's glower comp
        /// </summary>
        public ThingComp GlowerComp { get; set; }

        /// <summary>
        /// The actuator for this comp's glower
        /// </summary>
        public IGlowerActuator GlowerActuator { get; set; }

        /// <summary>
        /// The number of ticks remaining before the light can be put into standby
        /// </summary>
        protected int StandbyDelayTicks { get; set; } = 1;

        /// <summary>
        /// Whether or not setup has run for this comp
        /// </summary>
        protected bool IsSetUp { get; set; } = false;
    }
}