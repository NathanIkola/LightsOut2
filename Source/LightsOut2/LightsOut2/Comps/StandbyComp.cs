using LightsOut2.Comps.Properties;
using LightsOut2.Core;
using LightsOut2.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public bool InStandby => _transition.InStandby;

        /// <summary>
        /// Whether or not this comp desires to be in standby
        /// </summary>
        /// <remarks>
        /// This may not match InStandby because the standby may be delayed
        /// </remarks>
        public bool DesiresStandby => _transition.DesiresStandby;

        /// <summary>
        /// Tries to retrieve the first standby influencer of the requested type.
        /// </summary>
        /// <typeparam name="TInfluencer">The influencer type to retrieve.</typeparam>
        /// <param name="influencer">The matching influencer, if one is attached.</param>
        /// <returns>True if a matching influencer was found; otherwise, false.</returns>
        public bool TryGetInfluencer<TInfluencer>(out TInfluencer influencer)
            where TInfluencer : StandbyInfluencerBase
        {
            influencer = _standbyInfluencers.OfType<TInfluencer>().FirstOrDefault();
            return influencer != null;
        }

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
                foreach (Type influencerType in standbyProps.standbyInfluencers)
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
            foreach (StandbyInfluencerBase influencer in _standbyInfluencers)
            {
                influencer.PostSpawnSetup(respawningAfterLoad);
            }

            foreach (StandbyEffectBase effect in _standbyEffects)
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
            foreach (StandbyInfluencerBase influencer in _standbyInfluencers)
            {
                influencer.ExposeData();
            }

            foreach (StandbyEffectBase effect in _standbyEffects)
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

            foreach (StandbyInfluencerBase influencer in _standbyInfluencers)
            {
                string debugString = influencer.DebugInspectString()?.Trim();
                if (!string.IsNullOrWhiteSpace(debugString))
                {
                    sb.AppendLine($"  {debugString}");
                }
            }

            foreach (StandbyEffectBase effect in _standbyEffects)
            {
                string debugString = effect.DebugInspectString()?.Trim();
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
            foreach (StandbyInfluencerBase influencer in _standbyInfluencers)
            {
                influencer.Dispose();
            }

            foreach (StandbyEffectBase effect in _standbyEffects)
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
            int delayTicks = UsesDelayOff ? GenTicks.SecondsToTicks(LightsOut2Settings.LightDelaySeconds) : 0;
            if (_transition.Tick(WantsToBeInStandby(), delayTicks))
            {
                UpdateStandbyEffects();
            }
        }

        /// <summary>
        /// Determines whether this comp is in standby mode
        /// </summary>
        /// <returns>Whether or not this comp should be in standby</returns>
        public bool WantsToBeInStandby()
        {
            // if any of the influencers are active then we are not in standby mode
            return _standbyInfluencers.All(influencer => influencer.WantsToBeInStandby);
        }

        /// <summary>
        /// Updates the standby effects based on a change in this comp's standby status
        /// </summary>
        private void UpdateStandbyEffects()
        {
            foreach (StandbyEffectBase effect in _standbyEffects)
            {
                effect.UpdateStandby(InStandby);
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
            foreach (StandbyEffectBase effect in _standbyEffects)
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
        private readonly StandbyTransition _transition = new StandbyTransition();

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
