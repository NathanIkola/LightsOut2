using LightsOut2.CompProperties;
using LightsOut2.StandbyActuators;
using System;
using LightsOut2.Core.StandbyActuators;
using LightsOut2.Core.StandbyComps;

namespace LightsOut2.ThingComps
{
    /// <summary>
    /// This class allows us to mark certain power-consumers as "on standby"
    /// </summary>
    public class StandbyComp : IStandbyComp
    {
        /// <summary>
        /// Initialize the comp and set up the gizmo
        /// </summary>
        /// <param name="props">The properties for this comp</param>
        public override void Initialize(Verse.CompProperties props)
        {
            base.Initialize(props);
            if (props is CompProperties_Standby standbyProps)
            {
                IsEnabled = standbyProps.startEnabled;
                Type standbyActuatorType = standbyProps.standbyActuatorClass ?? typeof(BenchStandbyActuator);
                StandbyActuator = Activator.CreateInstance(standbyActuatorType) as IStandbyActuator;
            }
        }

        /// <summary>
        /// A function for determining the rate given a specific standby state
        /// </summary>
        /// <param name="isInStandby">Whether or not the thing is in standby</param>
        /// <returns>The rate to modify the power draw by</returns>
        public override float GetRateAsStandbyStatus(bool isInStandby)
        {
            return isInStandby
                    ? LightsOut2Mod.StandbyPowerDrawDecimal
                    : LightsOut2Mod.ActivePowerDrawDecimal;
        }
    }
}