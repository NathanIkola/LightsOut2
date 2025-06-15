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
    public sealed class StandbyComp : ThingComp
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
                        _standbyInfluencers.Add(influencer);
                    }
                    else
                    {
                        LightsOut2Mod.StaticLogger.LogError($"Tried to add a standby influencer of type '{influencerType}' but it does not inherit from StandbyInfluencerBase");
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
            sb.AppendLine($"Standby: {_inStandby}");
            sb.AppendLine($"Multiplier: {_currentMultiplier}");

            foreach(StandbyInfluencerBase influencer in _standbyInfluencers)
            {
                string debugString = influencer.DebugInspectString();
                if (!string.IsNullOrWhiteSpace(debugString))
                {
                    sb.AppendLine(debugString);
                }
            }

            return sb.ToString().Trim();
        }

        /// <summary>
        /// Update the state once per tick
        /// </summary>
        public override void CompTick()
        {
            base.CompTick();

            // update the desired standby state
            bool previouslyDesiredStandby = _desiresStandby;
            _desiresStandby = WantsToBeInStandby();

            // if we're in standby and no longer want to be, then exit standby mode
            if (_inStandby && !_desiresStandby)
            {
                _inStandby = false;
                _ticksUntilStandby = 0;
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
                if (_ticksUntilStandby <= 0) { _inStandby = true; }
            }

            _currentMultiplier = ResourceDrawMultiplier();
        }

        public override void ReceiveCompSignal(string signal)
        {
            base.ReceiveCompSignal(signal);
            LightsOut2Mod.StaticLogger.LogTrace($"Thing {parent} received comp signal: {signal}");
        }

        /// <summary>
        /// Determines whether this comp is in standby mode
        /// </summary>
        /// <returns>Whether or not this comp should be in standby</returns>
        public bool WantsToBeInStandby()
        {
            // if any of the influencers are active then we are not in standby mode
            if (_standbyInfluencers.Any(influencer => influencer.IsActive))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Gets the rate 
        /// </summary>
        /// <returns></returns>
        public float ResourceDrawMultiplier()
        {
            if (StandbyProps.isLight)
            {
                // lights are handled specially, they always use 0% power when in standby
                // and 100% when active, no matter what the settings say
                return _inStandby ? 0f : 1f;
            }
            else
            {
                return _inStandby
                    // in standby mode, use the standby coefficient , but ensure it is at least the minimum value
                    ? Math.Max(
                        LightsOut2Settings.StandbyCoefficientDecimal, 
                        LightsOut2Settings.MinDrawCoefficientDecimal) 
                    // otherwise use the active coefficient
                    : LightsOut2Settings.ActiveCoefficientDecimal;
            }
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
        /// The list of influencers that will determine if this thing is in standby mode
        /// </summary>
        private readonly List<StandbyInfluencerBase> _standbyInfluencers = new List<StandbyInfluencerBase>();
    }
}