using System;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.VoltageSwitchBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="VoltageSwitchModel"/>
    /// </summary>
    public class ModelBaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [ParameterName("ron"), ParameterInfo("Resistance when closed")]
        public GivenParameter<double> OnResistance { get; } = new GivenParameter<double>(1.0);
        [ParameterName("roff"), ParameterInfo("Resistance when off")]
        public GivenParameter<double> OffResistance { get; } = new GivenParameter<double>(1.0e12);
        [ParameterName("vt"), ParameterInfo("Threshold voltage")]
        public GivenParameter<double> Threshold { get; } = new GivenParameter<double>();
        [ParameterName("vh"), ParameterInfo("Hysteresis voltage")]
        public GivenParameter<double> Hysteresis { get; } = new GivenParameter<double>();

        /// <summary>
        /// Method for calculating the default values of derived parameters.
        /// </summary>
        /// <remarks>
        /// These calculations should be run whenever a parameter has been changed.
        /// </remarks>
        public override void CalculateDefaults()
        {
            // Only positive hysteresis values allowed
            Hysteresis.RawValue = Math.Abs(Hysteresis.RawValue);
        }
    }
}
