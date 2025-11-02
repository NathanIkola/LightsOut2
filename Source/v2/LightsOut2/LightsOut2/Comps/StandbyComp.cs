using LightsOut2.Comps.Properties;
using LightsOut2.Core;
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

                // create the standby effects based on the properties
                foreach (Type effectType in standbyProps.standbyEffects)
                {
                    if (effectType.IsSubclassOf(typeof(StandbyEffectBase)))
                    {
                        StandbyEffectBase effect = (StandbyEffectBase)Activator.CreateInstance(effectType, new object[] { parent });
                        effect.Initialize();
                        _standbyEffects.Add(effect);
                    }
                    else
                    {
                        LightsOut2Mod.StaticLogger.Error($"Tried to add a standby effect of type '{effectType}' but it does not inherit from StandbyEffectBase");
                    }
                }
            }
        }

        /// <summary>
        /// Performs post-spawn setup tasks for this comp and its influencers
        /// </summary>
        /// <param name="respawningAfterLoad">Whether or not this is a respawn after loading</param>
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            foreach(StandbyInfluencerBase influencer in _standbyInfluencers)
            {
                influencer.PostSpawnSetup(respawningAfterLoad);
            }

            foreach(StandbyEffectBase effect in _standbyEffects)
            {
                effect.PostSpawnSetup(respawningAfterLoad);
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

            foreach(StandbyEffectBase effect in _standbyEffects)
            {
                effect.ExposeData();
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
            StringBuilder sb = new StringBuilder(base.CompInspectStringExtra());

            // show nothing unless we're in debug mode
            if (!DebugSettings.ShowDevGizmos)
            {
                if (InStandby) { sb.AppendLine("In standby"); }
                return sb.ToString().Trim();
            }

            sb.AppendLine("LightsOut2");
            sb.AppendLine($"  Standby: {InStandby}");
            sb.AppendLine($"  Desires standby: {DesiresStandby}");
            sb.AppendLine($"  Delays shutoff: {UsesDelayOff}");

            foreach(StandbyInfluencerBase influencer in _standbyInfluencers)
            {
                string debugString = influencer.DebugInspectString().Trim();
                if (!string.IsNullOrWhiteSpace(debugString))
                {
                    sb.AppendLine($"  {debugString}");
                }
            }

            foreach(StandbyEffectBase effect in _standbyEffects)
            {
                string debugString = effect.DebugInspectString().Trim();
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

            foreach(StandbyEffectBase effect in _standbyEffects)
            {
                effect.Dispose();
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

            // update the standby state
            ReevaluateStandby();

            // now that standby is up to date, tick the effects
            TickStandbyEffects();
        }

        /// <summary>
        /// Determines if this comp should be in standby
        /// </summary>
        private void ReevaluateStandby()
        {
            // update the desired standby state
            bool previouslyDesiredStandby = _desiresStandby;
            _desiresStandby = WantsToBeInStandby();

            // this is the most likely case, so rule it out first
            if (_inStandby == _desiresStandby) { return; }

            // if we're in standby and no longer want to be, then exit standby mode
            if (_inStandby && !_desiresStandby)
            {
                _inStandby = false;
                _ticksUntilStandby = 0;
                UpdateStandbyEffects();
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
                    UpdateStandbyEffects();
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
        /// Updates the standby effects based on a change in this comp's standby status
        /// </summary>
        private void UpdateStandbyEffects()
        {
            foreach(StandbyEffectBase effect in _standbyEffects)
            {
                effect.UpdateStandby(_inStandby);
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
        /// Ticks all standby effects
        /// </summary>
        private void TickStandbyEffects()
        {
            foreach (StandbyEffectBase effect in  _standbyEffects)
            {
                effect.Tick();
            }
        }

        /// <summary>
        /// Quick cast to the properties for this comp
        /// </summary>
        private CompProperties_Standby StandbyProps => props as CompProperties_Standby;

        /// <summary>
        /// Whether or not this comp should delay turning off
        /// </summary>
        private bool UsesDelayOff => !StandbyProps.noDelay;

        /// <summary>
        /// Whether or not this thing is currently in standby mode
        /// </summary>
        private bool _inStandby = false;

        /// <summary>
        /// Whether or not this thing desires to be in standby mode
        /// </summary>
        private bool _desiresStandby = false;

        /// <summary>
        /// The number of ticks until this thing will be in standby mode
        /// </summary>
        private int _ticksUntilStandby = 0;

        /// <summary>
        /// The list of influencers that will determine if this thing is in standby mode
        /// </summary>
        private readonly List<StandbyInfluencerBase> _standbyInfluencers = new List<StandbyInfluencerBase>();

        /// <summary>
        /// The list of effects to have when standby changes
        /// </summary>
        private readonly List<StandbyEffectBase> _standbyEffects = new List<StandbyEffectBase>();
    }
}