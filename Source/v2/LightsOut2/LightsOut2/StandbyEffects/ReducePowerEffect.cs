using LightsOut2.Core;
using RimWorld;
using System;
using System.Text;
using Verse;

namespace LightsOut2.StandbyEffects
{
    /// <summary>
    /// A class that modulates power between the configured min and max
    /// </summary>
    public class ReducePowerEffect : StandbyEffectBase
    {
        /// <summary>
        /// The current multiplier for this comp based on the standby state
        /// </summary>
        public float CurrentMultiplier => _currentMultiplier;

        public ReducePowerEffect(ThingWithComps parent)
            : base(parent) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wasInStandby"></param>
        /// <param name="isInStandby"></param>
        protected override void OnStandbyChanged(bool wasInStandby, bool isInStandby) 
        {
            // nothing to do on the change, everything is handled in the tick
        }

        /// <summary>
        /// Updates the power draw on each tick
        /// </summary>
        /// <remarks>
        /// Having this happen tick-wise is sad, but necessary for a few reasons. Particularly,
        /// the power vars may be reset during the tick, so we need to make sure we always restore them
        /// </remarks>
        public override void Tick()
        {
            base.Tick();
            UpdatePowerDraw();
        }

        public override string DebugInspectString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Multiplier: {CurrentMultiplier * 100f}%");

            return sb.ToString().Trim();
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
        /// Gets the coefficient that should be applied to the power draw baed on the current state
        /// </summary>
        /// <returns>The coefficient based on the current state</returns>
        private float ResourceDrawMultiplier()
        {
            return _inStandby 
                // make sure the standby rate stays within the lower bound
                // without clamping this >0, some buildings shown the "no power" icon and break pathing
                ? Math.Max(StandbyDrawCoefficientDecimal(), LightsOut2Settings.MinDrawCoefficientDecimal)
                : ActiveDrawCoefficientDecimal();
        }

        /// <summary>
        /// Retrieves the power rate to apply when in standby
        /// </summary>
        /// <returns>The standby power draw coefficient as a decimal (e.g., 10% = 0.1)</returns>
        protected virtual float StandbyDrawCoefficientDecimal()
        {
            return LightsOut2Settings.StandbyCoefficientDecimal;
        }

        /// <summary>
        /// Retrieves the power rate to apply when in use
        /// </summary>
        /// <returns>The active power draw rate coefficient as a decimal (e.g., 125% = 1.25)</returns>
        protected virtual float ActiveDrawCoefficientDecimal()
        {
            return LightsOut2Settings.ActiveCoefficientDecimal;
        }

        /// <summary>
        /// The current rate multiplier for this comp
        /// </summary>
        private float _currentMultiplier = 0f;

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
                _powerTrader = _parent.TryGetComp<CompPowerTrader>();
                return _powerTrader;
            }
        }
    }
}