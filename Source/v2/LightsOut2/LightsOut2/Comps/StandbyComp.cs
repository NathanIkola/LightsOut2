using LightsOut2.Comps.Properties;
using LightsOut2.Core;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace LightsOut2.Comps
{
    /// <summary>
    /// The comp used to determine if a thing is in standby mode
    /// </summary>
    public sealed class StandbyComp : TickingCompBase
    {
        /// <summary>
        /// Whether or not this comp is currently in standby mode
        /// </summary>
        public bool InStandby => _inStandby;

        /// <summary>
        /// Whether or not this comp desires to be in standby
        /// </summary>
        /// <remarks>
        /// This may not match InStandby because the standby may be delayed
        /// </remarks>
        public bool DesiresStandby => _desiresStandby;

        /// <summary>
        /// The current multiplier for this comp based on the standby state
        /// </summary>
        public float CurrentMultiplier => _currentMultiplier;

        /// <summary>
        /// Initializes the comp with the given properties
        /// </summary>
        /// <param name="props">The properties</param>
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            if (props is CompProperties_Standby standbyProps)
            {
                // create the standby influencers based on the properties
                foreach(Type influencerType in standbyProps.standbyInfluencers)
                {
                    if (influencerType.IsSubclassOf(typeof(StandbyInfluencerBase)))
                    {
                        StandbyInfluencerBase influencer = (StandbyInfluencerBase)Activator.CreateInstance(influencerType, new object[] { parent });
                        influencer.Initialize();
                        _standbyInfluencers.Add(influencer);
                    }
                    else
                    {
                        LightsOut2Mod.StaticLogger.Error($"Tried to add a standby influencer of type '{influencerType}' but it does not inherit from StandbyInfluencerBase");
                    }
                }
            }
        }

        /// <summary>
        /// Exposes the data for this comp to allow saving/loading
        /// </summary>
        public override void PostExposeData()
        {
            base.PostExposeData();
            foreach(StandbyInfluencerBase influencer in _standbyInfluencers)
            {
                influencer.ExposeData();
            }
        }

        /// <summary>
        /// Retrieves the gizmos from the influencers
        /// </summary>
        /// <returns>All of the gizmos</returns>
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (StandbyInfluencerBase influencer in _standbyInfluencers)
            {
                foreach (Gizmo gizmo in influencer.GetGizmos())
                {
                    yield return gizmo;
                }
            }
            yield break;
        }

        /// <summary>
        /// Generates the string to display in the inspect panel for this comp
        /// </summary>
        /// <returns>The debug inspection strings</returns>
        public override string CompInspectStringExtra()
        {
            // show nothing unless we're in debug mode
            if (!DebugSettings.ShowDevGizmos)
            {
                return base.CompInspectStringExtra();
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("LightsOut2");
            sb.AppendLine($"  Standby: {InStandby}");
            sb.AppendLine($"  Desires standby: {DesiresStandby}");
            sb.AppendLine($"  Multiplier: {CurrentMultiplier*100f}%");
            sb.AppendLine($"  Is light: {StandbyProps.isLight}");
            sb.AppendLine($"  Delays shutoff: {UsesDelayOff}");

            foreach(StandbyInfluencerBase influencer in _standbyInfluencers)
            {
                string debugString = influencer.DebugInspectString().Trim();
                if (!string.IsNullOrWhiteSpace(debugString))
                {
                    sb.AppendLine($"  {debugString}");
                }
            }

            return sb.ToString().Trim();
        }

        public override void ReceiveCompSignal(string signal)
        {
            base.ReceiveCompSignal(signal);
            LightsOut2Mod.StaticLogger.Trace($"Thing {parent} received comp signal: {signal}");
        }

        /// <summary>
        /// Cleans up any outstanding dependencies of this comp or the influencers
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            foreach(StandbyInfluencerBase influencer in _standbyInfluencers)
            {
                influencer.Dispose();
            }
        }

        /// <summary>
        /// Update the state once per tick
        /// </summary>
        /// <remarks>
        /// We can't rely on the CompTick to fire since some buildings are not
        /// registered to be tickers, so we have to make our own ticker
        /// </remarks>
        protected override void Tick()
        {
            // tick our influencers first so we use their updated states to determine standby
            TickStandbyInfluencers();

            // update the desired standby state
            bool previouslyDesiredStandby = _desiresStandby;
            _desiresStandby = WantsToBeInStandby();

            // this is the most likely case, so rule it out first
            if (_inStandby == _desiresStandby) { return; }
            // if we're in standby and no longer want to be, then exit standby mode
            else if (_inStandby && !_desiresStandby)
            {
                _inStandby = false;
                _ticksUntilStandby = 0;
                UpdatePowerDraw();
            }
            // otherwise, if we want to go into standby but aren't currently
            else if (_desiresStandby && !_inStandby)
            {
                // count down a tick until standby
                // do this before setting the standby state so that we don't immediately subtract a tick
                if (_ticksUntilStandby > 0) { _ticksUntilStandby -= 1; }

                // if this is the tick we started wanting standby, then begin the transition
                if (!previouslyDesiredStandby) { BeginStandbyTransition(); }

                // detect when standby is ready to be enabled
                // this happens after the transition in case a user sets the delay to 0
                if (_ticksUntilStandby <= 0) 
                { 
                    _inStandby = true;
                    UpdatePowerDraw();
                }
            }
        }

        /// <summary>
        /// Determines whether this comp is in standby mode
        /// </summary>
        /// <returns>Whether or not this comp should be in standby</returns>
        public bool WantsToBeInStandby()
        {
            // if any of the influencers are active then we are not in standby mode
            if (_standbyInfluencers.Any(influencer => influencer.BuildingIsActive))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Gets the coefficient that should be applied to the power draw baed on the current state
        /// </summary>
        /// <returns>The coefficient based on the current state</returns>
        public float ResourceDrawMultiplier()
        {
            if (StandbyProps.isLight)
            {
                // lights are handled specially, they always use 0% power when in standby
                // and 100% when active, no matter what the settings say
                return _inStandby ? 0f : 1f;
            }
            return _inStandby
                    // in standby mode, use the standby coefficient, but ensure it is at least the minimum value
                    ? Math.Max(
                        LightsOut2Settings.StandbyCoefficientDecimal,
                        LightsOut2Settings.MinDrawCoefficientDecimal)
                    // otherwise use the active coefficient
                    : LightsOut2Settings.ActiveCoefficientDecimal;
        }

        /// <summary>
        /// Starts transitioning to standby mode
        /// </summary>
        private void BeginStandbyTransition()
        {
            // lights need to respect the delay setting
            if (UsesDelayOff)
            {
                _ticksUntilStandby = GenTicks.SecondsToTicks(LightsOut2Settings.LightDelaySeconds);
            }
        }

        /// <summary>
        /// Updates the power draw rate on this comp and also the associated power trader
        /// </summary>
        private void UpdatePowerDraw()
        {
            _currentMultiplier = ResourceDrawMultiplier();
            CompPowerTrader powerTrader = PowerTrader;
            if (powerTrader != null)
            {
                // reset it to its default power draw
                powerTrader.SetUpPowerVars();
                // then apply our multiplier so the power trader naturally pulls the correct amount without patching it
                // we want to avoid the patch on PowerOutput because it runs every tick for every single Thing with a CompPowerTrader,
                // which is A LOT of them in a normal colony
                powerTrader.powerOutputInt *= CurrentMultiplier;
            }
        }

        /// <summary>
        /// Ticks all standby influencers
        /// </summary>
        private void TickStandbyInfluencers()
        {
            foreach (StandbyInfluencerBase influencer in _standbyInfluencers)
            {
                influencer.Tick();
            }
        }

        /// <summary>
        /// Quick cast to the properties for this comp
        /// </summary>
        private CompProperties_Standby StandbyProps => props as CompProperties_Standby;

        /// <summary>
        /// Whether or not this comp should delay turning off
        /// </summary>
        private bool UsesDelayOff => !StandbyProps.noDelay && StandbyProps.isLight;

        /// <summary>
        /// Whether or not this thing is currently in standby mode
        /// </summary>
        private bool _inStandby = false;

        /// <summary>
        /// Whether or not this thing desires to be in standby mode
        /// </summary>
        private bool _desiresStandby = false;

        /// <summary>
        /// The current rate multiplier for this comp
        /// </summary>
        private float _currentMultiplier = 0f;

        /// <summary>
        /// The number of ticks until this thing will be in standby mode
        /// </summary>
        private int _ticksUntilStandby = 0;

        /// <summary>
        /// The power trader to affect when in/out of standby mode in lieu of a patch on PowerOutput
        /// </summary>
        private CompPowerTrader _powerTrader = null;

        /// <summary>
        /// Gets the power trader from the cache, or looks it up if it hasn't been cached yet
        /// </summary>
        private CompPowerTrader PowerTrader
        {
            get
            {
                // we already looked it up, so get the cached power trader
                if (_powerTrader != null) { return _powerTrader; }
                // otherwise look it up the first time it's needed and cache it for the next calls
                _powerTrader = parent.TryGetComp<CompPowerTrader>();
                return _powerTrader;
            }
        }

        /// <summary>
        /// The list of influencers that will determine if this thing is in standby mode
        /// </summary>
        private readonly List<StandbyInfluencerBase> _standbyInfluencers = new List<StandbyInfluencerBase>();
    }
}