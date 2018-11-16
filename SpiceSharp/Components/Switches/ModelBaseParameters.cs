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
        /// Parameters
        /// </summary>
        [ParameterName("ron"), ParameterInfo("Closed resistance")]
        public GivenParameter<double> OnResistance { get; } = new GivenParameter<double>(1.0);
        [ParameterName("roff"), ParameterInfo("Open resistance")]
        public GivenParameter<double> OffResistance { get; } = new GivenParameter<double>(1.0e12);

        public virtual GivenParameter<double> Threshold { get; } = new GivenParameter<double>();
        public virtual GivenParameter<double> Hysteresis { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the on conductance.
        /// </summary>
        /// <value>
        /// The on conductance.
        /// </value>
        public double OnConductance { get; private set; }

        /// <summary>
        /// Gets the off conductance.
        /// </summary>
        /// <value>
        /// The off conductance.
        /// </value>
        public double OffConductance { get; private set; }

        /// <summary>
        /// Method for calculating the default values of derived parameters.
        /// </summary>
        /// <remarks>
        /// These calculations should be run whenever a parameter has been changed.
        /// </remarks>
        public override void CalculateDefaults()
        {
            // Only positive hysteresis values!
            Hysteresis.RawValue = Math.Abs(Hysteresis.RawValue);

            OnConductance = 1.0 / OnResistance;
            OffConductance = 1.0 / OffResistance;
        }
    }
}
