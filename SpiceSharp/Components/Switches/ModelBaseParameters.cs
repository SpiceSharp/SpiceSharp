using System;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.SwitchBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="CurrentSwitchModel"/>
    /// </summary>
    public class ModelBaseParameters : ParameterSet
    {
        /// <summary>
        /// Gets the resistance parameter when closed.
        /// </summary>
        [ParameterName("ron"), ParameterInfo("Closed resistance")]
        public GivenParameter<double> OnResistance { get; } = new GivenParameter<double>(1.0);

        /// <summary>
        /// Gets the resistance parameter when open.
        /// </summary>
        [ParameterName("roff"), ParameterInfo("Open resistance")]
        public GivenParameter<double> OffResistance { get; } = new GivenParameter<double>(1.0e12);

        /// <summary>
        /// Gets the threshold parameter.
        /// </summary>
        public virtual GivenParameter<double> Threshold { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the hysteresis parameter.
        /// </summary>
        public virtual GivenParameter<double> Hysteresis { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the on conductance.
        /// </summary>
        public double OnConductance { get; private set; }

        /// <summary>
        /// Gets the off conductance.
        /// </summary>
        public double OffConductance { get; private set; }

        /// <summary>
        /// Method for calculating the default values of derived parameters.
        /// </summary>
        public override void CalculateDefaults()
        {
            // Only positive hysteresis values!
            Hysteresis.RawValue = Math.Abs(Hysteresis.RawValue);

            OnConductance = 1.0 / OnResistance;
            OffConductance = 1.0 / OffResistance;
        }
    }
}
